using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace ToolsUniversal
{
    public static class VisualTreeExtensions
    {
        public static T FindParent<T>(this DependencyObject obj)
        {
            object parent = VisualTreeHelper.GetParent(obj);

            if (parent == null)
                return default(T);

            if (parent is T)
                return (T)parent;

            return FindParent<T>(parent as DependencyObject);
        }
    }
}
