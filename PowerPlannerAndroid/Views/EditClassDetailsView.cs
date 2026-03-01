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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class EditClassDetailsView : PopupViewHost<EditClassDetailsViewModel>
    {
        public EditClassDetailsView(ViewGroup root) : base(Resource.Layout.EditClassDetails, root)
        {
            Title = PowerPlannerResources.GetString("String_EditDetails").ToUpper();
        }

        protected override void OnViewCreated()
        {
            FindViewById<EditText>(Resource.Id.EditTextDetails).Hint = R.S("ClassPage_PivotItemDetails.Header");

            BindingHost.SetBinding<string>(nameof(ViewModel.Details), details =>
            {
                FindViewById<EditText>(Resource.Id.EditTextDetails).Text = details;
            });

            FindViewById<EditText>(Resource.Id.EditTextDetails).TextChanged += EditClassDetailsView_TextChanged;

            base.OnViewCreated();
        }

        private void EditClassDetailsView_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Details = (sender as EditText).Text;
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            SetMenu(Resource.Menu.edit_class_details_menu);

            KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextDetails));
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel.Save();
                    break;
            }
        }
    }
}