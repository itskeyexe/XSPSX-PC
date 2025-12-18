using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Linq;

namespace XSPSX
{
    public partial class MainMenu : Window
    {
        private readonly List<string> categories = new List<string> { "Users", "Settings", "Media", "Games", "Network", "Friends" };
        private readonly Dictionary<string, List<(string Text, string IconPath)>> categoryData = new Dictionary<string, List<(string Text, string IconPath)>>
{
    { "Users", new List<(string, string)>
        {
            ("itskeyexe", "Resources/Icons/folder.png"),
            ("Dill353", "Resources/Icons/folder.png"),
            ("Tori<3", "Resources/Icons/folder.png"),
            ("log off", "Resources/Icons/folder.png"),
            ("Package Manager", "Resources/Icons/folderStar.png")
        }
    },
    { "Settings", new List<(string, string)>
        {
            ("System Settings", "Resources/Icons/folder.png"),
            ("My System Info", "Resources/Icons/folder.png"),
            ("Display Settings", "Resources/Icons/folder.png"),
            ("XSPSX Themes", "Resources/Icons/folder.png"),
            ("Package Manager", "Resources/Icons/folderStar.png")
        }
    },
    { "Media", new List<(string, string)>
        {
            ("Music Player", "Resources/Icons/folder.png"),
            ("Photo Gallery", "Resources/Icons/folder.png"),
            ("Video Player", "Resources/Icons/folder.png"),
            ("Placeholder1", "Resources/Icons/folder.png"),
            ("Package Manager", "Resources/Icons/folderStar.png")
        }
    },
    { "Games", new List<(string, string)>
        {
            ("Save Data", "Resources/Icons/memCard.png"),
            ("Launch Disk", "Resources/Icons/disk2.png"),
            ("PCSX2", "Resources/Icons/pcsx2Alt.png"),
            ("Games", "Resources/Icons/xspsxGamesIcon.png"),
            ("Package Manager", "Resources/Icons/folderStar.png")
        }
    },
    { "Network", new List<(string, string)>
        {
            ("Sign in", "Resources/Icons/folder.png"),
            ("XSPSX Store", "Resources/Icons/folder.png"),
            ("Web Browser", "Resources/Icons/folder.png"),
            ("Downloads", "Resources/Icons/folder.png"),
            ("Package Manager", "Resources/Icons/folderStar.png")
        }
    },
    { "Friends", new List<(string, string)>
        {
            ("Online:", "Resources/Icons/folder.png"),
            ("Message", "Resources/Icons/folder.png"),
            ("Achievements", "Resources/Icons/folder.png"),
            ("Placeholder1", "Resources/Icons/folder.png"),
            ("Package Manager", "Resources/Icons/folderStar.png")
        }
    }
};

        //xmb
        private int selectedCategoryIndex = 0;
        private int selectedVerticalIndex = 0; // Track the selected vertical item
                                               //media

        private string selectedGame;
        private List<string> availableGames = new List<string> { "Kingdom Hearts II - Final Mix", "Legend of Spyro, The - The Eternal Night (USA)", "CodeBreaker v10.1" };

        private MediaPlayer scrollSoundPlayer = new MediaPlayer();
        //controller

        private MediaPlayer notificationSoundPlayer = new MediaPlayer();
        // Controller
        private SharpXInputHandler xInputHandler; // Use SharpXInputHandler

        private System.Windows.Threading.DispatcherTimer inputTimer; // Timer to poll controller input

        private MediaPlayer backgroundMusicPlayer = new MediaPlayer();

        private MediaPlayer confirmSoundPlayer = new MediaPlayer();

        private System.Timers.Timer spinTimer;
        private bool isSpinning = false; // Tracks if the animation is running

        // Initializer---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public MainMenu()
        {

            // Inside MainMenu constructor
            xInputHandler = new SharpXInputHandler(this);
            // Other initializations...
            this.Activated += (s, e) =>
            {
                if (!inputTimer.IsEnabled)
                {
                    Console.WriteLine("Window activated. Resuming input.");
                    inputTimer.Start();
                    backgroundMusicPlayer.Play();
                }
            };

            this.Background = Brushes.Black; // Ensure no white background
            this.AllowsTransparency = true; // Smooth rendering
            this.WindowStyle = WindowStyle.None; // Optional: Makes transition cleaner
            this.ShowInTaskbar = false; // Prevents flicker from taskbar update


            // Initialize XSPSX File System
            FileSystemManager.InitializeFileSystem();


            // Initialize scroll sound
            try
            {
                scrollSoundPlayer.Open(new Uri("Resources/Sounds/scroll.mp3", UriKind.Relative));
                scrollSoundPlayer.Volume = 0.7;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing scroll sound: {ex.Message}");
            }

            // Play background music
            try
            {
                backgroundMusicPlayer.Open(new Uri("Resources/Sounds/01 Main.mp3", UriKind.Relative));
                backgroundMusicPlayer.Volume = 0.6; // Adjust volume (0.0 to 1.0)
                backgroundMusicPlayer.MediaEnded += BackgroundMusicPlayer_MediaEnded; // Handle looping
                backgroundMusicPlayer.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background music: {ex.Message}");
            }


            // Set up a timer to poll controller input
            inputTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
            };
            inputTimer.Tick += InputTimer_Tick;
            inputTimer.Start();


            InitializeComponent();
            HorizontalStackPanel.RenderTransform = new TranslateTransform(); // Ensure it's initialized
            InitializeHorizontalCategories();
            HighlightSelectedButton(selectedCategoryIndex);
            ScrollToCategory(selectedCategoryIndex, true); // Initialize to "Users"

            // Preload game side menu
            InitializeGameSideMenu();

        }

        private void InitializeGameSideMenu()
        {
            GameListBox.Items.Clear();

            foreach (var game in availableGames)
            {
                GameListBox.Items.Add(new ListBoxItem
                {
                    Content = game,
                    Tag = $@"C:\PCSX2\games\{game}.iso", // Ensure proper path format
                    FontSize = 16,
                    Foreground = Brushes.White,
                    Background = Brushes.Transparent
                });
            }

            // Add the "Close" option at the bottom
            GameListBox.Items.Add(new ListBoxItem
            {
                Content = "Close",
                Tag = "Close",
                FontSize = 16,
                Foreground = Brushes.White,
                Background = Brushes.Transparent
            });
        }


        private void ShowGameSideMenu()
        {
            GameSideMenu.Visibility = Visibility.Visible;

            // Focus on the game list for navigation
            GameListBox.Focus();

            // Reset selection for better navigation
            GameListBox.SelectedItem = null;
        }

        private void HideGameSideMenu()
        {
            GameSideMenu.Visibility = Visibility.Collapsed;

            // Resume XMB navigation
            CategoryScrollViewer.Focus();
        }


        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string selectedContent = selectedItem.Content.ToString();
                string selectedTag = selectedItem.Tag as string;

                // Handle Close option
                if (selectedContent == "Close")
                {
                    HideGameSideMenu();
                    return;
                }

                // Ensure the selected game path is valid
                if (!string.IsNullOrEmpty(selectedTag) && selectedTag != "Close")
                {
                    selectedGame = selectedTag.Trim(); // Ensure no whitespace errors

                    if (!System.IO.File.Exists(selectedGame))
                    {
                        ShowNotification("Invalid Game Path", "The selected game file does not exist.", "Resources/Icons/disk2.png");
                        return;
                    }

                    ShowNotification("Game Loaded", $"{selectedContent} is ready to launch!", "Resources/Icons/disk2.png");

                    // Update "Launch Disk" option to reflect the selected game
                    categoryData["Games"][1] = ("Launch Disk", "Resources/Icons/disk2.png");

                    // Close the side menu
                    HideGameSideMenu();
                }
            }
        }







        public void ShowNotification(string title, string message, string iconPath)
        {
            // Play the notification sound
            try
            {
                notificationSoundPlayer.Open(new Uri("Resources/Sounds/ps4-notification-sound.mp3", UriKind.Relative));
                notificationSoundPlayer.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing notification sound: {ex.Message}");
            }

            // Update UI elements
            NotificationTitle.Text = title;
            NotificationMessage.Text = message;
            NotificationIcon.Source = new BitmapImage(new Uri("Resources/Icons/gamesIcon.png", UriKind.Relative));


            // Show the notification with animation
            NotificationGrid.Visibility = Visibility.Visible;
            var animation = new ThicknessAnimation
            {
                From = new Thickness(320, 20, -320, 0), // Off-screen
                To = new Thickness(20, 20, 20, 0),     // On-screen
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            NotificationGrid.BeginAnimation(MarginProperty, animation);

            // Hide the notification after 3 seconds
            var timer = new System.Timers.Timer(3000);
            timer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    NotificationGrid.Visibility = Visibility.Collapsed;
                });
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }




        private void InputTimer_Tick(object sender, EventArgs e)
        {
            if (!xInputHandler.IsConnected)
            {
                Console.WriteLine("Waiting for controller connection...");
                return;
            }

            xInputHandler.Update(); // Update the controller state

            // D-Pad navigation
            if (xInputHandler.IsDPadLeftPressed())
            {
                NavigateCategories(-1); // Same as Left Arrow
            }
            else if (xInputHandler.IsDPadRightPressed())
            {
                NavigateCategories(1); // Same as Right Arrow
            }
            else if (xInputHandler.IsDPadUpPressed())
            {
                NavigateVerticalOptions(-1); // Same as Up Arrow
            }
            else if (xInputHandler.IsDPadDownPressed())
            {
                NavigateVerticalOptions(1); // Same as Down Arrow
            }

            // Button actions
            if (xInputHandler.IsButtonAPressed())
            {
                ConfirmSelection(); // Perform confirm action
            }
            else if (xInputHandler.IsButtonBPressed())
            {
                CancelSelection(); // Perform cancel action
            }
        }





        private void Category_Click(object sender, RoutedEventArgs e)
        {
            string selectedCategory = (sender as Button)?.Content.ToString();
            Console.WriteLine($"Category clicked: {selectedCategory}");

            // Clear both stacks
            PrimaryVerticalStackPanel.Children.Clear();
            SecondaryVerticalStackPanel.Children.Clear();

            if (categoryData.ContainsKey(selectedCategory))
            {
                // Populate the primary and secondary stacks
                var items = categoryData[selectedCategory];
                for (int i = 0; i < items.Count; i++)
                {
                    var (text, iconPath) = items[i];

                    var button = new Button
                    {
                        Width = 300,
                        Height = 60,
                        Margin = new Thickness(10),
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        Content = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                    {
                        new Image
                        {
                            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Relative)),
                            Width = 40,
                            Height = 40,
                            Margin = new Thickness(10, 0, 10, 0)
                        },
                        new TextBlock
                        {
                            Text = text,
                            FontSize = 20,
                            Foreground = Brushes.White,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                        }
                    };

                    if (i == 0)
                    {
                        PrimaryVerticalStackPanel.Children.Add(button); // Add the first item to the primary stack
                    }
                    else
                    {
                        SecondaryVerticalStackPanel.Children.Add(button); // Add remaining items to the secondary stack
                    }
                }
            }

            selectedVerticalIndex = 0;
            HighlightVerticalOption(selectedVerticalIndex);
            Console.WriteLine($"Added options for category: {selectedCategory}");
        }

  

        private void InitializeHorizontalCategories()
        {
            var icons = new Dictionary<string, string>
            {
                { "Users", "Resources/Icons/userIcon.png" },
                { "Settings", "Resources/Icons/settingsIcon.png" },
                { "Media", "Resources/Icons/mediaIcon.png" },
                { "Games", "Resources/Icons/gamesIcon.png" },
                { "Network", "Resources/Icons/networkIcon.png" },
                { "Friends", "Resources/Icons/friendsIcon.png" }
            };

            // Duplicate the list to allow infinite scrolling
            List<string> infiniteCategories = new List<string>(categories);
            infiniteCategories.InsertRange(0, categories); // Prepend a duplicate
            infiniteCategories.AddRange(categories);       // Append a duplicate

            foreach (var category in infiniteCategories)
            {
                var button = new Button
                {
                    Content = category,
                    Tag = icons[category],
                    Style = this.FindResource("CategoryButtonStyle") as Style,
                    Margin = new Thickness(75) // Ensure consistent margin
                };

                button.Click += Category_Click;
                HorizontalStackPanel.Children.Add(button);
            }
        }



        private void InitializeVerticalItems()
        {
            string selectedCategory = categories[selectedCategoryIndex];
            var items = categoryData[selectedCategory];

            PrimaryVerticalStackPanel.Children.Clear();
            SecondaryVerticalStackPanel.Children.Clear();

            // Populate the Primary Stack with all items
            foreach (var (text, iconPath) in items)
            {
                var button = new Button
                {
                    Width = 400,
                    Height = 100,
                    Margin = new Thickness(0, 20, 0, 20),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children =
                {
                    new Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Relative)),
                        Width = 60,
                        Height = 60,
                        Margin = new Thickness(20, 0, 20, 0)
                    },
                    new TextBlock
                    {
                        Text = text,
                        FontSize = 26,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 250
                    }
                }
                    }
                };

                PrimaryVerticalStackPanel.Children.Add(button); // Add all items to Primary initially
            }

            selectedVerticalIndex = 0;
            HighlightVerticalOption(0);
        }
//End initializer Start Navigation XMB----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void NavigateCategories(int direction)
        {
            selectedCategoryIndex += direction;

            // Handle infinite wrapping logic
            if (selectedCategoryIndex < 0)
            {
                selectedCategoryIndex += categories.Count;
            }
            else if (selectedCategoryIndex >= categories.Count)
            {
                selectedCategoryIndex -= categories.Count;
            }

            // Play scroll sound
            PlayScrollSound();

            // Scroll to align the selected category at the pinned position
            ScrollToCategory(selectedCategoryIndex, false);

            // Highlight the selected category
            HighlightSelectedButton(selectedCategoryIndex);

            // Initialize the vertical items for the selected category
            InitializeVerticalItems();
        }


        private void ScrollToCategory(int selectedIndex, bool instant)
        {
            if (HorizontalStackPanel.ActualWidth == 0)
                return;

            double pinnedPosition = ActualWidth * 0.4;

            var button = HorizontalStackPanel.Children[selectedIndex + categories.Count] as Button;
            double buttonWidth = button.ActualWidth;
            double buttonMargin = button.Margin.Left + button.Margin.Right;
            double totalButtonWidth = buttonWidth + buttonMargin;

            double targetOffset = -(selectedIndex * totalButtonWidth) + pinnedPosition - (buttonWidth / 2);

            var transform = HorizontalStackPanel.RenderTransform as TranslateTransform ?? new TranslateTransform();
            HorizontalStackPanel.RenderTransform = transform;

            var animation = new DoubleAnimation
            {
                From = transform.X,
                To = targetOffset,
                Duration = instant ? TimeSpan.Zero : TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            transform.BeginAnimation(TranslateTransform.XProperty, animation);

            // Update button visibility after scrolling
            UpdateButtonVisibility(selectedIndex);
        }



        private void NavigateVerticalOptions(int direction)
        {
            if (PrimaryVerticalStackPanel.Children.Count == 0)
                return;

            // Adjust the selected index
            selectedVerticalIndex += direction;

            // Handle moving down
            if (direction > 0)
            {
                if (PrimaryVerticalStackPanel.Children.Count > 1) // Ensure at least one item stays in Primary
                {
                    // Move the top item from Primary to the top of Secondary (mirrored)
                    var selectedItem = PrimaryVerticalStackPanel.Children[0];
                    PrimaryVerticalStackPanel.Children.RemoveAt(0);
                    SecondaryVerticalStackPanel.Children.Insert(0, selectedItem); // Add to the top of Secondary
                }

                // Handle infinite wrapping
                if (selectedVerticalIndex >= categoryData[categories[selectedCategoryIndex]].Count)
                {
                    selectedVerticalIndex = 0;
                }
            }
            // Handle moving up
            else if (direction < 0)
            {
                if (SecondaryVerticalStackPanel.Children.Count > 0)
                {
                    // Move the top item from Secondary to the top of Primary (mirrored back)
                    var returningItem = SecondaryVerticalStackPanel.Children[0];
                    SecondaryVerticalStackPanel.Children.RemoveAt(0);
                    PrimaryVerticalStackPanel.Children.Insert(0, returningItem); // Add to the top of Primary
                }

                // Handle infinite wrapping
                if (selectedVerticalIndex < 0)
                {
                    selectedVerticalIndex = categoryData[categories[selectedCategoryIndex]].Count - 1;
                }
            }

            // Highlight the new selection
            HighlightVerticalOption(0);

            // Play the scroll sound
            PlayScrollSound();

            // Trigger the disk spinning animation logic for "Launch Disk"
            HandleLaunchDiskAnimation();
        }




        private void HighlightVerticalOption(int selectedIndex)
        {
            for (int i = 0; i < PrimaryVerticalStackPanel.Children.Count; i++)
            {
                var button = PrimaryVerticalStackPanel.Children[i] as Button;

                if (i == selectedIndex) // Highlight the top item
                {
                    button.RenderTransform = new ScaleTransform(1.5, 1.5);
                    var glow = new DropShadowEffect
                    {
                        Color = Colors.Cyan,
                        BlurRadius = 20,
                        ShadowDepth = 0
                    };
                    button.Effect = glow;
                }
                else
                {
                    button.RenderTransform = new ScaleTransform(1, 1);
                    button.Effect = null;
                }
            }
        }



        private void HighlightSelectedButton(int selectedIndex)
        {
            for (int i = 0; i < HorizontalStackPanel.Children.Count; i++)
            {
                var button = HorizontalStackPanel.Children[i] as Button;

                if (i == selectedIndex)
                {
                    // Enlarge and apply glow effect
                    button.RenderTransform = new ScaleTransform(1.25, 1.25);
                    var glow = new DropShadowEffect
                    {
                        Color = Colors.Cyan,
                        BlurRadius = 15,
                        ShadowDepth = 0
                    };
                    button.Effect = glow;

                    // Glow animation
                    var glowAnimation = new DoubleAnimation
                    {
                        From = 15,
                        To = 25,
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        Duration = TimeSpan.FromSeconds(0.5)
                    };
                    glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnimation);
                }
                else
                {
                    // Reset non-selected buttons
                    button.RenderTransform = new ScaleTransform(1, 1);
                    button.Effect = null;
                }
            }
        }

        private void HandleLaunchDiskAnimation()
        {
            string currentOption = categoryData[categories[selectedCategoryIndex]][selectedVerticalIndex].Text;

            if (currentOption == "Launch Disk")
            {
                // Start the fade-in animation for the game background
                FadeInGameBackground();

                // Start a timer to trigger the spin animation after 1.5 seconds
                if (spinTimer != null)
                {
                    spinTimer.Stop();
                    spinTimer.Dispose();
                }

                spinTimer = new System.Timers.Timer(1500); // Reduced delay
                spinTimer.Elapsed += (s, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        StartDiskSpinAnimation();
                    });
                };
                spinTimer.Start();
            }
            else
            {
                // Stop the spinning animation if the selection changes
                StopDiskSpinAnimation();

                // Start the fade-out animation
                FadeOutGameBackground();
            }
        }

        private void FadeInGameBackground()
        {
            var fadeIn = new DoubleAnimation
            {
                From = GameBackground.Opacity,
                To = 1, // Fully visible
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var zoomIn = new DoubleAnimation
            {
                From = 1.2,
                To = 1.0, // Normal size
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            GameBackground.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            ((ScaleTransform)GameBackground.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, zoomIn);
            ((ScaleTransform)GameBackground.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, zoomIn);
        }

        private void FadeOutGameBackground()
        {
            var fadeOut = new DoubleAnimation
            {
                From = GameBackground.Opacity,
                To = 0, // Fully hidden
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var zoomOut = new DoubleAnimation
            {
                From = 1.0,
                To = 1.2, // Zoomed slightly out
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            GameBackground.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            ((ScaleTransform)GameBackground.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, zoomOut);
            ((ScaleTransform)GameBackground.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, zoomOut);
        }



        private void StartDiskSpinAnimation()
        {
            if (isSpinning)
                return;

            isSpinning = true;

            // Find the "Launch Disk" button
            var launchDiskButton = PrimaryVerticalStackPanel.Children.Cast<Button>()
                .FirstOrDefault(b => ((TextBlock)((StackPanel)b.Content).Children[1]).Text == "Launch Disk");

            if (launchDiskButton != null)
            {
                var icon = ((StackPanel)launchDiskButton.Content).Children[0] as Image;

                if (icon != null)
                {
                    var rotateTransform = new RotateTransform();
                    icon.RenderTransform = rotateTransform;
                    icon.RenderTransformOrigin = new Point(0.5, 0.5);

                    var rotateAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 360,
                        Duration = TimeSpan.FromSeconds(0.1),
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                }
            }
        }

        private void StopDiskSpinAnimation()
        {
            if (!isSpinning)
                return;

            isSpinning = false;

            if (spinTimer != null)
            {
                spinTimer.Stop();
                spinTimer.Dispose();
            }

            // Find the "Launch Disk" button
            var launchDiskButton = PrimaryVerticalStackPanel.Children.Cast<Button>()
                .FirstOrDefault(b => ((TextBlock)((StackPanel)b.Content).Children[1]).Text == "Launch Disk");

            if (launchDiskButton != null)
            {
                var icon = ((StackPanel)launchDiskButton.Content).Children[0] as Image;

                if (icon != null)
                {
                    var rotateTransform = icon.RenderTransform as RotateTransform;
                    if (rotateTransform != null)
                    {
                        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null); // Stop animation
                    }
                }
            }
        }


        private void UpdateButtonVisibility(int selectedIndex)
        {
            // Define ranges for the three sets
            int prependStartIndex = 0;
            int prependEndIndex = categories.Count - 1; // Prepend set
            int middleStartIndex = categories.Count;
            int middleEndIndex = middleStartIndex + categories.Count - 1; // Middle set
            int appendStartIndex = middleEndIndex + 1;
            int appendEndIndex = appendStartIndex + categories.Count - 1; // Append set

            Console.WriteLine($"Prepend: {prependStartIndex}-{prependEndIndex}");
            Console.WriteLine($"Middle: {middleStartIndex}-{middleEndIndex}");
            Console.WriteLine($"Append: {appendStartIndex}-{appendEndIndex}");

            // Iterate over all buttons in HorizontalStackPanel
            for (int i = 0; i < HorizontalStackPanel.Children.Count; i++)
            {
                var button = HorizontalStackPanel.Children[i] as Button;

                if (button == null) continue;

                // Only show buttons in the prepend set
                if (i >= prependStartIndex && i <= prependEndIndex)
                {
                    button.Visibility = Visibility.Visible; // Show prepend set
                    Console.WriteLine($"Button {i} ({button.Content}) is Visible");
                }
                else
                {
                    button.Visibility = Visibility.Hidden; // Hide middle and append sets
                    Console.WriteLine($"Button {i} ({button.Content}) is Hidden");
                }
            }
        }


        private void PlayScrollSound()
        {
            try
            {
                // Reset position and play the preloaded sound
                scrollSoundPlayer.Position = TimeSpan.Zero;
                scrollSoundPlayer.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing scroll sound: {ex.Message}");
            }
        }

        private void BackgroundMusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Reset position to the start and play again
            backgroundMusicPlayer.Position = TimeSpan.Zero;
            backgroundMusicPlayer.Play();
        }

        private void PlayConfirmSound()
        {
            try
            {
                confirmSoundPlayer.Open(new Uri("Resources/Sounds/04. Src11 Decide Refine D.mp3", UriKind.Relative));
                confirmSoundPlayer.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing confirmation sound: {ex.Message}");
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Make the primary vertical stack visible
            PrimaryScrollViewer.Visibility = Visibility.Visible;

            // Initialize horizontal navigation
            ScrollToCategory(selectedCategoryIndex, true);
            UpdateButtonVisibility(selectedCategoryIndex);
            HighlightSelectedButton(selectedCategoryIndex);

            // Initialize vertical items for the default category
            InitializeVerticalItems();

            // Play the wave background
            WaveBackground.Play();
        }


        private void WaveBackground_MediaEnded(object sender, RoutedEventArgs e)
        {
            WaveBackground.Position = TimeSpan.Zero;
            WaveBackground.Play();


        }
        //Inputs for Nav keyboard and Controller----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void ControllerTimer_Tick(object sender, EventArgs e)
        {
            // Update controller input
            xInputHandler.Update();

            // Handle navigation based on controller input
            if (!xInputHandler.IsDPadLeftPressed())
            {
                if (xInputHandler.IsDPadRightPressed())
                {
                    NavigateCategories(1);
                }
                else if (xInputHandler.IsDPadUpPressed())
                {
                    NavigateVerticalOptions(-1);
                }
                else if (xInputHandler.IsDPadDownPressed())
                {
                    NavigateVerticalOptions(1);
                }
            }
            else
            {
                NavigateCategories(-1);
            }

            if (xInputHandler.IsButtonAPressed())
            {
                ConfirmSelection();
            }
            else if (xInputHandler.IsButtonBPressed())
            {
                CancelSelection();
            }
        }



        private async void ConfirmSelection()
        {
            // Play confirmation sound
            PlayConfirmSound();

            string currentCategory = categories[selectedCategoryIndex];
            string currentOption = categoryData[currentCategory][selectedVerticalIndex].Text;

            try
            {
                if (currentCategory == "Games")
                {
                    string pcsx2Path = @"C:\PCSX2\pcsx2-qt.exe"; // Path to PCSX2 executable
                    PCSX2Launcher pcsx2Launcher = new PCSX2Launcher(pcsx2Path);

                    if (currentOption == "Games") // Open the game side menu
                    {
                        ShowGameSideMenu();
                    }
                    else if (currentOption == "PCSX2") // Launch PCSX2
                    {
                        ShowNotification("Launching PCSX2", "Preparing to boot the emulator...", "Resources/Icons/controller.png");

                        // Pause input and mute background audio
                        inputTimer.Stop();
                        backgroundMusicPlayer.Pause();

                        // Add a 2-second delay
                        await Task.Delay(2000);

                        // Launch PCSX2 without arguments
                        pcsx2Launcher.LaunchPCSX2(null, () =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                inputTimer.Start();
                                backgroundMusicPlayer.Play();
                                ShowNotification("PCSX2 Closed", "Returned to the main menu.", "Resources/Icons/controller.png");
                            });
                        });
                    }
                    else if (currentOption == "Launch Disk") // Launch the selected game
                    {
                        if (string.IsNullOrEmpty(selectedGame))
                        {
                            ShowNotification("No Game Selected", "Please select a game from the library.", "Resources/Icons/disk2.png");
                            return;
                        }

                        string isoPath = System.IO.Path.GetFullPath(selectedGame);
                        Console.WriteLine($"Attempting to launch: {isoPath}");

                        if (!System.IO.File.Exists(isoPath))
                        {
                            ShowNotification("Game Not Found", $"File not found: {isoPath}", "Resources/Icons/disk2.png");
                            Console.WriteLine($"ERROR: File does not exist: {isoPath}");
                            return;
                        }

                        SetGameBackground(selectedGame);

                        ShowNotification("Launching Game", $"Starting {System.IO.Path.GetFileNameWithoutExtension(isoPath)}...", "Resources/Icons/disk2.png");

                        // Pause input and mute background audio
                        inputTimer.Stop();
                        backgroundMusicPlayer.Pause();

                        // Add a 2-second delay
                        await Task.Delay(2000);

                        pcsx2Launcher.LaunchPCSX2(isoPath, () =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                inputTimer.Start();
                                backgroundMusicPlayer.Play();
                                ShowNotification("Game Closed", "Returned to the main menu.", "Resources/Icons/disk2.png");
                            });
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Confirmed selection: {currentOption}");
                    }
                }
                else if (currentCategory == "Network" && currentOption == "Web Browser")
                {
                    ShowNotification("Opening Web Browser", "Loading XSPSX Injector Website...", "Resources/Icons/browser.png");

                    // Pause input and mute background audio
                    inputTimer.Stop();
                    backgroundMusicPlayer.Pause();

                    // Open the Web Browser Window
                    Dispatcher.Invoke(() =>
                    {
                        WebBrowserWindow webBrowser = new WebBrowserWindow();
                        webBrowser.ShowDialog(); // Waits for browser window to close
                    });

                    // Resume input and background audio after closing the browser
                    inputTimer.Start();
                    backgroundMusicPlayer.Play();
                    ShowNotification("Web Browser Closed", "Returned to the main menu.", "Resources/Icons/browser.png");
                }
                else
                {
                    Console.WriteLine($"Confirmed selection: {currentOption}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to process selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void SetGameBackground(string gamePath)
        {
            string gameName = System.IO.Path.GetFileNameWithoutExtension(gamePath);
            string backgroundPath = $"Resources/GameBackgrounds/{gameName}.png";

            if (System.IO.File.Exists(backgroundPath))
            {
                GameBackground.Source = new BitmapImage(new Uri(backgroundPath, UriKind.Relative));
            }
            else
            {
                GameBackground.Source = new BitmapImage(new Uri("Resources/GameBackgrounds/default.png", UriKind.Relative)); // Default background
            }
        }






        private void CancelSelection()
        {
            Console.WriteLine("Cancelled selection.");
            // Add logic to cancel and go back
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.A)
            {
                NavigateCategories(-1);
            }
            else if (e.Key == Key.Right || e.Key == Key.D)
            {
                NavigateCategories(1);
            }
            else if (e.Key == Key.Up || e.Key == Key.W)
            {
                NavigateVerticalOptions(-1);
            }
            else if (e.Key == Key.Down || e.Key == Key.S)
            {
                NavigateVerticalOptions(1);
            }
        }
    }
}