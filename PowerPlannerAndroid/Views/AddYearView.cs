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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class AddYearView : PopupViewHost<AddYearViewModel>
    {
        public AddYearView(ViewGroup root) : base(Resource.Layout.AddYear, root)
        {
            SetMenu(Resource.Menu.add_year_menu);
        }

        public override void OnViewModelLoadedOverride()
        {
            switch (ViewModel.State)
            {
                case AddYearViewModel.OperationState.Adding:
                    Title = PowerPlannerResources.GetString("AddYearPage_Title_Adding");
                    break;

                case AddYearViewModel.OperationState.Editing:
                    Title = PowerPlannerResources.GetString("AddYearPage_Title_Editing");
                    break;
            }

            if (ViewModel.Name.Length == 0)
            {
                KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextName));
            }
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel.Save();
                    break;

                case Resource.Id.MenuItemDelete:
                    PromptDelete();
                    break;
            }
        }

        private void PromptDelete()
        {
            var builder = new AlertDialog.Builder(Context);

            builder
                .SetTitle(PowerPlannerResources.GetString("MessageDeleteYear_Title"))
                .SetMessage(PowerPlannerResources.GetString("MessageDeleteYear_Body"))
                .SetPositiveButton(PowerPlannerResources.GetMenuItemDelete(), delegate { ViewModel.Delete(); })
                .SetNegativeButton(PowerPlannerResources.GetStringCancel(), delegate { });

            builder.Create().Show();
        }
    }
}