using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace XSPSX
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Ensure both video and audio are ready to play
            BootVideo.MediaOpened += (s, e) =>
            {
                BootSound.Play();
                BootVideo.Play();
            };

            // Start the video
            BootVideo.LoadedBehavior = MediaState.Manual;
            BootSound.LoadedBehavior = MediaState.Manual;
            BootVideo.Play();
        }

        private void BootVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Stop the boot sound if still playing
            if (BootSound.CanPause)
            {
                BootSound.Stop();
            }

            // Show the warning message and blur effect
            WarningMessage.Visibility = Visibility.Visible;
            BlurredBackground.Visibility = Visibility.Visible;

            // Play the warning sound
            MediaPlayer warningSoundPlayer = new MediaPlayer();
            warningSoundPlayer.MediaOpened += WarningSoundPlayer_MediaOpened;
            warningSoundPlayer.Open(new Uri("Resources/Sounds/11 - SND System Ok.mp3", UriKind.Relative));
            warningSoundPlayer.Volume = 1.0; // Ensure the volume is set to maximum

            // Delay for 3 seconds
            Task.Delay(3000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    // Create fade-out animations
                    var fadeOutWarning = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(1)
                    };

                    var fadeOutBlur = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(1)
                    };

                    // Create a Storyboard
                    var storyboard = new Storyboard();
                    storyboard.Children.Add(fadeOutWarning);
                    storyboard.Children.Add(fadeOutBlur);

                    // Set target properties for animations
                    Storyboard.SetTarget(fadeOutWarning, WarningMessage);
                    Storyboard.SetTargetProperty(fadeOutWarning, new PropertyPath(UIElement.OpacityProperty));

                    Storyboard.SetTarget(fadeOutBlur, BlurredBackground);
                    Storyboard.SetTargetProperty(fadeOutBlur, new PropertyPath(UIElement.OpacityProperty));

                    // Attach a named Completed handler
                    storyboard.Completed += Storyboard_Completed;

                    // Start the Storyboard
                    storyboard.Begin();
                });
            });
        }

        private void WarningSoundPlayer_MediaOpened(object sender, EventArgs e)
        {
            // Play the warning sound when media is loaded
            var player = sender as MediaPlayer;
            player?.Play();
        }



        private void Storyboard_Completed(object sender, EventArgs e)
        {
            // Hide the elements after fade-out
            WarningMessage.Visibility = Visibility.Hidden;
            BlurredBackground.Visibility = Visibility.Hidden;

            // Load the main menu
            LoadMainMenu();
        }




        private void LoadMainMenu()
        {
            MainMenu mainMenu = new MainMenu();
            mainMenu.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainMenu.Opacity = 0; // Start fully transparent
            mainMenu.Show();

            // Fade-in animation for MainMenu
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1)
            };

            fadeIn.Completed += (s, e) =>
            {
                // After MainMenu is fully visible, fade out MainWindow
                var fadeOut = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeOut.Completed += (s2, e2) =>
                {
                    this.Hide(); // Hide MainWindow instead of closing immediately
                    Task.Delay(200).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => this.Close()); // Close after slight delay to prevent flicker
                    });
                };

                this.BeginAnimation(Window.OpacityProperty, fadeOut);
            };

            mainMenu.BeginAnimation(Window.OpacityProperty, fadeIn);
        }
    }
    }
