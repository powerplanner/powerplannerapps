using StorageEverywhere;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class RoamingHelper
    {
        public static Task<IFolder> GetAccountsFolderAsync()
        {
            if (FileSystem.Current.RoamingStorage == null)
                return null;

            return FileSystem.Current.RoamingStorage.CreateFolderAsync("Accounts", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<IFolder> GetAccountFolderAsync(long accountId)
        {
            // Has to use accountId (an online account concept) since local account ID's are different across devices
            if (accountId == 0)
                return null;

            IFolder accountsFolder = await GetAccountsFolderAsync();

            if (accountsFolder == null)
                return null;

            return await accountsFolder.CreateFolderAsync(accountId.ToString(), CreationCollisionOption.OpenIfExists);
        }

        public static async Task<IFolder> GetSavedGradeScalesFolder(long accountId)
        {
            IFolder accountFolder = await GetAccountFolderAsync(accountId);

            if (accountFolder == null)
                return null;

            return await accountFolder.CreateFolderAsync("SavedGradeScales", CreationCollisionOption.OpenIfExists);
        }
    }
}
