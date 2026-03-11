using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using InterfacesiOS.App;
using NetworkExtension;
using PhotosUI;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using StorageEverywhere;
using UIKit;
using Vx;

namespace PowerPlanneriOS.Extensions
{
    [SupportedOSPlatform("ios14.0")]
    public class iOSImagePickerExtension : ImagePickerExtension
    {
        public override async Task<IFile[]> PickImagesAsync()
        {
            // Create a PHPickerConfiguration
            var config = new PHPickerConfiguration()
            {
                SelectionLimit = 0, // 0 means no limit, can be set to any number for a specific limit
                Filter = PHPickerFilter.ImagesFilter
            };

            // Create a PHPickerViewController with the configuration
            var picker = new PHPickerViewController(config);

            // Create a TaskCompletionSource to handle the picker result
            var tcs = new TaskCompletionSource<IFile[]>();

            // Handler when images are picked
            picker.Delegate = new PickerDelegate(tcs);

            // Present the picker
            var currContent = PowerPlannerAppDataLibrary.App.PowerPlannerApp.Current.GetMainScreenViewModel().GetFinalContent();

            INativeComponent nativeComponent;
            if (currContent.NativeComponent == null || !currContent.NativeComponent.TryGetTarget(out nativeComponent))
            {
                throw new InvalidOperationException("NativeComponent was null in " + currContent.GetType().Name);
            }
            var currController = InterfacesiOS.Views.BareUIHelper.GetViewController(nativeComponent as Vx.iOS.iOSNativeComponent);
            currController.PresentViewController(picker, true, null);

            return await tcs.Task;
        }

        // Custom delegate to handle picker results
        class PickerDelegate : PHPickerViewControllerDelegate
        {
            private readonly TaskCompletionSource<IFile[]> _tcs;

            public PickerDelegate(TaskCompletionSource<IFile[]> tcs)
            {
                _tcs = tcs;
            }

            public override async void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
            {
                try
                {
                    picker.DismissViewController(true, null);

                    List<IFile> files = new List<IFile>();

                    // Process each picked image
                    foreach (var result in results)
                    {
                        if (result.ItemProvider.HasItemConformingTo("public.image"))
                        {
                            var file = await LoadImage(result.ItemProvider);
                            if (file != null)
                            {
                                files.Add(file);
                            }
                        }
                    }

                    _tcs.SetResult(files.ToArray());
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    _tcs.SetResult(new IFile[0]);
                }
            }

            private Task<IFile> LoadImage(Foundation.NSItemProvider itemProvider)
            {
                var taskCompletion = new TaskCompletionSource<IFile>();

                try
                {
                    itemProvider.LoadObject<UIImage>(async (image, error) =>
                    {
                        if (image is UIImage uiImage)
                        {
                            try
                            {
                                // Generate a temporary file path
                                var fileName = $"{Guid.NewGuid()}.png";
                                var tempFile = await TempFile.CreateAsync(fileName);

                                using (var imageData = uiImage.AsPNG())
                                {
                                    using (var fileStream = await tempFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                                    {
                                        imageData.AsStream().CopyTo(fileStream);
                                    }
                                }

                                taskCompletion.SetResult(tempFile);
                            }
                            catch (Exception ex)
                            {
                                TelemetryExtension.Current?.TrackException(ex);
                                taskCompletion.SetResult(null);
                            }
                        }
                        else
                        {
                            taskCompletion.SetResult(null);
                        }
                    });
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    return Task.FromResult<IFile>(null);
                }

                return taskCompletion.Task;
            }
        }
    }
}

