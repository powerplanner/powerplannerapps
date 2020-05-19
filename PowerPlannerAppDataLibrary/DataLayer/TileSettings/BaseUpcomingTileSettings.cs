using System;
using System.Runtime.Serialization;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.TileSettings
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerUWPLibrary.DataLayer.TileSettings")]
    public abstract class BaseUpcomingTileSettings : BindableBase
    {
        private bool _showTasks = true;
        [DataMember]
        public bool ShowTasks
        {
            get { return _showTasks; }
            set { SetProperty(ref _showTasks, value, nameof(ShowTasks)); }
        }

        private bool _showEvents = true;
        [DataMember]
        public bool ShowEvents
        {
            get { return _showEvents; }
            set { SetProperty(ref _showEvents, value, nameof(ShowEvents)); }
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
            return !ShowTasks && !ShowEvents;
        }

        public DateTime GetDateToStartDisplayingOn(DateTime todayAsUtc)
        {
            if (SkipItemsOlderThan == int.MinValue)
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

            return todayAsUtc.AddDays((SkipItemsOlderThan * -1) + 1);
        }
    }
}
