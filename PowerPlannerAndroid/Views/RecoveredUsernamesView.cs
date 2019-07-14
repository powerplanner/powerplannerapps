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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class RecoveredUsernamesView : PopupViewHost<RecoveredUsernamesViewModel>
    {
        private ItemsControlWrapper _itemsWrapperRecoveredUsernames;

        public RecoveredUsernamesView(ViewGroup root) : base(Resource.Layout.RecoveredUsernames, root)
        {
            Title = PowerPlannerResources.GetString("ForgotUsername_String_MessageHeader");
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            _itemsWrapperRecoveredUsernames = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupRecoveredUsernames))
            {
                ItemsSource = ViewModel.Usernames,
                ItemTemplate = new CustomDataTemplate<string>(CreateUsernameView)
            };
        }

        private View CreateUsernameView(ViewGroup root, string username)
        {
            var view = new TextView(root.Context)
            {
                Text = username,
                TextSize = 18
            };
            view.SetTextIsSelectable(true);
            view.SetPadding(0, ThemeHelper.AsPx(root.Context, 4), 0, ThemeHelper.AsPx(root.Context, 4));
            return view;
        }
    }
}