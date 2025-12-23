using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace XSPSX
{
    public partial class WebBrowserWindow : Window
    {
        private string xspsxDownloadPath = @"C:\XSPSX\Downloads\"; // Ensure this folder exists

        public WebBrowserWindow()
        {
            InitializeComponent();
            InitializeBrowser();
        }

        private async void InitializeBrowser()
        {
            try
            {
                await WebView.EnsureCoreWebView2Async(null);
                WebView.Source = new Uri("https://twilightrequiem.dev");

                // Inject JavaScript to detect file downloads
                WebView.CoreWebView2.NavigationCompleted += async (sender, args) =>
                {
                    string jsScript = @"
                        document.querySelectorAll('a').forEach(link => {
                            link.addEventListener('click', function(event) {
                                window.chrome.webview.postMessage(link.href);
                            });
                        });
                    ";
                    await WebView.CoreWebView2.ExecuteScriptAsync(jsScript);
                };

                WebView.WebMessageReceived += WebView_WebMessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load WebView2: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string downloadUrl = e.WebMessageAsJson.Trim('"'); // Extract URL from JSON

            if (Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri validUri))
            {
                StartDownload(validUri);
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UrlTextBox.Text))
            {
                WebView.Source = new Uri(UrlTextBox.Text);
            }
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                GoButton_Click(sender, e);
            }
        }

        private async void StartDownload(Uri fileUrl)
        {
            try
            {
                string fileName = Path.GetFileName(fileUrl.LocalPath);
                string savePath = Path.Combine(xspsxDownloadPath, fileName);
                Directory.CreateDirectory(xspsxDownloadPath); // Ensure download folder exists

                ShowNotification("Downloading...", $"{fileName}", "Resources/Icons/download.png");

                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        Console.WriteLine($"Downloading {fileName}: {e.ProgressPercentage}%");
                    };

                    client.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error == null)
                        {
                            string message = $"Download Complete: {fileName}";

                            if (fileName.EndsWith(".pkg"))
                            {
                                message += "\nInstall via Package Manager.";
                            }
                            else if (fileName.EndsWith(".pup"))
                            {
                                message += "\nInstall via System Settings.";
                            }
                            else if (fileName.EndsWith(".zip"))
                            {
                                message += "\nExtract to XSPSX Homebrew.";
                            }

                            ShowNotification("Download Complete", message, "Resources/Icons/downloadComplete.png");
                        }
                        else
                        {
                            ShowNotification("Download Failed", $"Error: {e.Error.Message}", "Resources/Icons/error.png");
                        }
                    };

                    await client.DownloadFileTaskAsync(fileUrl, savePath);
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Download error: {ex.Message}", "Resources/Icons/error.png");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowNotification(string title, string message, string iconPath)
        {
            NotificationTitle.Text = title;
            NotificationMessage.Text = message;
            NotificationIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Relative));

            NotificationGrid.Visibility = Visibility.Visible;

            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(3),
                BeginTime = TimeSpan.FromSeconds(3),
                AutoReverse = false
            };

            fadeOut.Completed += (s, e) => NotificationGrid.Visibility = Visibility.Collapsed;

            NotificationGrid.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
