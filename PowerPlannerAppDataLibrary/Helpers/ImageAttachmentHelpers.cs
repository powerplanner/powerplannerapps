using PCLStorage;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class ImageAttachmentHelpers
    {
        public static async Task<ImageAttachmentLoadResult> GetOrStartDownloadAsync(string imageName)
        {
            try
            {
                var currAccount = PowerPlannerApp.Current.GetCurrentAccount();
                if (currAccount == null)
                    return null;

                IFolder imagesFolder = await FileHelper.GetOrCreateImagesFolder(currAccount.LocalAccountId);

                //if already exists
                try
                {
                    IFile file = await imagesFolder.GetFileAsync(imageName);

                    if (file != null)
                    {
                        return new ImageAttachmentLoadResult()
                        {
                            Status = ImageAttachmentStatus.Loaded,
                            File = file
                        };
                    }
                }

                catch { }

                if (!currAccount.IsOnlineAccount)
                {
                    return new ImageAttachmentLoadResult()
                    {
                        Status = ImageAttachmentStatus.NotFound
                    };
                }

                //otherwise we need to download the image
                return new ImageAttachmentLoadResult()
                {
                    Status = ImageAttachmentStatus.Downloading,
                    DownloadingResultTask = ImageAttachmentDownloader.DownloadImageAsync(imagesFolder, imageName, Website.GetImageUrl(currAccount.AccountId, imageName))
                };
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return new ImageAttachmentLoadResult()
                {
                    Status = ImageAttachmentStatus.NotFound
                };
            }
        }
    }

    public enum ImageAttachmentStatus
    {
        NotStarted,
        Loaded,

        Downloading,

        /// <summary>
        /// Online account but image not downloaded yet and device is offline
        /// </summary>
        Offline,

        /// <summary>
        /// Image can't be found
        /// </summary>
        NotFound
    }

    public class ImageAttachmentLoadResult
    {
        public ImageAttachmentStatus Status { get; set; }

        /// <summary>
        /// If status was AlreadyDownloaded, this file will be set
        /// </summary>
        public IFile File { get; set; }

        /// <summary>
        /// If status was Downloading, this task will be set
        /// </summary>
        public Task<ImageAttachmentLoadResult> DownloadingResultTask { get; set; }
    }
}
