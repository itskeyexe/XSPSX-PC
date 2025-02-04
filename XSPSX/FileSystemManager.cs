using System;
using System.IO;

namespace XSPSX
{
    public static class FileSystemManager
    {
        public static string RootPath = @"C:\XSPSX\";

        public static void InitializeFileSystem()
        {
            string[] directories = {
                RootPath,
                Path.Combine(RootPath, "Games"),
                Path.Combine(RootPath, "Updates"),
                Path.Combine(RootPath, "Packages"),
                Path.Combine(RootPath, "Plugins"),
                Path.Combine(RootPath, "Themes"),
                Path.Combine(RootPath, "Homebrew"),
                Path.Combine(RootPath, "Saves"),
                Path.Combine(RootPath, "System"),
                Path.Combine(RootPath, "Downloads") // ✅ NEW: Added "Downloads" folder
            };

            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Console.WriteLine($"Created: {dir}");
                }
            }

            Console.WriteLine("✅ XSPSX File System Initialized.");
        }

        public static void InstallPackage(string packagePath)
        {
            if (!File.Exists(packagePath))
            {
                Console.WriteLine("❌ Package file not found.");
                return;
            }

            string destPath = Path.Combine(RootPath, "Packages", Path.GetFileName(packagePath));
            File.Copy(packagePath, destPath, true);
            Console.WriteLine($"✅ Installed Package: {destPath}");
        }

        public static string[] ListDirectoryContents(string subPath)
        {
            string fullPath = Path.Combine(RootPath, subPath);
            if (Directory.Exists(fullPath))
            {
                return Directory.GetFiles(fullPath);
            }
            return Array.Empty<string>();
        }

        public static void MoveDownloadedFileToPackages(string fileName)
        {
            string downloadsPath = Path.Combine(RootPath, "Downloads", fileName);
            string packagesPath = Path.Combine(RootPath, "Packages", fileName);

            if (File.Exists(downloadsPath))
            {
                File.Move(downloadsPath, packagesPath);
                Console.WriteLine($"✅ Moved {fileName} to Packages folder.");
            }
            else
            {
                Console.WriteLine($"❌ File not found in Downloads: {fileName}");
            }
        }
    }
}
