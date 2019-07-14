using UpgradeFromWin8.Model.TopItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsUniversal;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ToolsPortable;

namespace UpgradeFromWin8.Model
{
    public class Store
    {
        private static readonly string UPGRADED_TO_VERSION_2 = "UpgradedToVersion2";
        
        private static readonly AsyncLock _lock = new AsyncLock();

        private static StorageFolder _accountsFolder;
        public static async Task<StorageFolder> GetAccountsFolder()
        {
            if (_accountsFolder == null)
            {
                _accountsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AccountsWin", CreationCollisionOption.OpenIfExists);



                if (!AppSettings.Current.ContainsKey(UPGRADED_TO_VERSION_2))
                {
                    try
                    {
                        foreach (StorageFolder account in await _accountsFolder.GetFoldersAsync())
                        {
                            try
                            {
                                string contents;

                                using (Stream s = await account.OpenStreamForReadAsync(Files.ACCOUNT_FILE))
                                {
                                    using (StreamReader r = new StreamReader(s))
                                    {
                                        contents = r.ReadToEnd();
                                    }
                                }

                                string replaced = contents.Replace("http://schemas.datacontract.org/2004/07/ToolsPortableMost", "http://schemas.datacontract.org/2004/07/ToolsUniversal");

                                if (!contents.Equals(replaced))
                                {
                                    using (Stream s = await account.OpenStreamForWriteAsync(Files.ACCOUNT_FILE, CreationCollisionOption.ReplaceExisting))
                                    {
                                        using (StreamWriter w = new StreamWriter(s))
                                        {
                                            w.Write(replaced);
                                        }
                                    }
                                }
                            }

                            catch { }
                        }
                    }

                    catch { }

                    AppSettings.Current[UPGRADED_TO_VERSION_2] = true;
                }
            }

            return _accountsFolder;
        }

        /// <summary>
        /// Will always return initialized folder
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        public static async Task<StorageFolder> GetAccountFolder(Guid localAccountId)
        {
            StorageFolder accountsFolder = await GetAccountsFolder();

            return await accountsFolder.CreateFolderAsync(localAccountId.ToString(), CreationCollisionOption.OpenIfExists);
        }

        private static Dictionary<Guid, LoginWin> _loadedLogins = new Dictionary<Guid, LoginWin>();
        /// <summary>
        /// Ensures there is only one reference to each identifier login
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        private static async Task<LoginWin> getLogin(Guid identifier)
        {
            if (identifier == Guid.Empty)
                return null;

            LoginWin login;

            //if already loaded
            if (_loadedLogins.TryGetValue(identifier, out login))
                return login;

            //grab the account folder
            StorageFolder accountFolder = await GetAccountFolder(identifier);

            //account doesn't exist
            if (accountFolder == null)
                return null;

            //load the login file from account folder
            login = await StorageHelper.Load<LoginWin>(accountFolder, Files.LOGIN_FILE);

            //if login file existed and loaded successfully, cache login
            if (login != null)
            {
                _loadedLogins[identifier] = login;
                login.LocalAccountId = identifier;
            }

            return login;
        }

        private static MyObservableList<LoginWin> _logins;
        public static async System.Threading.Tasks.Task LoadLogins()
        {
            _logins = new MyObservableList<LoginWin>();

            foreach (StorageFolder accountFolder in await (await GetAccountsFolder()).GetFoldersAsync())
            {
                Guid localAccountId;

                if (Guid.TryParse(accountFolder.Name, out localAccountId))
                {
                    LoginWin login = await getLogin(localAccountId);

                    if (login != null)
                        _logins.Add(login);
                }
            }
        }

        public static void Dispose()
        {
            _logins = null;
            _loadedLogins.Clear();
        }

        public static MyObservableList<LoginWin> Logins
        {
            get { return _logins; }
        }

        /// <summary>
        /// Ensures there's only one reference of each account. Creates new account object if account file wasn't found.
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws exception if the folder for the account isn't found.</exception>
        private static async Task<AccountWin> getAccount(LoginWin login)
        {
            if (login.Account != null)
                return login.Account;

            StorageFolder accountFolder = await GetAccountFolder(login.LocalAccountId);

            if (accountFolder == null)
                throw new Exception("The folder for the requested account with id " + login.LocalAccountId + " wasn't found.");



            //using (Stream s = await StorageHelper.LoadStream(accountFolder, Files.ACCOUNT_FILE))
            //{
            //    if (s != null)
            //        UIHandler.ShowMessageBox(new StreamReader(s).ReadToEnd(), "Data");
            //    //return null;
            //}



            AccountWin account = await StorageHelper.Load<AccountWin>(accountFolder, Files.ACCOUNT_FILE);

            if (account == null)
                account = new AccountWin();


            //just in case data was randomly wiped, we'll make sure it syncs everything
            //if (account.School.Years.Count == 0)
            //    account.CurrentChangeNumber = 0;

                //throw new ArgumentException("The account with id " + login.LocalAccountId + " was not found.");

            account.Login = login;
            login.Account = account;

            account.Initialize();
            

            return account;
        }

        private static Guid getLastLoginIdentifier()
        {
            object val = ApplicationData.Current.LocalSettings.Values["LastLogin"];

            if (val == null)
                return Guid.Empty;

            return (Guid)val;
        }
        
        
        public static async System.Threading.Tasks.Task LoadData(LoginWin login)
        {
            await getAccount(login);
        }
        
        
    }
}
