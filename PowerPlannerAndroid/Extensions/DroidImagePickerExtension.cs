using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using StorageEverywhere;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidImagePickerExtension : ImagePickerExtension
    {
        public override async Task<IFile[]> PickImagesAsync()
        {
            using (var result = await PicturePickerHelper.GetImageStreamAsync())
            {
                if (result != null)
                {
                    // Copy to local temp file
                    var tempFile = await TempFile.CreateAsync($"{Guid.NewGuid()}.{result.Extension}");
                    using (var fileStream = await tempFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                    {
                        await result.Stream.CopyToAsync(fileStream);
                    }
                    return new IFile[] { tempFile };
                }
            }

            return new IFile[0];
        }
    }
}