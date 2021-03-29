using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vx.Views
{
    public class VxView
    {
        internal Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        protected void SetProperty(object value, [CallerMemberName]string propertyName = null)
        {
            if (value is VxView[] views)
            {
                Properties[propertyName] = views.Where(i => i != null).ToArray();
                return;
            }

            Properties[propertyName] = value;
        }

        protected T GetProperty<T>([CallerMemberName]string propertyName = null)
        {
            return (T)Properties[propertyName];
        }

        internal VxNativeView NativeView { get; set; }
    }
}
