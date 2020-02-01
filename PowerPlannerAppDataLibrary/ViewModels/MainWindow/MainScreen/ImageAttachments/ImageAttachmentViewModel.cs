using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments
{
    public class ImageAttachmentViewModel : BindableBase
    {
        public string ImageName { get; private set; }

        internal ImageAttachmentViewModel(string imageName)
        {
            ImageName = imageName;
        }

        /// <summary>
        /// Constructor used for adding a new temp image that hasn't been saved yet
        /// </summary>
        /// <param name="tempFile"></param>
        internal ImageAttachmentViewModel(IFile tempFile)
        {
            ImageName = tempFile.Name;
            File = tempFile;
            Status = ImageAttachmentStatus.Loaded;
            _hasStartedLoad = true;
        }

        /// <summary>
        /// Views should subscribe to status and change image based on that, not based on the file
        /// </summary>
        public ImageAttachmentStatus Status { get; set; } = ImageAttachmentStatus.NotStarted;

        private bool _hasStartedLoad;
        public async void StartLoad()
        {
            try
            {
                if (_hasStartedLoad)
                {
                    return;
                }
                _hasStartedLoad = true;
                var result = await ImageAttachmentHelpers.GetOrStartDownloadAsync(ImageName);
                File = result.File;
                Status = result.Status;

                if (result.DownloadingResultTask != null)
                {
                    var downloadResult = await result.DownloadingResultTask;
                    File = downloadResult.File;
                    Status = downloadResult.Status;
                }
                else
                {
                    File = result.File;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public IFile File { get; private set; }
    }
}
