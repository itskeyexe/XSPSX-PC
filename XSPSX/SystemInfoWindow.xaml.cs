using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace XSPSX
{
    public partial class SystemInfoWindow : Window
    {
        public SystemInfoWindow()
        {
            InitializeComponent();
            LoadSystemData();
            WaveBackground.Play();
            BackBtn.Focus();
        }

        private void LoadSystemData()
        {
            // Fetch the firmware version set during the update process 
            FirmwareDisplay.Text = SystemSettings.CurrentFirmware;

            if (SystemSettings.IsJailbroken)
            {
                JailbreakStatus.Text = "Cobra 8.4 Active";
                JailbreakStatus.Foreground = Brushes.Cyan; // Matches your exploit UI color 

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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Back) Back_Click(null, null);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WaveBackground_MediaEnded(object sender, RoutedEventArgs e)
        {
            WaveBackground.Position = TimeSpan.Zero;
            WaveBackground.Play();
        }
    }
}