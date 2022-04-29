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
            CustomHeight.IsEnabled = false; CustomWidth.IsEnabled = false;
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
        private void SizeCancelled(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            new ToastContentBuilder().AddText("Saving cancelled");
        }
        private async void FileSizeChosen(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (CustomHeight.IsEnabled && CustomWidth.IsEnabled)
            {
                SavingHeight = int.Parse(CustomHeight.Text);
                SavingWidth = int.Parse(CustomWidth.Text);
            }
            else
            {
                IReadOnlyList<InkStroke> currentStrokes = DrawingCanvas.InkPresenter.StrokeContainer.GetStrokes();
                CanvasDevice device = CanvasDevice.GetSharedDevice();
                CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, SavingWidth, SavingHeight, 96);
                var SaveDrawing = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    SuggestedFileName = "Untitled drawing"
                };
                if (currentStrokes.Count > 0)
                {
                    // Let users choose their ink file using a file picker.
                    // Initialize the picker.
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    savePicker.FileTypeChoices.Add("Portable Network Graphics Image", new List<string>() { ".png" });
                    savePicker.DefaultFileExtension = ".png";
                    savePicker.SuggestedFileName = "Untitled drawing";
                    StorageFile file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        CachedFileManager.DeferUpdates(file);
                        IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                        using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
                        {
                            await DrawingCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                            await outputStream.FlushAsync();
                        }
                        stream.Dispose();
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status == FileUpdateStatus.Complete)
                        {
                            // ContentDialog FileSaved = new ContentDialog()
                            // {
                            //  Title = "File saved as " + file.Name,
                            //  CloseButtonText = "Close",
                            // }; 
                            new ToastContentBuilder()
                                .AddText("File saved successfully!")
                                .AddText("Saved as " + file.Name);
                        }
                        else
                        {
                            // ContentDialog FileNotSaved = new ContentDialog()
                            // {
                            //  Title = "File wasn't saved.",
                            //  Content="Are you sure you have access to the current path?",
                            //  CloseButtonText = "Close",
                        };
                        new ToastContentBuilder()
                            .AddText("Failed to save file")
                            .AddText("Are you sure you have access to the current path?");
                    }
                }
                // User selects Cancel and picker returns null.
                else
                {
                    // Operation cancelled.
                    // ContentDialog FileSaveCancelled = new ContentDialog()
                    //{
                    //  Title = "Operation cancelled by user.",
                    //  CloseButtonText = "Close",
                    // };
                    new ToastContentBuilder()
                        .AddText("Operation cancelled by user");
                }
            }
        }
        private void ComboBoxGone(object sender, RoutedEventArgs e)
        {
            CustomWidth.IsEnabled = true; CustomHeight.IsEnabled = true; CustomSizeSelected.IsEnabled = true;
        }
        private void ComboBoxVisible(object sender, RoutedEventArgs e)
        {
            CustomWidth.IsEnabled = false; CustomHeight.IsEnabled = false; CustomSizeSelected.IsEnabled = false;
        }
        private async void SaveClick(object sender, RoutedEventArgs e)
        {
            await FileSizePicker.ShowAsync();

        }
        private void ExportCancelled(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            new ToastContentBuilder().AddText("Saving cancelled");
        }
        private void DefaultSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            int DSPIndex = DefaultSizePicker.SelectedIndex;
            switch(DSPIndex)
            {
                case 0: SavingHeight = 7680; SavingWidth = 4320; break;
                case 1: SavingHeight = 3840; SavingWidth = 2160; break;
                case 2: SavingHeight = 1920; SavingWidth = 1080; break;
                case 3: SavingHeight = 1280; SavingWidth = 720; break;
                case 4: SavingHeight = 960; SavingWidth = 540; break;
                default: SavingHeight = 0; SavingWidth = 0; break;
            }
        }
        private void CustomHeight_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        private void CustomWidth_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        private void CustomHeight_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
        private void CustomWidth_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
        private void CustomSizeSelected_Click(object sender, RoutedEventArgs e)
        {
            SavingHeight = Int32.Parse(CustomHeight.Text.ToString());
            SavingWidth = Int32.Parse(CustomWidth.Text.ToString());
        }
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }
    }
}