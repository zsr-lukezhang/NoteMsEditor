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
                await WriteToNoteMS(uriTextBox.Text, noteTextBox.Text);
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

        public async Task WriteToNoteMS(string url, string content)
        {
            try
            {
                if (webView.CoreWebView2 == null)
                {
                    await webView.EnsureCoreWebView2Async();
                }

                webView.Source = new Uri(url);

                webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
                {
                    string escapedContent = content.Replace("\n", "\\n").Replace("\r", "\\r").Replace("'", "\\'");
                    string script = $@"
                    var contentArea = document.querySelector('textarea');
                    if (contentArea) {{
                        contentArea.value = '{escapedContent}';
                        contentArea.dispatchEvent(new Event('input'));
                    }}
                ";
                    await webView.CoreWebView2.ExecuteScriptAsync(script);
                };

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
