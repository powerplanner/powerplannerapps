using PCLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class ImagePickerExtension
    {
        public static ImagePickerExtension Current { get; set; }

        /// <summary>
        /// Returns a list of temporary files the user picked.
        /// </summary>
        /// <returns></returns>
        public abstract Task<IFile[]> PickImagesAsync();
    }
}
