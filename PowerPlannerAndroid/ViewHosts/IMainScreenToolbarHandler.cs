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
        void OnMenuItemClick(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e);

        void RequestUpdateMenu();
    }
}