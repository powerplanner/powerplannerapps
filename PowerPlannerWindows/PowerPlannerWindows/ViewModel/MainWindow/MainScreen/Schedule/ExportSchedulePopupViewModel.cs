using BareMvvm.Core.ViewModels;
using InterfacesUWP;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerUWP.Views;
using PowerPlannerUWP.Views.ScheduleViews;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Schedule
{
    public class ExportSchedulePopupViewModel : BaseViewModel
    {
        private readonly ScheduleViewModel _scheduleViewModel;
        public UIElement Element { get; private set; }
        public Panel PanelForPrinting { get; set; }

        public ShareItem[] ShareItems { get; private set; }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, nameof(IsEnabled)); }
        }

        public ExportSchedulePopupViewModel(BaseViewModel parent, ScheduleViewModel scheduleViewModel) : base(parent)
        {
            _scheduleViewModel = scheduleViewModel;

            Element = CreateView();

            ShareItems = new ShareItem[]
            {
                // TODO: Localize strings
                new ShareItem()
                {
                    Title = LocalizedResources.GetString("String_ScheduleExportToImage_Clipboard_Title"),
                    Subtitle = LocalizedResources.GetString("String_ScheduleExportToImage_Clipboard_Subtitle"),
                    ClickAction = ExportToClipboard
                },

                new ShareItem()
                {
                    Title = LocalizedResources.GetString("String_ScheduleExportToImage_Save_Title"),
                    Subtitle = LocalizedResources.GetString("String_ScheduleExportToImage_Save_Subtitle"),
                    ClickAction = ExportToFile
                },

                // Only include share if it's supported
                App.IsSharingSupported ? new ShareItem()
                {
                    Title = LocalizedResources.GetString("String_ScheduleExportToImage_Share_Title"),
                    Subtitle = LocalizedResources.GetString("String_ScheduleExportToImage_Share_Subtitle"),
                    ClickAction = ExportToShare
                } : null,

                // Cutting since implementing this is more complicated than expected.
                // See description below for more details.
                //new ShareItem()
                //{
                //    Title = "Send to printer",
                //    Subtitle = "Print an image of your schedule",
                //    ClickAction = ExportToPrinter
                //},

                new ShareItem()
                {
                    Title = LocalizedResources.GetString("String_ScheduleExportToImage_Cancel_Title"),
                    Subtitle = LocalizedResources.GetString("String_ScheduleExportToImage_Cancel_Subtitle"),
                    ClickAction = delegate { base.GoBack(); }
                }
            }.Where(i => i != null).ToArray();
        }

        private FrameworkElement CreateView()
        {
            var scheduleView = new ScheduleView()
            {
                ViewModel = _scheduleViewModel
            };
            scheduleView.ConfigureForExport();

            // For some reason there's a black bar at the bottom...
            // Probably related to the XAML deferred loading (causing that 15 pixel divider bar to consume space)...
            // So we just accomodate for it by cropping out 15 px from the bottom
            var clippedView = new Border()
            {
                Child = scheduleView,
                Margin = new Thickness(0, 0, 0, -15)
            };

            return clippedView;
        }

        public async void ExportToClipboard()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_ExportSchedule_ViaClipboard");
                IsEnabled = false;

                var randomAccessStream = await RenderTargetBitmapHelper.ExportToStream(Element, Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId);

                try
                {
                    // Also write into a storage file
                    var tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("Schedule.jpg", CreationCollisionOption.ReplaceExisting);
                    using (var stream = await tempFile.OpenStreamForWriteAsync())
                    {
                        await randomAccessStream.AsStreamForRead().CopyToAsync(stream);
                    }

                    var streamReference = RandomAccessStreamReference.CreateFromStream(randomAccessStream);

                    DataPackage dp = new DataPackage();
                    dp.SetBitmap(streamReference);
                    dp.SetStorageItems(new IStorageItem[] { tempFile });

                    dp.Destroyed += delegate
                    {
                        try
                        {
                            randomAccessStream.Dispose();
                        }
                        catch { }
                    };

                    Clipboard.SetContent(dp);
                }
                catch
                {
                    randomAccessStream.Dispose();
                    throw;
                }

                GoBack();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                IsEnabled = true;
            }
        }

        public async void ExportToFile()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_ExportSchedule_ViaFile");
                IsEnabled = false;

                FileSavePicker picker = new FileSavePicker
                {
                    DefaultFileExtension = ".jpg"
                };
                picker.FileTypeChoices.Add(new KeyValuePair<string, IList<string>>("JPEG", new List<string>() { ".jpg" }));
                picker.SuggestedFileName = "Schedule";
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                var storageFile = await picker.PickSaveFileAsync();

                if (storageFile != null)
                {
                    using (var randomAccessStream = await RenderTargetBitmapHelper.ExportToStream(Element, Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId))
                    {
                        using (var stream = await storageFile.OpenStreamForWriteAsync())
                        {
                            await randomAccessStream.AsStreamForRead().CopyToAsync(stream);
                        }
                    }

                    GoBack();
                }
                else
                {
                    IsEnabled = true;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                IsEnabled = true;
            }
        }

        public void ExportToShare()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_ExportSchedule_ViaShare");
                IsEnabled = false;
                App.ShowShareUI(ShareDataRequested);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                IsEnabled = true;
            }
        }

        private async void ShareDataRequested(DataRequestedEventArgs args)
        {
            var request = args.Request;

            var deferral = request.GetDeferral();

            try
            {
                var randomAccessStream = await RenderTargetBitmapHelper.ExportToStream(Element, Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId);

                try
                {
                    // Also write into a storage file
                    var tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("Schedule.jpg", CreationCollisionOption.ReplaceExisting);
                    using (var stream = await tempFile.OpenStreamForWriteAsync())
                    {
                        await randomAccessStream.AsStreamForRead().CopyToAsync(stream);
                    }

                    // Provide both bitmap and storage item, since some apps only support one
                    request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(randomAccessStream));
                    request.Data.SetStorageItems(new IStorageItem[] { tempFile });

                    request.Data.RequestedOperation = DataPackageOperation.Copy;
                    request.Data.Properties.Title = "Schedule";

                    request.Data.Destroyed += delegate
                    {
                        try
                        {
                            randomAccessStream.Dispose();
                        }
                        catch { }
                    };

                    request.Data.OperationCompleted += delegate
                    {
                        try
                        {
                            if (IsCurrentNavigatedPage)
                            {
                                GoBack();
                            }
                        }
                        catch { }
                    };
                }

                catch (Exception ex)
                {
                    randomAccessStream.Dispose();
                    throw ex;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                IsEnabled = true;
                deferral.Complete();
            }
        }

        public class ShareItem
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }

            public Action ClickAction { get; set; }
        }
    }
}
