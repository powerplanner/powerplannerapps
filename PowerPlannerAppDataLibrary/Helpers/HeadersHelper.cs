using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class HeadersHelper
    {
        public static void SetLoginCredentials(HttpContentHeaders headers, AccountDataItem account)
        {
            headers.Add("AccountId", account.AccountId.ToString());
            headers.Add("Username", account.Username);
            headers.Add("Session", account.Token);
        }

        public static void SetApiKey(HttpContentHeaders headers)
        {
            headers.Add("HashedKey", Website.ApiKey.HashedKey);
        }
    }
}
