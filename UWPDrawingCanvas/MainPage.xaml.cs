using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.Storage;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Provider;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPDrawingCanvas
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public int SavingHeight, SavingWidth;
        public MainPage()
        {
            this.InitializeComponent();
            DrawingCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);
            Window.Current.SetTitleBar(AppTitleBar);
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            Window.Current.Activated += Current_Activated;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            DrawingCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
        }
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }
        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            AppTitleBar.Height = coreTitleBar.Height;
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }
        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible) AppTitleBar.Visibility = Visibility.Visible;
            else AppTitleBar.Visibility = Visibility.Collapsed;
        }
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == CoreWindowActivationState.Deactivated) AppTitle.Foreground = inactiveForegroundBrush;
            else AppTitle.Foreground = defaultForegroundBrush;
        }
        private void CustomWidth_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
        private void SizeCancelled(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            new ToastContentBuilder().AddText("Saving cancelled");
        }
        private void FileSizeChosen(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SavingHeight = int.Parse(CustomHeight.Text);
            SavingWidth = int.Parse(CustomWidth.Text);
        }
        private void ComboBoxGone(object sender, RoutedEventArgs e)
        {
            CustomWidth.IsEnabled = true; CustomHeight.IsEnabled = true;
        }
        private void ComboBoxVisible(object sender, RoutedEventArgs e)
        {
            CustomWidth.IsEnabled = false; CustomHeight.IsEnabled = false;
        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FileSavePicker));
        }
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }
    }
}