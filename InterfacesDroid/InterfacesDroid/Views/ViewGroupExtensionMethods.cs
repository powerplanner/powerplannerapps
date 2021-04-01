using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Views
{
    public static class ViewGroupExtensionMethods
    {
        public static IEnumerable<View> GetAllChildren(this ViewGroup viewGroup)
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                yield return viewGroup.GetChildAt(i);
            }
        }
    }
}