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
using AndroidX.Core.View;

namespace PowerPlannerAndroid.Views
{
    public class PopupViewHost<TViewModel> : InterfacesDroid.Views.PopupViewHost<TViewModel>, AndroidX.Core.View.IOnApplyWindowInsetsListener where TViewModel : BaseViewModel
    {
        public AndroidX.AppCompat.Widget.Toolbar Toolbar { get; private set; }

        public PopupViewHost(int resourceId, ViewGroup root) : base(resourceId, root)
        {
        }

        public PopupViewHost(ViewGroup root) : base(root)
        {
        }

        protected void AddNonInflatedView(View content)
        {
            View popupView = LayoutInflater.FromContext(Context).Inflate(Resource.Layout.PopupView, null);
            Toolbar = popupView.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.Toolbar);
            Toolbar.MenuItemClick += Toolbar_MenuItemClick;

            Toolbar.SetNavigationIcon(Resource.Drawable.ic_arrow_back_black_24dp);
            Toolbar.NavigationClick += delegate { ViewModel.GoBack(); };

            popupView.FindViewById<ViewGroup>(Resource.Id.ContentFrame).AddView(content);

            InitializePopupView(popupView);

            AddView(popupView);
        }

        protected override View CreateView(LayoutInflater inflater, int resourceId, ViewGroup root)
        {
            View popupView = inflater.Inflate(Resource.Layout.PopupView, null);
            Toolbar = popupView.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.Toolbar);
            Toolbar.MenuItemClick += Toolbar_MenuItemClick;

            Toolbar.SetNavigationIcon(Resource.Drawable.ic_arrow_back_black_24dp);
            Toolbar.NavigationClick += delegate { ViewModel.GoBack(); };

            // Use the base method so that it participates in binding correctly
            View content = base.CreateView(inflater, resourceId, root);

            popupView.FindViewById<ViewGroup>(Resource.Id.ContentFrame).AddView(content);

            InitializePopupView(popupView);

            return popupView;
        }

        private void InitializePopupView(View popupView)
        {
            if (MainScreenView.WindowInsets != null)
            {
                OnApplyWindowInsets(popupView, MainScreenView.WindowInsets);
            }

            ViewCompat.SetOnApplyWindowInsetsListener(popupView, this);
        }

        public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat windowInsets)
        {
            var insets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());

            var statusBarSpacer = v.FindViewById(Resource.Id.StatusBarSpacer);
            var statusBarSpacerLp = statusBarSpacer.LayoutParameters;
            statusBarSpacerLp.Height = insets.Top;
            statusBarSpacer.LayoutParameters = statusBarSpacerLp;

            // Return CONSUMED if you don't want want the window insets to keep being
            // passed down to descendant views.
            return WindowInsetsCompat.Consumed;
        }

        private void Toolbar_MenuItemClick(object sender, AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
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

        public virtual void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            // Nothing
        }
    }
}