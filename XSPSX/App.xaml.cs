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
            public static string CurrentFirmware { get; set; } = "XSPSX_OFW_1.00";
        }
    

    public partial class App : Application
    {
    }
}
