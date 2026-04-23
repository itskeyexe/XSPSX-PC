using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace XSPSX
{
    public partial class SystemInfoWindow : Window
    {
        private SharpXInputHandler xInput;
        private DispatcherTimer inputTimer;

        public SystemInfoWindow()
        {
            InitializeComponent();
            LoadSystemData();

            // Initialize Controller Handler
            try
            {
                xInput = new SharpXInputHandler(null);
            }
            catch { /* Silent fail */ }

            // Setup Timer for Controller Input
            inputTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            inputTimer.Tick += InputTimer_Tick;
            inputTimer.Start();

            this.Loaded += (s, e) =>
            {
                WaveBackground.Play();
                BackBtn.Focus(); // Ensure the button has focus for keyboard 'Enter'
            };
        }

        private void LoadSystemData()
        {
            FirmwareDisplay.Text = SystemSettings.CurrentFirmware;

            if (SystemSettings.IsJailbroken)
            {
                JailbreakStatus.Text = "Cobra 8.4 Active";
                JailbreakStatus.Foreground = Brushes.Cyan;
                SyscallStatus.Text = SystemSettings.SyscallsEnabled ? "LV2 PEEK/POKE Enabled" : "Disabled";
                HomebrewStatus.Text = SystemSettings.HomebrewEnabled ? "Enabled (HEN)" : "Locked";
            }
            else
            {
                JailbreakStatus.Text = "Disabled";
                JailbreakStatus.Foreground = Brushes.Gray;
                SyscallStatus.Text = "Restricted";
                HomebrewStatus.Text = "Locked";
            }
        }

        private void InputTimer_Tick(object sender, EventArgs e)
        {
            if (xInput == null || !xInput.IsConnected) return;

            xInput.Update();

            // A Button to confirm 'Back' if focused, or B Button to close immediately
            if (xInput.IsButtonAPressed() && BackBtn.IsFocused)
            {
                Back_Click(null, null);
            }
            else if (xInput.IsButtonBPressed())
            {
                Back_Click(null, null);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter (A) handles the focused button automatically, 
            // but we add Escape/Back (B) here for the keyboard
            if (e.Key == Key.Escape || e.Key == Key.Back)
            {
                Back_Click(null, null);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            inputTimer?.Stop(); // Stop polling before closing
            this.Close();
        }

        private void WaveBackground_MediaEnded(object sender, RoutedEventArgs e)
        {
            WaveBackground.Position = TimeSpan.Zero;
            WaveBackground.Play();
        }
    }
}