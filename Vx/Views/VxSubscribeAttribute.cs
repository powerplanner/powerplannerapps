using System;
using System.Collections.Generic;
using System.Text;

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
