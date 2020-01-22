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

namespace PowerPlannerAndroid.ViewHosts
{
    public interface IMainScreenToolbarHandler
    {
        void OnMenuItemClick(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e);

        void RequestUpdateMenu();
    }
}