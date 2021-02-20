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
            switch (option)
            {
                case ImageUploadOptions.Always:
                    return PowerPlannerResources.GetString("String_Always");

                case ImageUploadOptions.Never:
                    return PowerPlannerResources.GetString("String_Never");

                case ImageUploadOptions.WifiOnly:
                    return PowerPlannerResources.GetString($"ImageUploadOptions_WifiOnly");

                default:
                    return option.ToString();
            }
        }
    }
}
