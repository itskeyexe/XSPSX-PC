using System;
using System.Diagnostics;

public class PCSX2Launcher
{
    private string pcsx2Path;

    public PCSX2Launcher(string pcsx2Path)
    {
        this.pcsx2Path = pcsx2Path;
    }

    public void LaunchPCSX2(string isoPath = null, Action onPCSX2Exit = null)
    {
        try
        {
            // Ensure the ISO path is correctly enclosed in quotes
            string arguments = string.IsNullOrEmpty(isoPath) ? "" : $"\"{isoPath}\"";

            Console.WriteLine($"Launching PCSX2 with arguments: {arguments}");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pcsx2Path,
                Arguments = arguments,  // Correctly formatted game path
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true // No extra command window
            };

            Process pcsx2Process = new Process { StartInfo = startInfo };
            pcsx2Process.EnableRaisingEvents = true;

            pcsx2Process.Exited += (sender, args) =>
            {
                Console.WriteLine("PCSX2 process exited.");
                onPCSX2Exit?.Invoke();
            };

            pcsx2Process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start PCSX2: {ex.Message}");
        }
    }
}

