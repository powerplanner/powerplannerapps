using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StorageEverywhere;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    /// <summary>
    /// Every call should be from a background thread, because this uses locking which will lock the current thread
    /// </summary>
    public static class FileHelper
    {
        private static IFolder _accountsFolder;

        /// <summary>
        /// Gets or creates the root Accounts folder
        /// </summary>
        /// <returns></returns>
        public static async Task<IFolder> GetOrCreateAccountsFolder()
        {
            if (_accountsFolder == null)
            {
                _accountsFolder = await Task.Run(async delegate
                {
                    return await FileSystem.Current.LocalStorage.CreateFolderByPathAsync(FileNames.ACCOUNTS_FOLDER_PATH());
                });
            }

            return _accountsFolder;
        }

        public static async Task<IList<IFolder>> GetAllAccountFolders()
        {
            IFolder accountsFolder = await GetOrCreateAccountsFolder();

            return await accountsFolder.GetFoldersAsync();
        }

        public static async Task<IFolder> GetOrCreateAccountFolder(Guid localAccountId)
        {
            return await FileSystem.Current.LocalStorage.CreateFolderByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId));
        }

        public static async Task<IFolder> GetOrCreateClassTilesSettingsFolder(Guid localAccountId)
        {
            return await FileSystem.Current.LocalStorage.CreateFolderByPathAsync(FileNames.ACCOUNT_CLASS_TILES_SETTINGS_PATH(localAccountId));
        }

        public static async Task<IFolder> GetAccountFolder(Guid localAccountId)
        {
            try
            {
                return await FileSystem.Current.LocalStorage.GetFolderByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId));
            }

            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<IFolder> GetOrCreateImagesFolder(Guid localAccountId)
        {
            IFolder accountFolder = await GetAccountFolder(localAccountId);

            if (accountFolder == null)
                return null;

            return await accountFolder.CreateFolderAsync(FileNames.ACCOUNT_IMAGES_FOLDER, CreationCollisionOption.OpenIfExists);
        }

        /// <summary>
        /// Returns null if the account doesn't exist.
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        public static async Task<IFolder> GetSavedGradeScalesFolder(Guid localAccountId)
        {
            IFolder accountFolder = await GetAccountFolder(localAccountId);

            if (accountFolder == null)
                return null;

            return await accountFolder.CreateFolderAsync("SavedGradeScales", CreationCollisionOption.OpenIfExists);
        }

        public static Task<IFolder> CreatePartialSyncsFolder()
        {
            return FileSystem.Current.LocalStorage.CreateFolderAsync("PartialSyncs", CreationCollisionOption.ReplaceExisting);
        }

        public static async Task DeletePartialSyncsFolder()
        {
            try
            {
                var folder = await FileSystem.Current.LocalStorage.GetFolderAsync("PartialSyncs");
                if (folder != null)
                {
                    await folder.DeleteAsync();
                }
            }
            catch { }
        }

        public static async Task<IFile> CreateFileByPathAsync(this IFolder startingFolder, string[] path, string fileName, CreationCollisionOption collisionOption)
        {
            IFolder folder = await startingFolder.CreateFolderByPathAsync(path);

            return await folder.CreateFileAsync(fileName, collisionOption);
        }

        public static async Task<IFolder> CreateFolderByPathAsync(this IFolder startingFolder, string[] path)
        {
            IFolder currFolder = startingFolder;

            for (int i = 0; i < path.Length; i++)
            {
                currFolder = await currFolder.CreateFolderAsync(path[i], CreationCollisionOption.OpenIfExists);
            }

            return currFolder;
        }

        public static Task<IFile> GetFileByPathAsync(this IFolder startingFolder, string[] path, string fileName)
        {
            string finalPath = Path.Combine(startingFolder.Path, Path.Combine(path), fileName);
            return FileSystem.Current.GetFileFromPathAsync(finalPath);
        }

        /// <summary>
        /// Returns the folder under the specified path if it exists. Otherwise throws FileNotFound exception (same stuff as GetFolderAsync).
        /// </summary>
        /// <param name="startingFolder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Task<IFolder> GetFolderByPathAsync(this IFolder startingFolder, string[] path)
        {
            string finalPath = Path.Combine(startingFolder.Path, Path.Combine(path));
            return FileSystem.Current.GetFolderFromPathAsync(finalPath);
        }
    }
}
