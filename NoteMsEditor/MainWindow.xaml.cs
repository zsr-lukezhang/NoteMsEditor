//                                           NoteMsEditor v0.1.0
//                                   A simple note taking app using Note.MS
//                                      Created by: Luke Zhang (2024)
//                                     Canary Version! May be unstable!
//                              The copyright of Note.MS belongs to Note.MS
//                             This project is licensed under the GNU v3 License



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NoteMsEditor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            // Extend the window into the title bar so that we can display the title bar content.
            ExtendsContentIntoTitleBar = true;
            // Setup WebView2
            InitializeWebView2();
        }

        /// <summary>
        /// This method is called when the sendOnceButton is clicked.
        /// Send a request to the div element to change its content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendOnceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                disableAll();
                await WriteToNoteMS(uriTextBox.Text, noteTextBox.Text);
                await Task.Delay(1000); // Just for UI LOL
                enableAll();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void InitializeWebView2()
        {
            await webView.EnsureCoreWebView2Async();
        }

        /// <summary>
        /// Write content to a Note.MS note.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task WriteToNoteMS(string url, string content)
        {
            try
            {
                string FullURL = $"https://note.ms/{url}";

                if (webView.CoreWebView2 == null)
                {
                    await webView.EnsureCoreWebView2Async();
                }

                webView.Source = new Uri(FullURL);

                async void NavigationCompletedHandler(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
                {
                    webView.CoreWebView2.NavigationCompleted -= NavigationCompletedHandler; // Only Once

                    string escapedContent = content.Replace("\n", "\\n").Replace("\r", "\\r").Replace("'", "\\'");
                    string script = $@"
                        var contentArea = document.querySelector('textarea');
                        if (contentArea) {{
                            contentArea.value = '{escapedContent}';
                            contentArea.dispatchEvent(new Event('input'));
                        }}
                    ";
                    await webView.CoreWebView2.ExecuteScriptAsync(script);
                }

                webView.CoreWebView2.NavigationCompleted += NavigationCompletedHandler;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Read content from a Note.MS note.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> ReadFromNoteMS(string url)
        {
            string FullURL = $"https://note.ms/{url}";

            if (webView.CoreWebView2 == null)
            {
                await webView.EnsureCoreWebView2Async();
            }

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            webView.Source = new Uri(FullURL);

            async void NavigationCompletedHandler(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
            {
                webView.CoreWebView2.NavigationCompleted -= NavigationCompletedHandler; // Only Once

                string script = @"
                var contentArea = document.querySelector('textarea');
                if (contentArea) {
                    contentArea.value;
                } else {
                    '';
                }
            ";
                try
                {
                    string result = await webView.CoreWebView2.ExecuteScriptAsync(script);
                    result = result.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r"); // Change \n to enter
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            webView.CoreWebView2.NavigationCompleted += NavigationCompletedHandler;

            return await tcs.Task;
        }
        /// <summary>
        /// This method is called when the refreshOnceButton is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void refreshOnceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                disableAll();
                string RemoteContent = await ReadFromNoteMS(uriTextBox.Text);
                noteTextBox.Text = RemoteContent;
                enableAll();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void disableAll()
        {
            loadingProgressRing.Visibility = Visibility.Visible;
            sendOnceButton.IsEnabled = false;
            refreshOnceButton.IsEnabled = false;
            uriTextBox.IsEnabled = false;
            noteTextBox.IsEnabled = false;
        }

        private void enableAll()
        {
            loadingProgressRing.Visibility = Visibility.Collapsed;
            sendOnceButton.IsEnabled = true;
            refreshOnceButton.IsEnabled = true;
            uriTextBox.IsEnabled = true;
            noteTextBox.IsEnabled = true;
        }

        private void Grid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.F12)
            {
                Dev.Visibility = Visibility.Visible;
                webView.UseLayoutRounding = true;
            }
        }

        private void stopDebugButton_Click(object sender, RoutedEventArgs e)
        {
            Dev.Visibility = Visibility.Collapsed;
        }
    }
}
