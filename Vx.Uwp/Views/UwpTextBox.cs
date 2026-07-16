using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpTextBox : UwpView<Vx.Views.TextBox, BareTextBox>
    {
        private bool _pasteHandlerAttached;

        public UwpTextBox()
        {
            View.NativeTextBox.TextChanged += View_TextChanged;
            View.NativeTextBox.LostFocus += NativeTextBox_LostFocus;
            View.NativeTextBox.GotFocus += NativeTextBox_GotFocus;
            View.Loaded += View_Loaded;
            View.EnterPressed += View_EnterPressed;
        }

        private void View_EnterPressed(object sender, EventArgs e)
        {
            if (VxView.OnSubmit != null)
            {
                VxView.OnSubmit();
            }
        }

        private void View_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView != null && VxView.AutoFocus)
            {
                View.NativeTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                View.NativeTextBox.SelectAll();
            }
        }

        private void NativeTextBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(true);
            }
        }

        private void NativeTextBox_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(false);
            }
        }

        private void View_TextChanged(object sender, TextChangedEventArgs e)
        {
            VxView.Text?.ValueChanged?.Invoke(View.Text);
        }

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            // For first time initializing, initialize multi-line
            if (newView is Vx.Views.MultilineTextBox multiline && oldView == null)
            {
                View.NativeTextBox.AcceptsReturn = true;
                View.NativeTextBox.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            }

            // Attach paste/drop handler if OnImagesPasted is set
            if (newView is Vx.Views.MultilineTextBox mlb && mlb.OnImagesPasted != null && !_pasteHandlerAttached)
            {
                View.NativeTextBox.Paste += NativeTextBox_Paste;
                View.AllowDrop = true;
                View.NativeTextBox.AllowDrop = true;
                View.Drop += View_Drop;
                View.DragOver += View_DragOver;
                _pasteHandlerAttached = true;
            }

            View.Header = newView.Header;
            View.Text = newView.Text?.Value ?? "";
            View.PlaceholderText = newView.PlaceholderText;
            View.ValidationState = newView.ValidationState;
            View.IsEnabled = newView.IsEnabled;

            if (oldView == null || oldView.InputScope != newView.InputScope)
            {
                switch (newView.InputScope)
                {
                    case Vx.Views.InputScope.Normal:
                        View.IsSpellCheckEnabled = true;
                        View.IsTextPredictionEnabled = true;
                        View.InputScope = new Windows.UI.Xaml.Input.InputScope
                        {
                            Names =
                            {
                                new Windows.UI.Xaml.Input.InputScopeName(Windows.UI.Xaml.Input.InputScopeNameValue.Text)
                            }
                        };
                        break;

                    case Vx.Views.InputScope.Email:
                        View.IsSpellCheckEnabled = false;
                        View.IsTextPredictionEnabled = true;
                        View.InputScope = new Windows.UI.Xaml.Input.InputScope
                        {
                            Names =
                            {
                                new Windows.UI.Xaml.Input.InputScopeName(Windows.UI.Xaml.Input.InputScopeNameValue.EmailSmtpAddress)
                            }
                        };
                        break;

                    case Vx.Views.InputScope.Username:
                        View.IsSpellCheckEnabled = false;
                        View.IsTextPredictionEnabled = true;
                        View.InputScope = new Windows.UI.Xaml.Input.InputScope
                        {
                            Names =
                            {
                                new Windows.UI.Xaml.Input.InputScopeName(Windows.UI.Xaml.Input.InputScopeNameValue.EmailNameOrAddress)
                            }
                        };
                        break;
                }
            }
        }

        private void View_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Bitmap) || e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.Handled = true;
            }
        }

        private async void View_Drop(object sender, DragEventArgs e)
        {
            var images = await ExtractImagesFromDataPackageAsync(e.DataView);
            if (images.Count > 0 && VxView is Vx.Views.MultilineTextBox mlb)
            {
                mlb.OnImagesPasted?.Invoke(images);
            }
        }

        private async void NativeTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Bitmap) || dataPackageView.Contains(StandardDataFormats.StorageItems))
            {
                var images = await ExtractImagesFromDataPackageAsync(dataPackageView);
                if (images.Count > 0)
                {
                    e.Handled = true;
                    if (VxView is Vx.Views.MultilineTextBox mlb)
                    {
                        mlb.OnImagesPasted?.Invoke(images);
                    }
                }
            }
        }

        private static async Task<List<Vx.Views.PastedImage>> ExtractImagesFromDataPackageAsync(DataPackageView dataView)
        {
            var results = new List<Vx.Views.PastedImage>();

            try
            {
                if (dataView.Contains(StandardDataFormats.Bitmap))
                {
                    var bitmapRef = await dataView.GetBitmapAsync();
                    using (var stream = await bitmapRef.OpenReadAsync())
                    {
                        var bytes = new byte[stream.Size];
                        var buffer = await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                        results.Add(new Vx.Views.PastedImage
                        {
                            Data = buffer.ToArray(),
                            MediaType = stream.ContentType.Split(",")[0] ?? "image/png"
                        });
                    }
                }

                //if (dataView.Contains(StandardDataFormats.StorageItems))
                //{
                //    var items = await dataView.GetStorageItemsAsync();
                //    foreach (var item in items)
                //    {
                //        if (item is Windows.Storage.StorageFile file && IsImageFile(file))
                //        {
                //            using (var stream = await file.OpenReadAsync())
                //            {
                //                var bytes = new byte[stream.Size];
                //                var buffer = await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                //                results.Add(new Vx.Views.PastedImage
                //                {
                //                    Data = buffer.ToArray(),
                //                    MediaType = file.ContentType ?? "image/png"
                //                });
                //            }
                //        }
                //    }
                //}
            }
            catch
            {
                // Silently fail if we can't read clipboard/drop data
            }

            return results;
        }

        private static bool IsImageFile(Windows.Storage.StorageFile file)
        {
            var ext = file.FileType?.ToLowerInvariant();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp" || ext == ".webp";
        }
    }
}
