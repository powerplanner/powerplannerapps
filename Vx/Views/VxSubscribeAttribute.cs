using System;
namespace Vx.Views
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class VxSubscribeAttribute : Attribute
    {

        public VxSubscribeAttribute()
        {

        }
    }
}
