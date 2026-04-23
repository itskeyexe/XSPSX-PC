using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;


namespace XSPSX
{
    public partial class SystemUpdateWindow : Window
    {
        private SharpXInputHandler xInput;
        private DispatcherTimer inputTimer;
        private bool isUpdating = false;
        private bool isExploitPath = false;
        private string selectedUpdatePath = string.Empty;
        private Random rng = new Random();

        public SystemUpdateWindow()
        {
            InitializeComponent();

            try
            {
                // Passing null to prevent the MainMenu crash; handler must check for null
                xInput = new SharpXInputHandler(null);
            }
            catch (Exception)
            {
                // Silent fail if controller initialization fails
            }

            inputTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            inputTimer.Tick += InputTimer_Tick;
            inputTimer.Start();

            this.Loaded += SystemUpdateWindow_Loaded;
            CheckForUpdateFiles();
        }

        private void SystemUpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WaveBackground.Play();
            UpdateBtn.Focus();
        }

        private void WaveBackground_MediaEnded(object sender, RoutedEventArgs e)
        {
            WaveBackground.Position = TimeSpan.Zero;
            WaveBackground.Play();
        }

        private void PlayUISound(string fileName)
        {
            try
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Sounds", fileName), UriKind.Absolute));
                player.Play();
            }
            catch { /* Ignore if file is missing */ }
        }

        private void CheckForUpdateFiles()
        {
            string updateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates");
            if (!Directory.Exists(updateDir)) Directory.CreateDirectory(updateDir);

            var files = Directory.GetFiles(updateDir, "*.upd");

            if (files.Length > 0)
            {
                // Update found: Show the panel and set the text
                ConfirmationPanel.Visibility = Visibility.Visible;
                UpdateStatusHeader.Text = "Latest update data was found.";

                selectedUpdatePath = files[0];
                string fileName = Path.GetFileName(selectedUpdatePath);
                UpdateFileName.Text = fileName;

                UpdateBtn.IsEnabled = true;
                UpdateBtn.Opacity = 1.0;
                UpdateBtn.Focus();

                if (fileName.Contains("HFW") || fileName.Contains("CFW") || fileName.ToLower().Contains("jailbreak"))
                {
                    isExploitPath = true;
                }
            }
            else
            {
                // No update found: Keep panel collapsed or show an error state
                ConfirmationPanel.Visibility = Visibility.Visible; // Show it to display the error
                UpdateStatusHeader.Text = "No update data was found.";
                UpdateFileName.Text = "Please insert storage media containing update files.";

                UpdateBtn.IsEnabled = false;
                UpdateBtn.Opacity = 0.5;
                CancelBtn.Focus();
            }
        }

        private void InputTimer_Tick(object sender, EventArgs e)
        {
            if (xInput == null || !xInput.IsConnected || isUpdating) return;

            xInput.Update();

            if (xInput.IsDPadDownPressed() || xInput.IsDPadUpPressed())
            {
                if (UpdateBtn.IsFocused) CancelBtn.Focus();
                else UpdateBtn.Focus();
            }

            if (xInput.IsButtonAPressed())
            {
                if (UpdateBtn.IsFocused && UpdateBtn.IsEnabled) StartUpdate_Click(null, null);
                else if (CancelBtn.IsFocused) CancelUpdate_Click(null, null);
            }

            if (xInput.IsButtonBPressed()) CancelUpdate_Click(null, null);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isUpdating) return;

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (UpdateBtn.IsFocused) CancelBtn.Focus();
                else UpdateBtn.Focus();
            }
            else if (e.Key == Key.Enter)
            {
                if (UpdateBtn.IsFocused && UpdateBtn.IsEnabled) StartUpdate_Click(null, null);
                else if (CancelBtn.IsFocused) CancelUpdate_Click(null, null);
            }
            else if (e.Key == Key.Escape) CancelUpdate_Click(null, null);
        }

        private async void StartUpdate_Click(object sender, RoutedEventArgs e)
        {
            PlayUISound("SND_SYSTEM_OK.mp3"); // Play confirm sound
            isUpdating = true;
            ConfirmationPanel.Visibility = Visibility.Collapsed;
            InstallPanel.Visibility = Visibility.Visible;
            await BeginUpdateProcess();
        }

        private void CancelUpdate_Click(object sender, RoutedEventArgs e)
        {
            PlayUISound("SND_CANCEL.mp3"); // Play cancel sound
            FadeOutAndClose();
        }

        private async Task BeginUpdateProcess()
        {
            for (int i = 0; i <= 100; i++)
            {
                UpdateProgressBar.Value = i;
                PercentageText.Text = $"{i}%";

                if (i < 20) StatusText.Text = "Checking for update data... Do not turn off the system.";
                else if (i < 45) StatusText.Text = "Verifying... This may take a few moments.";
                else if (i < 70) StatusText.Text = "Copying update files to system storage...";
                else if (i >= 95) StatusText.Text = "Installation complete. System will restart.";

                if (isExploitPath && i == 71)
                {
                    await TriggerExploitSequence();
                }

                await Task.Delay(rng.Next(50, 150));
            }

            await Task.Delay(2000);
            CompleteUpdateAndReboot();
        }

        private void FadeOutAndClose()
        {
            inputTimer?.Stop();

            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            fadeOut.Completed += (s, e) => this.Close();
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }

        // Update the end of your reboot logic to use this too:
        private void CompleteUpdateAndReboot()
        {
            SystemSettings.CurrentFirmware = UpdateFileName.Text;

            MainWindow rebootedBoot = new MainWindow();
            rebootedBoot.Show();

            FadeOutAndClose(); // Smooth transition to the cold boot
        }

        private async Task TriggerExploitSequence()
        {
            StatusText.Text = "SYSTEM ERROR: 8002F14E";
            StatusText.Foreground = Brushes.Red;
            await Task.Delay(800);

            ExploitOverlay.Visibility = Visibility.Visible;
            KernelLogs.Text = string.Empty;

            string[] logs = {
                "[#] Initializing kernel panic...",
                "[#] Mapping L2 shadow registers...",
                "[#] Triggering memory corruption at 0x80000000",
                "[!] Buffer overflow detected in sys_update.self",
                "[#] Injected ROP Chain (Length: 256)",
                "[#] Found toc at 0xFFFFFFFF8001CB40",
                "[#] Overwriting LV2 syscall table...",
                "[#] Escalating privileges to Ring 0...",
                "[SUCCESS] Kernel access established.",
                "[#] HV_SC_627 Bypass: Active",
                "[#] Resuming service..."
            };

            foreach (string log in logs)
            {
                KernelLogs.Text += log + Environment.NewLine;
                LogScroller.ScrollToEnd();
                await Task.Delay(rng.Next(150, 400));
            }

            await Task.Delay(1500);
            ExploitOverlay.Visibility = Visibility.Collapsed;
            StatusText.Text = "Installing system software... Do not turn off the system.";
            StatusText.Foreground = Brushes.White;
        }
    }
}