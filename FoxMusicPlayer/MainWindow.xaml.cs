using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace FoxMusicPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MusicListView.SelectedIndex = 0;
            Player.MediaEnded += Player_MediaEnded;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private bool IsPlaying = false;
        private bool Loop = false;
        private bool userIsDraggingSlider = false;

        List<string> paths = new List<string>();

        public class MusicData
        {

            public int Num { get; set; }
            public string Name { get; set; }
            public string Duration { get; set; }
        }
        private void BtnOpen(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg)|*.mp3;*.mpg;*.mpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    string trackName = Path.GetFileNameWithoutExtension(file);
                    if (paths.Any(p => Path.GetFileNameWithoutExtension(p) == trackName))
                    {
                        continue;
                    }

                    paths.Add(Path.GetFullPath(file));

                    MusicData data = new MusicData
                    {
                        Num = paths.Count,
                        Name = trackName,
                        Duration = GetDuration(file)
                    };
                    MusicListView.Items.Add(data);
                }
            }
            Player.Source = new Uri(paths[MusicListView.SelectedIndex]);
            Player.LoadedBehavior = MediaState.Pause;
        }
        private string GetDuration(string filePath)
        {
            try
            {
                using (var reader = new AudioFileReader(filePath))
                {
                    return reader.TotalTime.ToString(@"mm\:ss");
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = Player.Position.TotalSeconds;
            }
        }
        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                await Task.Delay(500);
                Player.LoadedBehavior = MediaState.Pause;
                PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                IsPlaying = false;

            }
            else
            {
                if (paths.Count == 0)
                {

                }
                else
                {
                    await Task.Delay(500);
                    Player.LoadedBehavior = MediaState.Play;
                    IsPlaying = true;
                    PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                }
            }
        }
        private void BtnExit(object sender, EventArgs e)
        {
            if (MessageBox.Show("You Want Close Application?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                //do no stuff
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        private void BtnNext(object sender, EventArgs e)
        {
            if (IsPlaying)
            {
                MusicListView.SelectedIndex = (MusicListView.SelectedIndex + 1) % paths.Count;
                Player.Source = new Uri(paths[MusicListView.SelectedIndex]);
                Player.LoadedBehavior = MediaState.Play;
                IsPlaying = true;
            }
            else
            {

            }

        }
        private void BtnPrevious(object sender, EventArgs e)
        {
            if (IsPlaying)
            {
                if (MusicListView.SelectedIndex < 0)
                {
                    MusicListView.SelectedIndex = paths.Count - 1;
                }
                else
                    MusicListView.SelectedIndex = (MusicListView.SelectedIndex - 1 + paths.Count) % paths.Count;
                Player.Source = new Uri(paths[MusicListView.SelectedIndex]);
                Player.LoadedBehavior = MediaState.Play;
                IsPlaying = true;
            }
            else
            {

            }
        }
        private void LoopButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoopButton.Background == null)
            {
                LoopButton.Background = new SolidColorBrush(Color.FromRgb(0x7F, 0x7F, 0x7F));
                Loop = true;
            }
            else
            {
                LoopButton.Background = null;
                Loop = false;
            }
        }
        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (Loop == false)
            {
                MusicListView.SelectedIndex = (MusicListView.SelectedIndex + 1) % paths.Count;
                Player.Source = new Uri(paths[MusicListView.SelectedIndex]);
                Player.LoadedBehavior = MediaState.Play;
                IsPlaying = true;
                PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
            else
            {
                MusicListView.SelectedIndex = (MusicListView.SelectedIndex);
                Player.Source = new Uri(paths[MusicListView.SelectedIndex]);
                Player.LoadedBehavior = MediaState.Play;
                IsPlaying = true;
                PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            Player.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Volume > 0)
            {
                Player.Volume = 0;
                VolumeButton.Content = new PackIcon
                {
                    Kind = PackIconKind.VolumeOff,
                    Width = 20,
                    Height = 20,
                    Foreground = new LinearGradientBrush
                    {
                        EndPoint = new Point(0.5, 1),
                        MappingMode = BrushMappingMode.RelativeToBoundingBox,
                        StartPoint = new Point(0.5, 0),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop { Color = Color.FromRgb(214, 144, 22), Offset = 0 },
                            new GradientStop { Color = Color.FromRgb(214, 81, 30), Offset = 0.747 },
                            new GradientStop { Color = Color.FromRgb(155, 51, 13), Offset = 0.807 }
                        }
                    }
                };
            }
            else
            {
                Player.Volume = 1;
                VolumeButton.Content = new PackIcon
                {
                    Kind = PackIconKind.VolumeHigh,
                    Width = 20,
                    Height = 20,
                    Foreground = new LinearGradientBrush
                    {
                        EndPoint = new Point(0.5, 1),
                        MappingMode = BrushMappingMode.RelativeToBoundingBox,
                        StartPoint = new Point(0.5, 0),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop { Color = Color.FromRgb(214, 144, 22), Offset = 0 },
                            new GradientStop { Color = Color.FromRgb(214, 81, 30), Offset = 0.747 },
                            new GradientStop { Color = Color.FromRgb(155, 51, 13), Offset = 0.807 }
                        }
                    }
                };
            }
        }
    }
}
