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
using BareMvvm.Core.ViewModels;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAndroid.Views;

namespace PowerPlannerAndroid.ViewHosts
{
    public class MainScreenViewHostDescendant<TViewModel> : InterfacesDroid.Views.PopupViewHost<TViewModel>, IMainScreenToolbarHandler where TViewModel : BaseViewModel
    {
        public MainScreenView MainScreenView { get; private set; }

        public MainScreenViewHostDescendant(int resourceId, ViewGroup root) : base(resourceId, root)
        {
            View v = root;
            while (true)
            {
                if (v == null)
                {
                    throw new InvalidOperationException("Couldn't find MainScreenView from parent");
                }

                if (v is MainScreenView)
                {
                    MainScreenView = v as MainScreenView;
                    break;
                }

                v = v.Parent as View;
            }
        }

        protected virtual int GetMenuResource()
        {
            return 0;
        }

        public void RequestUpdateMenu()
        {
            MainScreenView.Toolbar.Menu.Clear();

            int menuResource = GetMenuResource();

            if (menuResource == 0)
            {
                return;
            }

            MenuInflater inflater = new MenuInflater(Context);
            inflater.Inflate(menuResource, MainScreenView.Toolbar.Menu);

            LocalizationHelper.LocalizeMenu(MainScreenView.Toolbar.Menu);
        }

        public virtual void OnMenuItemClick(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            // Nothing
        }
    }
}