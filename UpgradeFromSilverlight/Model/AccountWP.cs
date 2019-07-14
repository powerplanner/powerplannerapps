using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using ToolsPortable;
using UpgradeFromSilverlight.Sections;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class AccountWP
    {
        /// <summary>
        /// This will be triggered when a change to ANY semester has been made that might affect schedules.
        /// It is only triggered once per each save, not for each individual item change.
        /// </summary>
        public event EventHandler OnSchedulesChanged;

        internal void triggerOnSchedulesChanged()
        {
            if (OnSchedulesChanged != null)
                OnSchedulesChanged(this, null);
        }

        public AccountWP()
        {
            initialize();
        }

        /// <summary>
        /// Empty constructor for testing
        /// </summary>
        /// <param name="test"></param>
        public AccountWP(bool test)
        {

        }

        [OnDeserialized]
        public void _onDeserialized(StreamingContext context)
        {
            initialize();
        }

        private void initialize()
        {
            _changedItems = new Dictionary<Guid, BaseItem>();
        }

        public static AccountWP Create(string username, string encryptedPassword, bool autoLogin, bool rememberUsername, bool rememberPassword, long accountId = 0, int deviceId = 0)
        {
            return new AccountWP()
            {
                Username = username,
                Password = encryptedPassword,
                AutoLogin = autoLogin,
                RememberUsername = rememberUsername,
                RememberPassword = rememberPassword,
                AccountId = accountId,
                DeviceId = deviceId
            };
        }

        public AccountSection AccountSection { get; internal set; }

        public LoginCredentials GenerateLoginCredentials()
        {
            return new LoginCredentials()
            {
                AccountId = AccountId,
                Username = Username,
                Password = Password
            };
        }
        

        [DataMember]
        public Guid LocalId { get; set; }
        
        [DataMember]
        public int DeviceId { get; set; }
        
        [DataMember]
        public string Username { get; set; }
        
        [DataMember]
        public string Password { get; set; }
        
        [DataMember]
        public long AccountId { get; set; }
        
        [DataMember]
        public bool AutoLogin { get; set; } = true;

        [DataMember]
        public bool RememberUsername { get; set; } = true;

        [DataMember]
        public bool RememberPassword { get; set; } = true;
        
        [DataMember]
        public int CurrentChangeNumber { get; set; }

        #region Settings
        
        /// <summary>
        /// In case user exits or is offline when they modify settings, I'll remember that I need to sync them
        /// </summary>
        [DataMember]
        public bool NeedsToSyncSettings { get; set; }
        
        [DataMember]
        public GpaOptions GpaOption { get; set; }
        
        [DataMember]
        public DateTime WeekOneStartsOn { get; set; }
        
        [DataMember]
        public bool RemindersDayBefore { get; set; }
        
        [DataMember]
        public bool RemindersDayOf { get; set; }
        
        [DataMember]
        public ImageUploadOptions ImageUploadOption { get; set; } = ImageUploadOptions.WifiOnly;

        [DataMember]
        public bool ShowHomeworkOnTiles { get; set; } = true;
        
        [DataMember]
        public bool ShowExamsOnTiles { get; set; } = true;

        [DataMember]
        public int IgnoreItemsOlderThan { get; set; }





        [DataMember]
        public int _versionMajor;
        [DataMember]
        public int _versionMinor;
        [DataMember]
        public int _versionBuild;
        [DataMember]
        public int _versionRevision;

        public Version Version
        {
            get
            {
                return new Version(_versionMajor, _versionMinor, _versionBuild, _versionRevision);
            }

            set
            {
                _versionMajor = value.Major;
                _versionMinor = value.Minor;
                _versionBuild = value.Build;
                _versionRevision = value.Revision;
            }
        }

        #endregion

        public bool PasswordEquals(string other)
        {
            return Password.Equals(other);
        }


        [DataMember(Name = "Changes")]
        public Changes _oldChanges;

        private PartialChanges _partialChanges;
        public PartialChanges PartialChanges
        {
            get
            {
                if (_partialChanges == null)
                    loadPartialChanges();

                return _partialChanges;
            }

            set { _partialChanges = value; }
        }

        private void loadPartialChanges()
        {
            bool upgradedFromOld = false;

            try
            {
                using (Stream s = IMyStorage.LoadStream(Files.PARTIAL_CHANGES_FILE(LocalId)))
                {
                    _partialChanges = new DataContractJsonSerializer(typeof(PartialChanges)).ReadObject(s) as PartialChanges;
                }
            }

            //not found, try upgrading from old version
            catch
            {
                _partialChanges = new PartialChanges();

                if (_oldChanges != null)
                {
                    foreach (var pair in _oldChanges._changedItems)
                    {
                        if (pair.Value.IsDeleted)
                            PartialChanges.Delete(pair.Key);
                        else
                            PartialChanges.New(pair.Key);
                    }

                    upgradedFromOld = true;
                }
            }

            if (upgradedFromOld)
            {
                //make the old Changes null so it doesn't load/waste space
                _oldChanges = null;
            }
        }

        [DataMember(Name="_imagesToUpload")]
        public LinkedList<string> StoredImagesToUpload = new LinkedList<string>();

        
        
        public int Offset; //this isn't serialized since it needs to get a new offset every time app is loaded

        public IEnumerable<BaseItem> GetAllItemsInSendingFormat()
        {
            foreach (var child in this.School.GetAllChildren())
            {
                yield return child.Serialize(0);
            }
        }

        public BaseItemWP Find(Guid identifier)
        {
            return AccountSection.Find(identifier);
        }

        /// <summary>
        /// Initialized with initialize()
        /// </summary>
        private Dictionary<Guid, BaseItem> _changedItems;



        #region School

        

        


        #region DisplayLists

        [DataMember]
        public SchoolWP School = new SchoolWP();

        #endregion


        #endregion



        
        
        [DataMember]
        public bool IsPushEnabled { get; set; } = true;

        public bool IsOnlineAccount
        {
            get { return AccountId != 0 && DeviceId != 0; }
        }
        
        /// <summary>
        /// Is always UTC time
        /// </summary>
        [DataMember]
        public DateTime PremiumAccountExpiresOn { get; set; } = DateTime.SpecifyKind(SqlDate.MinValue, DateTimeKind.Utc);

        public bool IsLifetimePremiumAccount
        {
            get { return PremiumAccountExpiresOn == PowerPlannerSending.DateValues.LIFETIME_PREMIUM_ACCOUNT; }
        }
    }
}
