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
#if DEBUG
        public static readonly string URL = "https://powerplanner.net/api/";
        //public static readonly string URL = "http://powerplannerapp-staging.azurewebsites.net/api/";
        //public static readonly string URL = "http://localhost:55458/api/";
#else
        public static readonly string URL = "https://powerplanner.net/api/";
#endif

        /// <summary>
        /// The root url, like "https://powerplanner.net/"
        /// </summary>
        public const string ROOT_URL = "https://powerplanner.net/";

        /// <summary>
        /// This is set from PowerPlannerApp.Initialize
        /// </summary>
        public static ApiKeyCombo ApiKey { get; set; }

        public static async Task<ForgotUsernameResponse> ForgotUsername(string email)
        {
            return await WebHelper.Download<ForgotUsernameRequest, ForgotUsernameResponse>(URL + "forgotusernamemodern", new ForgotUsernameRequest()
            {
                Email = email
            }, ApiKey);
        }


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
