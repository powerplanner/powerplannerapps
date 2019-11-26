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
using InterfacesDroid.Views;
using BareMvvm.Core.ViewModels;
using PowerPlannerAndroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class PopupViewHost<TViewModel> : InterfacesDroid.Views.PopupViewHost<TViewModel> where TViewModel : BaseViewModel
    {
        public Android.Support.V7.Widget.Toolbar Toolbar { get; private set; }

        public PopupViewHost(int resourceId, ViewGroup root) : base(resourceId, root)
        {
        }

        protected override View CreateView(LayoutInflater inflater, int resourceId, ViewGroup root)
        {
            View popupView = inflater.Inflate(Resource.Layout.PopupView, null);
            Toolbar = popupView.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Toolbar);
            Toolbar.MenuItemClick += Toolbar_MenuItemClick;

            Toolbar.SetNavigationIcon(Resource.Drawable.ic_arrow_back_black_24dp);
            Toolbar.NavigationClick += delegate { ViewModel.GoBack(); };

            // Use the base method so that it participates in binding correctly
            View content = base.CreateView(inflater, resourceId, root);

            popupView.FindViewById<ViewGroup>(Resource.Id.ContentFrame).AddView(content);

            return popupView;
        }

        private void Toolbar_MenuItemClick(object sender, Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            OnMenuItemClicked(e);
        }

        public string Title
        {
            get { return Toolbar.Title; }
            set { Toolbar.Title = value; }
        }

        public void SetMenu(int menuResource)
        {
            Toolbar.Menu.Clear();
            MenuInflater inflater = new MenuInflater(Context);
            inflater.Inflate(menuResource, Toolbar.Menu);

            LocalizationHelper.LocalizeMenu(Toolbar.Menu);
        }

        public virtual void OnMenuItemClicked(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            // Nothing
        }
    }
}