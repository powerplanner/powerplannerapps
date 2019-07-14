using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public static class Credentials
    {
        public static bool IsUsernameOkay(string username)
        {
            if (username == null || username.Length == 0)
                return false;

            if (username.Length > 50)
                return false;

            if (StringTools.IsStringFilenameSafe(username) && StringTools.IsStringUrlSafe(username))
                return true;

            return false;
        }

        public static readonly string USERNAME_ERROR = "Usernames must be 50 or fewer characters long, and can only contain letters, numbers, and the special symbols " + StringTools.ToString(StringTools.VALID_SPECIAL_URL_CHARS, ", ");
    }
}
