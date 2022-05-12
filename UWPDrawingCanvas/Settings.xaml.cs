using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPDrawingCanvas
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
            else
            {
                ContentDialog NavigationError = new ContentDialog() {
                    Title = "Failed to go to previous page",
                    Content = "No page to navigate to in navigation cache",
                    CloseButtonText = "Close"
                };
            }
        }
        private void LightMode(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Light;
        }
        private void DarkMode(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Dark;
        }
        private void SystemDefault(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as FrameworkElement).RequestedTheme = ElementTheme.Default;
        }
    }
}