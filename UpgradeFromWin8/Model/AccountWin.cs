using UpgradeFromWin8.Model.TopItems;
using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.ApplicationModel.Core;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.UI.Core;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public class AccountWin : BindableBase
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

        public AccountWin()
        {
            initialize();
            
            School = new SchoolWP();
        }

        public LoginWin Login { get; internal set; }

        /// <summary>
        /// Empty constructor for testing
        /// </summary>
        /// <param name="test"></param>
        public AccountWin(bool test)
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

        public LoginCredentials GenerateLoginCredentials()
        {
            return Login.GenerateCredentials();
        }


        [DataMember(Name = "Version2")]
        public Version Version;

        private int _currentChangeNumber;
        [DataMember]
        public int CurrentChangeNumber
        {
            get { return _currentChangeNumber; }
            set { SetProperty(ref _currentChangeNumber, value, "CurrentChangeNumber"); }
        }
        

        private GpaOptions _gpaOption;
        [DataMember]
        public GpaOptions GpaOption
        {
            get { return _gpaOption; }
            set { SetProperty(ref _gpaOption, value, "GpaOption"); }
        }

        private DateTime _weekOneStartsOn = DateTools.Last(DayOfWeek.Sunday, DateTime.UtcNow);
        [DataMember]
        public DateTime WeekOneStartsOn
        {
            get { return _weekOneStartsOn; }
            set { SetProperty(ref _weekOneStartsOn, DateTime.SpecifyKind(value, DateTimeKind.Utc), "WeekOneStartsOn", "CurrentWeek", "WeekChangesOn"); }
        }

        public DayOfWeek WeekChangesOn
        {
            get { return WeekOneStartsOn.DayOfWeek; }
        }

        /// <summary>
        /// Automatically sets WeekOneStartsOn
        /// </summary>
        public Schedule.Week CurrentWeek
        {
            get
            {
                if (MyMath.Mod((int)(DateTime.UtcNow - DateTime.SpecifyKind(WeekOneStartsOn.Date, DateTimeKind.Utc)).TotalDays, 14) < 7)
                    return Schedule.Week.WeekOne;

                return Schedule.Week.WeekTwo;
            }
        }
        

        [DataMember(Name = "Changes")]
        public Changes _oldChanges;

        private PartialChanges partialChanges;

        private static readonly string PARTIAL_CHANGES_FILENAME = "PartialChanges.json";
        private StorageFolder accountFolder;
        private async System.Threading.Tasks.Task loadAccountFolder()
        {
            accountFolder = await Store.GetAccountFolder(Login.LocalAccountId);
        }

        public async Task<PartialChanges> GetPartialChanges()
        {
            await loadPartialChanges();

            return partialChanges;
        }

        private async System.Threading.Tasks.Task loadPartialChanges()
        {
            if (accountFolder == null)
                await loadAccountFolder();

            bool upgradedFromOld = false;

            try
            {
                using (Stream s = await accountFolder.OpenStreamForReadAsync(PARTIAL_CHANGES_FILENAME))
                {
                    partialChanges = new DataContractJsonSerializer(typeof(PartialChanges)).ReadObject(s) as PartialChanges;
                }
            }

            //not found, try upgrading from old version
            catch
            {
                partialChanges = new PartialChanges();

                if (_oldChanges != null)
                {
                    foreach (var pair in _oldChanges._changedItems)
                    {
                        if (pair.Value.IsDeleted)
                            partialChanges.Delete(pair.Key);
                        else
                            partialChanges.New(pair.Key);
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
        


        private HashSet<string> _imagesToUpload;
        private static readonly string IMAGES_TO_UPLOAD_FILENAME = "ImagesToUpload.dat";
        private async System.Threading.Tasks.Task loadImagesToUpload()
        {
            if (accountFolder == null)
                await loadAccountFolder();

            try
            {
                using (Stream s = await accountFolder.OpenStreamForReadAsync(IMAGES_TO_UPLOAD_FILENAME))
                {
                    _imagesToUpload = (HashSet<string>)new DataContractSerializer(typeof(HashSet<string>)).ReadObject(s);
                }
            }

            catch { _imagesToUpload = new HashSet<string>(); }
        }
        
        

        public async Task<IEnumerable<string>> GetAllImagesToUpload()
        {
            if (_imagesToUpload == null)
                await loadImagesToUpload();

            return _imagesToUpload;
        }

        

        /// <summary>
        /// Assigns updated time, makes sure it's under this account
        /// </summary>
        /// <param name="item"></param>
        private void assignInfo(BaseItem item)
        {
            //assign updated time
            item.Updated = DateTime.UtcNow;

            //make sure it's under this account
            //item.Account = this;
        }

        /// <summary>
        /// Initialized with initialize()
        /// </summary>
        private Dictionary<Guid, BaseItem> _changedItems;
        


        #region School






        #region DisplayLists

        [DataMember]
        public SchoolWP School { get; set; }

        #endregion


        #endregion


        

        public BaseItemWin Find(Guid identifier)
        {
            return School.Find(identifier);
        }

        internal void Initialize()
        {
            School.Initialize(null);
        }

        private DateTime _premiumAccountExpiresOn = DateTime.SpecifyKind(SqlDate.MinValue, DateTimeKind.Utc);
        /// <summary>
        /// Is always UTC time
        /// </summary>
        [DataMember]
        public DateTime PremiumAccountExpiresOn
        {
            get { return _premiumAccountExpiresOn; }
            set { SetProperty(ref _premiumAccountExpiresOn, value.ToUniversalTime(), "PremiumAccountExpiresOn"); }
        }

        public bool IsLifetimePremiumAccount
        {
            get { return PremiumAccountExpiresOn == PowerPlannerSending.DateValues.LIFETIME_PREMIUM_ACCOUNT; }
        }





        public BaseItem[] GetAllItemsInSendingFormat()
        {
            return School.GetAllChildren().Select(i => i.Serialize()).ToArray();
        }
    }
}
