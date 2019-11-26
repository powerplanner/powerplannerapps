using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPImagePickerExtension : ImagePickerExtension
    {
        public override async Task<IFile[]> PickImagesAsync()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".gif");

            List<IFile> answer = new List<IFile>();
            foreach (StorageFile file in await picker.PickMultipleFilesAsync())
            {
                // Copy to local temp file
                string fileName = $"{Guid.NewGuid()}{file.FileType}";
                var tempFile = await TempFile.CreateAsync(fileName);
                using (var fileStream = await tempFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                {
                    using (var originalStream = await file.OpenStreamForReadAsync())
                    {
                        await originalStream.CopyToAsync(fileStream);
                    }
                }
                answer.Add(tempFile);
            }
            return answer.ToArray();
        }
    }
}
