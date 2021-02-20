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
        public static readonly string USERNAME_ERROR = "Usernames must be 50 or fewer characters long, and can only contain letters, numbers, and the special symbols " + string.Join(", ", StringTools.VALID_SPECIAL_URL_CHARS.Intersect(StringTools.VALID_SPECIAL_FILENAME_CHARS));

        public static string GetUsernameError(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return PowerPlannerResources.GetString("UsernameInvalid_Empty");

            if (username.Length > 50)
                return PowerPlannerResources.GetString("UsernameInvalid_TooLong");

            if (username.Contains(' '))
                return PowerPlannerResources.GetString("UsernameInvalid_ContainsSpace");

            if (StringTools.IsStringFilenameSafe(username) && StringTools.IsStringUrlSafe(username))
                return null;

            var characters = username.ToCharArray().Distinct().ToArray();
            var validSpecialChars = StringTools.VALID_SPECIAL_FILENAME_CHARS.Intersect(StringTools.VALID_SPECIAL_URL_CHARS).ToArray();

            var validCharacters = characters.Where(i => Char.IsLetterOrDigit(i) || validSpecialChars.Contains(i)).ToArray();
            var invalidCharacters = characters.Except(validCharacters).ToArray();

            return PowerPlannerResources.GetStringWithParameters("UsernameInvalid_InvalidCharacters", string.Join(", ", invalidCharacters));
        }
    }
}
