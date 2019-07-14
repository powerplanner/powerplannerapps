using PowerPlannerSending;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpgradeFromSilverlight.Model;
using UpgradeFromSilverlight.Sections;
using Windows.Storage;
using Windows.UI.StartScreen;
using PCLStorage;
using PowerPlannerAppDataLibrary.Extensions;

namespace UpgradeFromSilverlight
{
    public class SilverlightUpgrader
    {
        private static IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

        public static async System.Threading.Tasks.Task UpgradeAccounts()
        {
            try
            {
                // If there's already accounts in the app, do nothing
                if (!(await AccountsManager.GetAllAccounts()).Any())
                {
                    foreach (string dir in GetDirectoryNames(Files.ACCOUNTS_FOLDER))
                    {
                        await UpgradeAccount(dir);
                    }


                    // We theoretically should be able to retrieve the old settings
                    // according to this post: https://www.pedrolamas.com/2014/08/14/upgrading-from-the-isolatedstoragesettings-to-applicationdata-localsettings/
                    // but it's easier to just set the single account as logged in
                    // since the code from that website didn't work perfectly, and not worth the effort


                    var accounts = await AccountsManager.GetAllAccounts();

                    // If they only had one account, we'll set it as last login
                    // We unfortunately can't transfer the Silverlight settings unfortunately
                    // so this is the best thing we can do.
                    if (accounts.Count == 1)
                    {
                        var onlyAccount = accounts.First();

                        AccountsManager.SetLastLoginIdentifier(onlyAccount.LocalAccountId);
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            // Secondary Silverlight tiles should be automatically disappear, but don't due to a bug
            DeleteOldData();
        }

        private static PowerPlannerAppDataLibrary.DataLayer.ImageUploadOptions ConvertImageUploadOption(Model.ImageUploadOptions option)
        {
            switch (option)
            {
                case Model.ImageUploadOptions.Always:
                    return PowerPlannerAppDataLibrary.DataLayer.ImageUploadOptions.Always;

                case Model.ImageUploadOptions.Never:
                    return PowerPlannerAppDataLibrary.DataLayer.ImageUploadOptions.Never;

                default:
                    return PowerPlannerAppDataLibrary.DataLayer.ImageUploadOptions.WifiOnly;
            }
        }

        private static async System.Threading.Tasks.Task UpgradeAccount(string dir)
        {
            try
            {
                var accountSection = AccountSection.Get(Guid.Parse(dir)).Value;

                // Null means that data deserialization failed for some reason
                if (accountSection == null)
                    return;

                AccountDataItem accountUWP = new AccountDataItem(accountSection.LocalId)
                {
                    AccountId = accountSection.AccountId,
                    AutoLogin = accountSection.AutoLogin,
                    CurrentChangeNumber = accountSection.CurrentChangeNumber,
                    CurrentSemesterId = accountSection.School.ActiveSemesterIdentifier,
                    DeviceId = accountSection.DeviceId,
                    Token = accountSection.Password,
                    PremiumAccountExpiresOn = accountSection.PremiumAccountExpiresOn,
                    RememberPassword = accountSection.RememberPassword,
                    RememberUsername = accountSection.RememberUsername,
                    Username = accountSection.Username,
                    WeekOneStartsOn = accountSection.WeekOneStartsOn,
                    Version = accountSection.Version,
                    ImageUploadOption = ConvertImageUploadOption(accountSection.ImageUploadOption),
                    IsPushDisabled = !accountSection.IsPushEnabled,
                    NeedsToSyncSettings = accountSection.NeedsToSyncSettings,
                    RemindersDayBefore = accountSection.RemindersDayBefore,
                    RemindersDayOf = accountSection.RemindersDayOf
                };

                accountUWP.MainTileSettings.ShowExams = accountSection.ShowExamsOnTiles;
                accountUWP.MainTileSettings.ShowHomework = accountSection.ShowHomeworkOnTiles;
                accountUWP.MainTileSettings.SkipItemsOlderThan = accountSection.IgnoreItemsOlderThan;

                // Save account (need to do this so there's a folder for everything else)
                await AccountsManager.Save(accountUWP);

                try
                {
                    AccountDataStore dataStoreUWP = await AccountDataStore.Get(accountUWP.LocalAccountId);

                    // Transfer the changes
                    await AccountDataStore.ChangedItems.ImportChangesAsync(accountUWP.LocalAccountId, accountSection.PartialChanges.Changes.Keys.ToArray(), accountSection.PartialChanges.Deletes.Keys.ToArray());

                    try
                    {
                        // Transfer images to upload
                        string[] imagesToUpload = accountSection.StoredImagesToUpload.ToArray();
                        await dataStoreUWP.AddImagesToUploadAsync(imagesToUpload);


                        // Transfer stored imagesDataContract
                        IFolder newImagesFolder = await PowerPlannerAppDataLibrary.DataLayer.FileHelper.GetOrCreateImagesFolder(accountUWP.LocalAccountId);


                        foreach (string imageName in accountSection.StoredImagesToUpload)
                        {
                            string path = Files.IMAGE_FILE_NAME(imageName, accountUWP.LocalAccountId);

                            // Try transfering the image
                            try
                            {
                                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    using (IsolatedStorageFileStream existingStream = store.OpenFile(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                                    {
                                        var newFile = await newImagesFolder.CreateFileAsync(imageName, PCLStorage.CreationCollisionOption.ReplaceExisting);
                                        using (Stream newStream = await newFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                                        {
                                            existingStream.CopyTo(newStream);
                                        }
                                    }
                                }
                            }

                            catch (Exception ex)
                            {
                                TelemetryExtension.Current?.TrackException(ex);
                            }
                        }
                    }

                    catch { }


                    // Get all the existing items in universal sync language
                    BaseItem[] syncItems = accountSection.GetAllItemsInSendingFormat().ToArray();

                    // Convert them to the UWP data format
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

                    await AccountsManager.Delete(accountUWP.LocalAccountId);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Debug.WriteLine("Failed upgrading Silverlight account");
            }
        }

        private static string[] GetDirectoryNames(string searchPattern)
        {
            try
            {
                return store.GetDirectoryNames(searchPattern);
            }

            catch { return new string[0]; }
        }

        private static void DeleteOldData()
        {
            try
            {
                IMyStorage.DeleteDirectory(Files.ACCOUNTS_FOLDER);
            }

            catch
            {
                Debug.WriteLine("Failed to delete old Silverlight directory");
            }
        }
    }
}
