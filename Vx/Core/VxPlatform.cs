using System;
namespace Vx
{
    public static class VxPlatform
    {
        public static Platform Current { get; set; }
    }

    public enum Platform
    {
        Uwp,
        Android,
        iOS
    }
}
