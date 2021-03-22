using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vx.Views
{
    public class VxView
    {
        internal Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        protected void SetProperty(object value, [CallerMemberName]string propertyName = null)
        {
            Properties[propertyName] = value;
        }

        protected T GetProperty<T>([CallerMemberName]string propertyName = null)
        {
            return (T)Properties[propertyName];
        }

        internal VxNativeView NativeView { get; set; }
    }
}
