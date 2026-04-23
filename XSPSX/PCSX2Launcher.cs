using System;
using System.Diagnostics;
using System.IO;

namespace XSPSX
{
    public class PCSX2Launcher
    {
        private string pcsx2Path;

        public PCSX2Launcher(string emuPath = null)
        {
            // If we pass a path, use it; otherwise, use your local hardcoded path
            this.pcsx2Path = emuPath ?? @"C:\Users\ikerk\Documents\XSPSX-PC-master\XSPSX-PC\XSPSX\PCSX2\pcsx2-qt.exe";
        }

        public void LaunchPCSX2(string isoPath = null, Action onPCSX2Exit = null)
        {
            try
            {
                if (!File.Exists(pcsx2Path))
                {
                    System.Windows.MessageBox.Show($"Emulator not found at: {pcsx2Path}");
                    return;
                }

                // Launch arguments
                string arguments = string.IsNullOrEmpty(isoPath)
                    ? "-fullscreen"
                    : $"-fullscreen -batch \"{isoPath}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pcsx2Path,
                    Arguments = arguments,
                    UseShellExecute = true
                };

                Process pcsx2Process = new Process { StartInfo = startInfo };
                pcsx2Process.EnableRaisingEvents = true;

                pcsx2Process.Exited += (sender, args) =>
                {
                    onPCSX2Exit?.Invoke();
                };

                pcsx2Process.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Critical Launch Error: {ex.Message}");
            }
        }
    }
}
