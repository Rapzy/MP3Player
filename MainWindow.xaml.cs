using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using WMPLib;

namespace Course_Project_WPF
{
    public partial class MainWindow : Window
    {
        WindowsMediaPlayer Player = new WindowsMediaPlayer();
        bool isRepeat = false;
        int CurrentMediaIndex;
        public MainWindow()
        {
            InitializeComponent();
            Player.settings.autoStart = false;
            Player.PlayStateChange += Player_PlayStateChange;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(Player.playState == WMPPlayState.wmppsReady)
            {
                PlayerToggle();
                (sender as System.Windows.Threading.DispatcherTimer).Stop();
            }
        }
        private void Player_PlayStateChange(int NewState)
        {
           // MessageBox.Show(NewState.ToString());
            if(NewState == 3) { SongNameLabel.Content = String.Join(", ",Playlist[CurrentMediaIndex].Performers) + " - " + Playlist[CurrentMediaIndex].Title; }
            if (NewState == 8)
            {
                if (!Player.settings.getMode("loop"))
                {
                    PathGeometry pg = new PathGeometry();
                    pg.FillRule = FillRule.Nonzero;
                    PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                    pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M8 5v14l11-7z");
                    ToggleButtonPath.Data = pg;
                    ((Path)FindName("PlayTrackButton" + CurrentMediaIndex.ToString() + "Path")).Data = pg;
                    Player.controls.stop();
                    if (isRepeat)
                    {
                        if (CurrentMediaIndex + 1 == Playlist.Count)
                        {
                            CurrentMediaIndex = 0;
                            Player.currentMedia = Playlist[CurrentMediaIndex].Media;
                        }
                        else
                        {
                            CurrentMediaIndex++;
                            Player.currentMedia = Playlist[CurrentMediaIndex].Media;
                        }
                    }
                    else
                    {
                        if (CurrentMediaIndex + 1 != Playlist.Count)
                        {
                            CurrentMediaIndex++;
                            Player.currentMedia = Playlist[CurrentMediaIndex].Media;
                        }
                    }
                    System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                    dispatcherTimer.Start();
                }
            }
        }
        private void PlayerToggle()
        {
            if (Player.playState != WMPPlayState.wmppsPlaying)
            {
                Player.controls.play();
                PathGeometry pg = new PathGeometry();
                pg.FillRule = FillRule.Nonzero;
                PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M6 19h4V5H6v14zm8-14v14h4V5h-4z");
                ToggleButtonPath.Data = pg;
                ((Path)FindName("PlayTrackButton" + CurrentMediaIndex.ToString() + "Path")).Data = pg;
            }
            else
            {
                Player.controls.pause();
                PathGeometry pg = new PathGeometry();
                pg.FillRule = FillRule.Nonzero;
                PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M8 5v14l11-7z"); 
                ToggleButtonPath.Data = pg;
                ((Path)FindName("PlayTrackButton" + CurrentMediaIndex.ToString() + "Path")).Data = pg;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if(Playlist.Count > 0)
                PlayerToggle();
            else
            {
                PlaylistButton_Click(PlaylistButton, e);
            }
        }
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            if (!isRepeat && !Player.settings.getMode("loop"))
            {
                but.MouseLeave -= Button_MouseLeave;
                ((Path)but.FindName(but.Name + "Path")).Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                PathGeometry pg = new PathGeometry();
                pg.FillRule = FillRule.Nonzero;
                PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M7 7h10v3l4-4-4-4v3H5v6h2V7zm10 10H7v-3l-4 4 4 4v-3h12v-6h-2v4zm-4-2V9h-1l-2 1v1h1.5v4H13z");
                ((Path)but.FindName(but.Name + "Path")).Data = pg;
                Player.settings.setMode("loop", true);
            }
            else
            {
                Player.settings.setMode("loop", false);
                if (!isRepeat)
                {
                    PathGeometry pg = new PathGeometry();
                    pg.FillRule = FillRule.Nonzero;
                    PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                    pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M7 7h10v3l4-4-4-4v3H5v6h2V7zm10 10H7v-3l-4 4 4 4v-3h12v-6h-2v4z");
                    ((Path)but.FindName(but.Name + "Path")).Data = pg;
                    isRepeat = true;
                }
                else
                {
                    isRepeat = false;
                    but.MouseLeave += Button_MouseLeave;
                    ((Path)but.FindName(but.Name + "Path")).Fill = new SolidColorBrush(Color.FromRgb(98, 114, 123));
                }
            }
        }
        List<Track> Playlist = new List<Track>();
        class Track
        {
            public IWMPMedia Media { get; set; }
            public string Title { get; set; }
            public string[] Performers { get; set; }
            public string Duration { get; set; }
        }
        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.DefaultExt = ".mp3";
            ofd.Filter = "Audio files (*.mp3)|*.mp3";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (ofd.ShowDialog() == true)
            {
                //Playlist.Clear();
                foreach (string filename in ofd.FileNames) {
                    var mp3File = TagLib.File.Create(filename);
                    Playlist.Add(new Track {
                        Media = Player.newMedia(filename),
                        Title = mp3File.Tag.Title,
                        Performers = mp3File.Tag.Performers,
                        Duration = mp3File.Properties.Duration.ToString("mm\\:ss")
                    });
                    Console.WriteLine("Artist: " + String.Join(", ", mp3File.Tag.Performers));
                    Console.WriteLine("Track number: " + mp3File.Tag.Track);
                    Console.WriteLine("Title: " + mp3File.Tag.Title);
                    Console.WriteLine("Album: " + mp3File.Tag.Album);
                    Console.WriteLine("Year: " + mp3File.Tag.Year);
                    Console.WriteLine("Genre: " + mp3File.Tag.FirstGenre);
                    Console.WriteLine("Bitrate: " + mp3File.Properties.AudioBitrate + " kbps");
                    Console.WriteLine("Channels: " + mp3File.Properties.AudioChannels);
                    Console.WriteLine("Duration: " + mp3File.Properties.Duration.ToString("mm\\:ss"));
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(50);
                    Style st = FindResource("PlayerButton") as Style;
                    PlaylistGrid.RowDefinitions.Add(gridRow);
                    Button TrackPlayButton = new Button() {
                        Name = "PlayTrackButton"+ (Playlist.Count - 1).ToString(),
                        Style = st,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Background = Brushes.Transparent,
                        Height = 40,
                        Width = 40,
                        Cursor = Cursors.Hand,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(10,0,10,0),
                        };
                    TrackPlayButton.MouseEnter += TrackPlayButton_MouseEnter;
                    TrackPlayButton.MouseLeave += TrackPlayButton_MouseLeave;
                    TrackPlayButton.Click += TrackPlayButton_Click;
                    PathGeometry pg = new PathGeometry();
                    pg.FillRule = FillRule.Nonzero;
                    PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                    pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M8 5v14l11-7z");
                    Path ButtonPath = new Path()
                    {
                        Name = "PlayTrackButton" + (Playlist.Count - 1).ToString()+"Path",
                        Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Data = pg,
                        Stretch = Stretch.Fill,
                        Height=15,
                        Width=11,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid PathGrid = new Grid()
                    {
                        Width = 10,
                        Height = 15,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center   
                    };
                    Label TrackName = new Label()
                    {
                        Content = String.Join(", ",mp3File.Tag.Performers)+" - " + mp3File.Tag.Title,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(60, 0, 0, 0),
                    };
                    PathGrid.Children.Add(ButtonPath);
                    TrackPlayButton.Content = PathGrid;
                    Border Container = new Border() {
                        Height = 50,
                        Background = Brushes.Transparent,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(55, 70, 79)),
                        BorderThickness = new Thickness(0,0,0,2),
                        Child = TrackPlayButton
                    };
                    
                    Grid.SetRow(Container, Playlist.Count - 1);
                    Grid.SetColumn(Container, 0);
                    Grid.SetRow(TrackName, Playlist.Count - 1);
                    Grid.SetColumn(TrackName, 0);
                    PlaylistGrid.Children.Add(Container);
                    PlaylistGrid.Children.Add(TrackName);
                    if (PlaylistGrid.FindName(ButtonPath.Name) == null) { PlaylistGrid.RegisterName(ButtonPath.Name, ButtonPath); }
                    if (PlaylistScroll.Height < 250) { PlaylistScroll.Height += 50; }

                }
            }
            Player.controls.stop();
            if (Playlist.Count > 0)
            {
                Player.currentMedia = Playlist[0].Media;
                CurrentMediaIndex = 0;
                PlayerToggle();
            }
        }

        private void TrackPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button; //("M8 5v14l11-7z"); M6 19h4V5H6v14zm8 - 14v14h4V5h - 4z
            if (CurrentMediaIndex == int.Parse(but.Name.Substring("PlayTrackButton".Length)))
            {
                PlayerToggle();
            }
            else { 
                PathGeometry pg = new PathGeometry();
                pg.FillRule = FillRule.Nonzero;
                PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
                pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M8 5v14l11-7z");
                ((Path)FindName("PlayTrackButton" + CurrentMediaIndex.ToString() + "Path")).Data = pg;
                Player.controls.stop();
                CurrentMediaIndex = int.Parse(but.Name.Substring("PlayTrackButton".Length));
                Player.currentMedia = Playlist[CurrentMediaIndex].Media;
                PlayerToggle();
            }
        }

        private void TrackPlayButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button but = sender as Button;
            but.Background = Brushes.Transparent;
            ((Path)but.FindName(but.Name + "Path")).Fill = Brushes.White;
        }

        private void TrackPlayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button but = sender as Button;
            but.Background = Brushes.White;
            ((Path)but.FindName(but.Name + "Path")).Fill = new SolidColorBrush(Color.FromRgb(98, 114, 123));
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button but = sender as Button;
            but.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            ((Path)but.FindName(but.Name + "Path")).Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button but = sender as Button;
            but.Foreground = new SolidColorBrush(Color.FromRgb(98, 114, 123));
            ((Path)but.FindName(but.Name + "Path")).Fill = new SolidColorBrush(Color.FromRgb(98, 114, 123));
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PathGeometry pg = new PathGeometry();
            pg.FillRule = FillRule.Nonzero;
            PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
            pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M8 5v14l11-7z");
            ((Path)FindName("PlayTrackButton" + CurrentMediaIndex.ToString() + "Path")).Data = pg;
            Player.controls.stop();
            if (CurrentMediaIndex + 1 == Playlist.Count)
            {
                CurrentMediaIndex = 0;
                Player.URL = Playlist.First().Media.sourceURL;
            }
            else
            {
                CurrentMediaIndex++;
                Player.URL = Playlist[CurrentMediaIndex].Media.sourceURL;
            }
            PlayerToggle();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            PathGeometry pg = new PathGeometry();
            pg.FillRule = FillRule.Nonzero;
            PathFigureCollectionConverter pfcc = new PathFigureCollectionConverter();
            pg.Figures = (PathFigureCollection)pfcc.ConvertFrom("M8 5v14l11-7z");
            ((Path)FindName("PlayTrackButton" + CurrentMediaIndex.ToString() + "Path")).Data = pg;
            Player.controls.stop();
            if (CurrentMediaIndex == 0)
            {
                CurrentMediaIndex = Playlist.Count-1;
                Player.currentMedia = Playlist.Last().Media;
            }
            else
            {
                CurrentMediaIndex--;
                Player.currentMedia = Playlist[CurrentMediaIndex].Media;
            }
            PlayerToggle();
        }
        private void TopPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ButtonClose_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Path)FindName("ButtonClosePath")).Fill = Brushes.White;
        }

        private void ButtonClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}