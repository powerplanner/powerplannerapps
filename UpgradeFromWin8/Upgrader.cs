using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradeFromWin8.Model;
using Windows.Storage;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;

namespace UpgradeFromWin8
{
    public class Upgrader
    {
        /// <summary>
        /// Will not throw any exceptions, it'll silently fail if error. Deletes all old data once done.
        /// </summary>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task UpgradeAccounts()
        {
            try
            {
                // If there's already accounts in the app, do nothing
                if ((await AccountsManager.GetAllAccounts()).Any())
                    return;

                await Store.LoadLogins();

                foreach (LoginWin login in Store.Logins)
                {
                    try
                    {
                        await UpgradeAccount(login);
                    }

                    catch (Exception ex)
                    {
                        try
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }

                        catch { }
                    }
                }
            }

            catch (Exception ex)
            {
                try
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                catch { }
            }

            finally
            {
                await DeleteOldData();
            }
        }

        private static async System.Threading.Tasks.Task DeleteOldData()
        {
            try
            {
                var folder = await Store.GetAccountsFolder();
                await folder.DeleteAsync();
            }

            catch { }

            try
            {
                var mainTileSettings = await ApplicationData.Current.LocalFolder.GetFileAsync("MainTileSettings.dat");
                await mainTileSettings.DeleteAsync();
            }

            catch { }
        }

        private static async System.Threading.Tasks.Task UpgradeAccount(LoginWin login)
        {
            await Store.LoadData(login);

            AccountWin account = login.Account;

            AccountDataItem accountUWP = new AccountDataItem(login.LocalAccountId)
            {
                AccountId = login.AccountId,
                AutoLogin = login.AutoLogin,
                CurrentChangeNumber = account.CurrentChangeNumber,
                CurrentSemesterId = account.School.ActiveSemesterIdentifier,
                DeviceId = login.DeviceId,
                LocalAccountId = login.LocalAccountId,
                LocalToken = login.Password,
                PremiumAccountExpiresOn = account.PremiumAccountExpiresOn,
                RememberPassword = login.RememberPassword,
                RememberUsername = login.RememberUsername,
                Username = login.Username,
                WeekOneStartsOn = account.WeekOneStartsOn,
                Version = account.Version
            };

            // Save account (need to do this so there's a folder for everything else)
            await AccountsManager.Save(accountUWP);

            try
            {
                AccountDataStore dataStoreUWP = await AccountDataStore.Get(login.LocalAccountId);

                // Transfer the Changes
                PartialChanges existingChanges = await account.GetPartialChanges();
                await AccountDataStore.ChangedItems.ImportChangesAsync(login.LocalAccountId, existingChanges.Changes.Keys.ToArray(), existingChanges.Deletes.Keys.ToArray());

                try
                {
                    // Transfer images to upload
                    string[] imagesToUpload = (await account.GetAllImagesToUpload()).ToArray();
                    await dataStoreUWP.AddImagesToUploadAsync(imagesToUpload);

                    // Transfer stored images
                    StorageFolder oldImagesFolder = await login.GetImagesFolder();
                    IFolder newImagesFolderPortable = await PowerPlannerAppDataLibrary.DataLayer.FileHelper.GetOrCreateImagesFolder(login.LocalAccountId);
                    StorageFolder newImagesFolder = await StorageFolder.GetFolderFromPathAsync(newImagesFolderPortable.Path);

                    foreach (StorageFile existingImage in await oldImagesFolder.GetFilesAsync())
                        await existingImage.MoveAsync(newImagesFolder);
                }

                catch { }

                // Get all the existing items
                BaseItem[] syncItems = account.GetAllItemsInSendingFormat();

                // Translate them to the universal sync language
                BaseDataItem[] uwpItems = PowerPlannerAppDataLibrary.SyncLayer.Sync.GetSyncItemsAsDataItems(syncItems);

                // And then input those into the new database
                await dataStoreUWP.ImportItemsAsync(uwpItems);
            }

            catch (Exception ex)
            {
                try
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                catch { }

                await AccountsManager.Delete(login.LocalAccountId);
            }
        }
    }
}
