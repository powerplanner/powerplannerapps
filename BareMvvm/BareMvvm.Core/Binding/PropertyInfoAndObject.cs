using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BareMvvm.Core.Binding
{
    public class PropertyInfoAndObject
    {
        public PropertyInfo PropertyInfo { get; private set; }

        public object Object { get; private set; }

        internal PropertyInfoAndObject(object o, PropertyInfo propertyInfo)
        {
            Object = o;
            PropertyInfo = propertyInfo;
        }
    }
}
