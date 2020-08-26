using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PowerPlannerUWP.Helpers
{
    public static class UWPNotificationsHelper
    {
        /// <summary>
        /// Handles rare cases like invalid 0x11 character (which usually happens with Chinese or other foreign texts)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripInvalidCharacters(string text)
        {
            // https://stackoverflow.com/questions/21053138/c-sharp-hexadecimal-value-0x12-is-an-invalid-character
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(text, r, "", RegexOptions.Compiled);
        }
    }
}
