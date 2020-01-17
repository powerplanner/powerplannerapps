using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.Storage;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public class LoginWin : BindableBase
    {
        private StorageFolder _imagesFolder;
        public async Task<StorageFolder> GetImagesFolder()
        {
            if (_imagesFolder == null)
            {
                StorageFolder accountFolder = await Store.GetAccountFolder(LocalAccountId);

                _imagesFolder = await accountFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
            }

            return _imagesFolder;
        }

        /// <summary>
        /// Not stored, since it'll be obtained via its parents file name
        /// </summary>
        public Guid LocalAccountId;

        private int _deviceId;
        [DataMember]
        public int DeviceId
        {
            get { return _deviceId; }
            set { SetProperty(ref _deviceId, value, "DeviceId", "IsOnlineAccount"); }
        }

        private long _accountId;
        [DataMember]
        public long AccountId
        {
            get { return _accountId; }
            set { SetProperty(ref _accountId, value, "AccountId", "IsOnlineAccount"); }
        }

        private string _username;
        [DataMember]
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, "Username"); }
        }

        private string _password;
        [DataMember]
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, "Password"); }
        }

        private bool _rememberUsername;
        [DataMember]
        public bool RememberUsername
        {
            get { return _rememberUsername; }
            set
            {
                SetProperty(ref _rememberUsername, value, "RememberUsername", "IsAutoLoginPossible");

                if (value == false)
                    AutoLogin = false;
            }
        }

        private bool _rememberPassword;
        [DataMember]
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set
            {
                SetProperty(ref _rememberPassword, value, "RememberPassword", "IsAutoLoginPossible");

                if (value == false)
                    AutoLogin = false;
            }
        }

        private bool _autoLogin;
        [DataMember]
        public bool AutoLogin
        {
            get { return _autoLogin; }
            set { SetProperty(ref _autoLogin, value, "AutoLogin"); }
        }

        public bool IsAutoLoginPossible
        {
            get { return RememberUsername && RememberPassword; }
        }

        public AccountWin Account { get; internal set; }

        public bool IsOnlineAccount
        {
            get { return AccountId != 0 && DeviceId != 0; }
        }

        public LoginCredentials GenerateCredentials()
        {
            return new LoginCredentials()
            {
                Username = Username,
#pragma warning disable CS0618 // Type or member is obsolete
                Password = Password,
#pragma warning restore CS0618 // Type or member is obsolete
                AccountId = AccountId
            };
        }
    }
}
