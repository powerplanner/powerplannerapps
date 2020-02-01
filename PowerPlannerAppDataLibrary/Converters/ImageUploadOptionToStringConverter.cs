using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class ImageUploadOptionToStringConverter
    {
        public static string Convert(ImageUploadOptions option)
        {
            return PowerPlannerResources.GetString($"ImageUploadOptions_{option.ToString()}");
        }
    }
}
