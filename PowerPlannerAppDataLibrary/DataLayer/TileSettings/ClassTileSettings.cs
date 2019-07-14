using System.Runtime.Serialization;

namespace PowerPlannerAppDataLibrary.DataLayer.TileSettings
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerUWPLibrary.DataLayer.TileSettings")]
    public class ClassTileSettings : BaseUpcomingTileSettings
    {
        private byte[] _customColor;
        [DataMember]
        public byte[] CustomColor
        {
            get { return _customColor; }
            set { SetProperty(ref _customColor, value, "CustomColor"); }
        }
    }
}
