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

            base.OnViewCreated();
        }

        public override void OnViewModelLoadedOverride()
        {
            SetMenu(Resource.Menu.edit_class_details_menu);

            var editTextDetails = FindViewById<EditText>(Resource.Id.EditTextDetails);
            editTextDetails.Text = ViewModel.Details;
            editTextDetails.TextChanged += EditClassDetailsView_TextChanged;

            KeyboardHelper.FocusAndShow(editTextDetails);
        }

        private void EditClassDetailsView_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Details = e.Text.ToString();
            }
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