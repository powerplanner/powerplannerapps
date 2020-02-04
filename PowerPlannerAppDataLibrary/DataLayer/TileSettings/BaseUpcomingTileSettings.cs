using System;
using System.Runtime.Serialization;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.TileSettings
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerUWPLibrary.DataLayer.TileSettings")]
    public abstract class BaseUpcomingTileSettings : BindableBase
    {
        [DataMember]
        public bool ShowHomework { get; set; } = true;

        [DataMember]
        public bool ShowExams { get; set; } = true;

        [DataMember]
        public int SkipItemsOlderThan { get; set; } = int.MinValue;

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
