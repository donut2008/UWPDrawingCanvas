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
        { UpdateTitleBarLayout(sender); }
        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            AppTitleBar.Height = coreTitleBar.Height;
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }
        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
                AppTitleBar.Visibility = Visibility.Visible;
            else
                AppTitleBar.Visibility = Visibility.Collapsed;
        }
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
                AppTitle.Foreground = inactiveForegroundBrush;
            else
                AppTitle.Foreground = defaultForegroundBrush;
        }
        private void CustomWidth_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
        private void SizeCancelled(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            new ToastContentBuilder() .AddText("Saving cancelled");
        }
        private void FileSizeChosen(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SavingHeight = int.Parse(CustomHeight.Text); SavingWidth = int.Parse(CustomWidth.Text);
        }
        private void ComboBoxGone(object sender, RoutedEventArgs e)
        {
            CustomWidth.IsEnabled = false; CustomHeight.IsEnabled = false;
        }
        private void ComboBoxVisible(object sender, RoutedEventArgs e)
        {
            CustomWidth.IsEnabled = true; CustomHeight.IsEnabled = true;
        }
        private async void SaveClick(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> currentStrokes = DrawingCanvas.InkPresenter.StrokeContainer.GetStrokes();
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, SavingHeight, SavingWidth, 96);
            var SaveDrawing = new FileSavePicker();
            SaveDrawing.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            SaveDrawing.SuggestedFileName = "Untitled drawing";
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
                        /*ContentDialog FileSaved = new ContentDialog()
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        /*ContentDialog FileSaved = new ContentDialog()
                        {
                            Title = "File saved as " + file.Name,
                            CloseButtonText = "Close",
                        };*/
                        new ToastContentBuilder()
                            .AddText("File saved successfully as" + file.Name)
                            .AddButton(new ToastButton()
                                .SetContent("Close"));
                    }
                    else
                    {
                        /* ContentDialog FileNotSaved = new ContentDialog()
                        {
                            Title = "File wasn't saved.",
                            Content="Are you sure you have access to the current path?",
                            CloseButtonText = "Close",
                        };*/
                        new ToastContentBuilder()
                            .AddText("File could not be saved!")
                            .AddText("Are you sure you have access to the save path?")
                            .AddButton(new ToastButton()
                                .SetContent("Close"));
                    }
                }
                // User selects Cancel and picker returns null.
                else
                {
                    // Operation cancelled.
                    /* ContentDialog FileSaveCancelled = new ContentDialog()
                    {
                        Title = "Operation cancelled by user.",
                        CloseButtonText = "Close",
                    };*/
                    new ToastContentBuilder()
                            .AddText("Saving file cancelled by user.")
                            .AddButton(new ToastButton()
                                .SetContent("Close"));
                }
            }
        }
        private void CustomHeight_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        private void CustomHeight_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
        private void CustomWidth_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }
        private void CustomHeight_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        private void CustomHeight_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }
        private void CustomWidth_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}