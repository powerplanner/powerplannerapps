using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using PowerPlannerAndroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public class VxPopupViewHost<TViewModel> : VxViewHost<TViewModel> where TViewModel : BaseViewModel
    {
        public AndroidX.AppCompat.Widget.Toolbar Toolbar { get; private set; }
        private ViewGroup _contentFrame;

        public VxPopupViewHost(Context context) : base(context)
        {
            View popupView = LayoutInflater.FromContext(context).Inflate(Resource.Layout.PopupView, null);
            Toolbar = popupView.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.Toolbar);
            Toolbar.MenuItemClick += Toolbar_MenuItemClick;

            Toolbar.SetNavigationIcon(Resource.Drawable.ic_arrow_back_black_24dp);
            Toolbar.NavigationClick += delegate { ViewModel.GoBack(); };

            _contentFrame = popupView.FindViewById<ViewGroup>(Resource.Id.ContentFrame);

            base.View = popupView;
        }

        public override View View
        {
            get => _contentFrame.ChildCount == 0 ? null : _contentFrame.GetChildAt(0);

            set
            {
                _contentFrame.RemoveAllViews();
                if (value != null)
                {
                    _contentFrame.AddView(value);
                }
            }
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