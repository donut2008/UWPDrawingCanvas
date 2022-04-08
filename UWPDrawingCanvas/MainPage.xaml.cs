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
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPDrawingCanvas
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }
        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                AppTitle.Foreground = inactiveForegroundBrush;
            }
            else
            {
                AppTitle.Foreground = defaultForegroundBrush;
            }
        }
        /* private async void SaveClick(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> currentStrokes = DrawingCanvas.InkPresenter.StrokeContainer.GetStrokes();
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)DrawingCanvas.ActualWidth, (int)DrawingCanvas.ActualHeight, 96);
            var SaveDrawing = new Windows.Storage.Pickers.FileSavePicker();
            SaveDrawing.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            SaveDrawing.SuggestedFileName = "Untitled drawing";
            if (currentStrokes.Count > 0)
            {
                // Let users choose their ink file using a file picker.
                // Initialize the picker.
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add(
                    "Portable Network Graphics",
                    new List<string>() { ".png" });
                savePicker.DefaultFileExtension = ".png";
                savePicker.SuggestedFileName = "Untitled drawing";
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    Windows.Storage.CachedFileManager.DeferUpdates(file);
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
                    {
                        await DrawingCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                        await outputStream.FlushAsync();
                    }
                    stream.Dispose();
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        ContentDialog FileSaved = new ContentDialog()
                        {
                            Title = "File saved as " + file.Name + ".",
                            CloseButtonText = "Close",
                        };
                    }
                    else
                    {
                        ContentDialog FileNotSaved = new ContentDialog()
                        {
                            Title = "File wasn't saved.",
                            Content="Are you sure you have access to the current path?",
                            CloseButtonText = "Close",
                        };
                    }
                }
                // User selects Cancel and picker returns null.
                else
                {
                    // Operation cancelled.
                    ContentDialog FileSaveCancelled = new ContentDialog()
                    {
                        Title = "Operation cancelled by user.",
                        CloseButtonText = "Close",
                    };
                }
            }
        } */
        private byte[] ConvertInkCanvasToByteArray()
        {
            var canvasStrokes = DrawingCanvas.InkPresenter.StrokeContainer.GetStrokes();

            if (canvasStrokes.Count > 0)
            {
                var width = (int)DrawingCanvas.ActualWidth;
                var height = (int)DrawingCanvas.ActualHeight;
                var device = CanvasDevice.GetSharedDevice();
                var renderTarget = new CanvasRenderTarget(device, width,
                    height, 96);

                using (var ds = renderTarget.CreateDrawingSession())
                {
                    ds.Clear(Windows.UI.Colors.White);
                    ds.DrawInk(DrawingCanvas.InkPresenter.StrokeContainer.GetStrokes());
                }

                return renderTarget.GetPixelBytes();
            }
            else
            {
                return null;
            }
        }
        private WriteableBitmap GetSignatureBitmapFull()
        {
            var bytes = ConvertInkCanvasToByteArray();

            if (bytes != null)
            {
                var width = (int)DrawingCanvas.ActualWidth;
                var height = (int)DrawingCanvas.ActualHeight;

                var bmp = new WriteableBitmap(width, height);
                using (var stream = bmp.PixelBuffer.AsStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    return bmp;
                }
            }
            else
                return null;
        }

        private async Task<WriteableBitmap> GetSignatureBitmapFullAsync()
        {
            var bytes = ConvertInkCanvasToByteArray();

            if (bytes != null)
            {
                var width = (int)DrawingCanvas.ActualWidth;
                var height = (int)DrawingCanvas.ActualHeight;

                var bmp = new WriteableBitmap(width, height);
                using (var stream = bmp.PixelBuffer.AsStream())
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    return bmp;
                }
            }
            else
                return null;
        }
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }
    }
}