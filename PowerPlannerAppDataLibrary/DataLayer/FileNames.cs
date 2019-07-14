using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public static class FileNames
    {
        public static string[] ACCOUNTS_FOLDER_PATH()
        {
            return new string[] { "Accounts" };
        }

        public static string[] ACCOUNT_FOLDER_PATH(Guid localAccountId)
        {
            return Append(ACCOUNTS_FOLDER_PATH(), localAccountId.ToString());
        }

        public static string[] ACCOUNT_CLASS_TILES_SETTINGS_PATH(Guid localAccountId)
        {
            return Append(ACCOUNT_FOLDER_PATH(localAccountId), ACCOUNT_CLASS_TILES_SETTINGS_FOLDER);
        }

        public const string ACCOUNT_DATABASE_FILE_NAME = "Database.db";

        public const string ACCOUNT_CHANGED_ITEMS_FILE_NAME = "ChangedItems.json";
        public const string TEMP_ACCOUNT_CHANGED_ITEMS_FILE_NAME = "Temp" + ACCOUNT_CHANGED_ITEMS_FILE_NAME;

        public const string ACCOUNT_FILE_NAME = "Account.dat";

        public const string ACCOUNT_IMAGES_FOLDER = "Images";

        public const string ACCOUNT_CLASS_TILES_SETTINGS_FOLDER = "ClassTilesSettings";

        private static string[] Append(string[] existing, string newFolder)
        {
            string[] answer = new string[existing.Length + 1];

            existing.CopyTo(answer, 0);

            answer[answer.Length - 1] = newFolder;

            return answer;
        }
    }
}
