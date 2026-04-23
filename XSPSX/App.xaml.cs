using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace XSPSX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
//
    public static class SystemSettings
    {
        // Defaults to Official Firmware
        public static string CurrentFirmware { get; set; } = "XSPSX_OFW_1.0.0";

        // Track the "jailbreak" state globally
        public static bool IsJailbroken { get; set; } = false;

        // Fun toggles for the System Info screen
        public static string KernelVersion { get; set; } = "0.1.25-LVK2-PRE";
        public static bool SyscallsEnabled { get; set; } = false;
        public static bool HomebrewEnabled { get; set; } = false;
    }


    public partial class App : Application
    {
    }
}
