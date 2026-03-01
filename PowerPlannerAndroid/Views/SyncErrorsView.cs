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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;
using PowerPlannerAppDataLibrary.DataLayer;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class SyncErrorsView : PopupViewHost<SyncErrorsViewModel>
    {
        private ItemsControlWrapper _itemsWrapperErrors;

        public SyncErrorsView(ViewGroup root) : base(Resource.Layout.SyncErrors, root)
        {
            Title = "SYNC ERRORS";

            FindViewById<TextView>(Resource.Id.SyncErrors_Description).Text = R.S("SyncErrors_Description");
        }

        public override void OnViewModelLoadedOverride()
        {
            _itemsWrapperErrors = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.ViewGroupSyncErrors))
            {
                ItemsSource = ViewModel.SyncErrors,
                ItemTemplate = new CustomDataTemplate<LoggedError>(CreateLoggedErrorView)
            };
        }

        private View CreateLoggedErrorView(ViewGroup parent, LoggedError error)
        {
            TextView tv = new TextView(parent.Context)
            {
                Text = error.Name + "\n" + error.Date + "\n" + error.Message
            };
            tv.SetTextIsSelectable(true);
            var padding = ThemeHelper.AsPx(parent.Context, 4);
            tv.SetPadding(0, padding, 0, padding);
            return tv;
        }
    }
}