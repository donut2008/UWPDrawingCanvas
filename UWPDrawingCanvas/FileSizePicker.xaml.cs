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
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;
using Windows.Storage.Provider;
using Microsoft.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPDrawingCanvas
{
    public sealed partial class FileSizePicker : ContentDialog
    {
        MainPage mp = new MainPage();
        public int SavingHeight, SavingWidth;
        public FileSizePicker()
        {
            this.InitializeComponent();
        }
        private async Task FileSizeChosen(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IReadOnlyList<InkStroke> currentStrokes = mp.DrawingCanvas.InkPresenter.StrokeContainer.GetStrokes();
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
                        await mp.DrawingCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                        await outputStream.FlushAsync();
                    }
                    stream.Dispose();
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        /* ContentDialog FileSaved = new ContentDialog()
                        {
                            Title = "File saved as " + file.Name,
                            CloseButtonText = "Close",
                        }; */
                        new ToastContentBuilder()
                            .AddText("File saved successfully!")
                            .AddText("Saved as " + file.Name);
                    }
                    else
                    {
                        /* ContentDialog FileNotSaved = new ContentDialog()
                        {
                            Title = "File wasn't saved.",
                            Content="Are you sure you have access to the current path?",
                            CloseButtonText = "Close",
                        }; */
                        new ToastContentBuilder()
                            .AddText("Failed to save file")
                            .AddText("Are you sure you have access to the current path?");
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
                    }; */
                    new ToastContentBuilder()
                        .AddText("Operation cancelled by user");
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
        private void ExportCancelled(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            new ToastContentBuilder().AddText("Saving cancelled");
        }
        private void ComboBoxGone(object sender, RoutedEventArgs e)
        {
            CustomHeight.IsEnabled = true; CustomWidth.IsEnabled = true;
        }
        private void ComboBoxVisible(object sender, RoutedEventArgs e)
        {
            CustomHeight.IsEnabled = false; CustomWidth.IsEnabled = false;
        }
        private void CustomSizeSelected_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(CustomWidth.Text))
            {
                TeachingTip teachingTip = new TeachingTip()
                {
                    Title="Please enter a valid custom height/width.",
                    Subtitle="You cannot export a file of your specified height/width."
                };
            }
        }
        private void DefaultSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItemIndex=DefaultSizePicker.SelectedIndex.ToString();
            switch(selectedItemIndex)
            {
                case "0":
                    SavingHeight = 7680;
                    SavingWidth = 4320;
                    break;
                case "1":
                    SavingHeight = 3840;
                    SavingWidth = 2160;
                    break;
                case "2":
                    SavingHeight = 1920;
                    SavingHeight = 2160;
                    break;
                case "3":
                    SavingHeight = 1280;
                    SavingWidth = 720;
                    break;
                case "4":
                    SavingHeight = 960;
                    SavingWidth = 540;
                    break;
            }
        }
    }
}
