namespace Vx
{
    public class VxDeviceType
    {
        public static DeviceType Current { get; set; }
    }

    public enum DeviceType
    {
        Phone,
        Tablet,
        Desktop
    }
}
