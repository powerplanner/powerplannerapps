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
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Helpers
{
    public static class LocalizationHelper
    {
        public static void LocalizeMenu(IMenu menu)
        {
            for (int i = 0; i < menu.Size(); i++)
            {
                var item = menu.GetItem(i);

                if (item.TitleFormatted != null)
                {
                    string title = item.TitleFormatted.ToString();

                    if (title.StartsWith("{") && title.EndsWith("}"))
                    {
                        item.SetTitle(PowerPlannerResources.GetString(item.TitleFormatted.ToString().Substring(1, title.Length - 2)));
                    }
                }
            }
        }
    }
}