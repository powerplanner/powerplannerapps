using System;
using System.Runtime.Serialization;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.TileSettings
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerUWPLibrary.DataLayer.TileSettings")]
    public abstract class BaseUpcomingTileSettings : BindableBase
    {
        private bool _showHomework = true;
        [DataMember]
        public bool ShowHomework
        {
            get { return _showHomework; }
            set { SetProperty(ref _showHomework, value, "ShowHomework"); }
        }

        private bool _showExams = true;
        [DataMember]
        public bool ShowExams
        {
            get { return _showExams; }
            set { SetProperty(ref _showExams, value, "ShowExams"); }
        }

        private int _skipItemsOlderThan = int.MinValue;
        [DataMember]
        public int SkipItemsOlderThan
        {
            get { return _skipItemsOlderThan; }
            set { SetProperty(ref _skipItemsOlderThan, value, "SkipItemsOlderThan"); }
        }

        public bool IsDisabled()
        {
            return !ShowHomework && !ShowExams;
        }

        public DateTime GetDateToStartDisplayingOn(DateTime todayAsUtc)
        {
            if (SkipItemsOlderThan == int.MinValue)
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

            return todayAsUtc.AddDays((SkipItemsOlderThan * -1) + 1);
        }
    }
}
