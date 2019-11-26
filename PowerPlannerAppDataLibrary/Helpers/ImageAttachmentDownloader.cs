﻿using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsPortable.Web;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class ImageAttachmentDownloader
    {
        private static SimpleAsyncWorkerQueue<ImageAttachmentLoadResult> _downloadsQueue = new SimpleAsyncWorkerQueue<ImageAttachmentLoadResult>();

        public static Task<ImageAttachmentLoadResult> DownloadImageAsync(IFolder imagesFolder, string imageName, string imageUrl)
        {
            return _downloadsQueue.QueueOrMergeAsync(imageUrl, delegate
            {
                return DownloadImageHelperAsync(imagesFolder, imageName, imageUrl);
            }, allowMergeWithAlreadyStarted: true);
        }

        private static async Task<ImageAttachmentLoadResult> DownloadImageHelperAsync(IFolder imagesFolder, string imageName, string imageUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var stream = await client.GetStreamAsync(imageUrl))
                    {
                        IFile file = await imagesFolder.CreateFileAsync(imageName, CreationCollisionOption.ReplaceExisting);
                        try
                        {
                            using (Stream storageStream = await file.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                            {
                                await stream.CopyToAsync(storageStream);
                            }
                            return new ImageAttachmentLoadResult()
                            {
                                Status = ImageAttachmentStatus.Loaded,
                                File = file
                            };
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                            await file.DeleteAsync();
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                return new ImageAttachmentLoadResult()
                {
                    Status = ImageAttachmentStatus.Offline
                };
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return new ImageAttachmentLoadResult()
            {
                Status = ImageAttachmentStatus.NotFound
            };
        }
    }
}
