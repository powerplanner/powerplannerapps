using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary
{
    public static class Website
    {
        public const string ClientApiUrl = "https://client.api.powerplanner.net/api/";
        public const string DataApiUrl = "https://data.powerplanner.net/api/";
        public const string UploadApiUrl = "https://upload.powerplanner.net/api/";

        /// <summary>
        /// The root url, like "https://powerplanner.net/"
        /// </summary>
        public const string ROOT_URL = "https://powerplanner.net/";

        /// <summary>
        /// This is set from PowerPlannerApp.Initialize
        /// </summary>
        public static ApiKeyCombo ApiKey { get; set; }


        /// <summary>
        /// Returns something like https://powerplannerstorage.blob.core.windows.net/modern-91353/Images/635121668276610139-58978167.jpg
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string GetImageUrl(long accountId, string image)
        {
            //api/getimagemodern/[AccountId]_[ImageName]
            //return IMAGE_URL + accountId + "_" + image;

            return "https://powerplannerstorage.blob.core.windows.net/modern-" + accountId + "/Images/" + image;
        }
    }
}
