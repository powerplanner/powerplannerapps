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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.App;
using InterfacesDroid.ViewModelPresenters;
using PowerPlannerAppDataLibrary.Extensions;
using BareMvvm.Core.ViewModels;
using PowerPlannerAndroid.ViewModel.Settings;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class SettingsListView : InterfacesDroid.Views.PopupViewHost<SettingsListViewModel>
    {
        private View _listItemUpgradeToPremium;

        public SettingsListView(ViewGroup root) : base(Resource.Layout.SettingsList, root)
        {
            FindViewById<Button>(Resource.Id.ButtonViewYearsAndSemesters).Click += delegate { ViewModel.OpenYears(); };
            FindViewById<Button>(Resource.Id.ButtonSync).Click += delegate { ViewModel.StartSync(); };
            FindViewById<Button>(Resource.Id.ButtonViewSyncErrors).Click += delegate { ViewModel.ViewSyncErrors(); };
            FindViewById<View>(Resource.Id.SettingsListItemCreateAccount).Click += delegate { ViewModel.OpenCreateAccount(); };
            FindViewById<View>(Resource.Id.SettingsListItemLogIn).Click += delegate { ViewModel.OpenLogIn(); };
            FindViewById<View>(Resource.Id.SettingsListItemAccount).Click += delegate { ViewModel.OpenMyAccount(); };
            FindViewById<View>(Resource.Id.SettingsListItemWidgets).Click += delegate { ShowCustomViewModel<WidgetsViewModel>(); };
            FindViewById<View>(Resource.Id.SettingsListItemAbout).Click += delegate { ViewModel.OpenAbout(); };
            FindViewById<View>(Resource.Id.SettingsListItemTwoWeekSchedule).Click += delegate { ViewModel.OpenTwoWeekScheduleSettings(); };
            FindViewById<View>(Resource.Id.SettingsListItemSyncOptions).Click += delegate { ViewModel.OpenSyncOptionsSimple(); };
            FindViewById<View>(Resource.Id.SettingsListItemGoogleCalendar).Click += GoogleCalendar_Click;
            FindViewById<View>(Resource.Id.SettingsListItemTimeZone).Click += delegate { ViewModel.OpenSchoolTimeZone(); };
            FindViewById<View>(Resource.Id.SettingsListItemHelp).Click += SettingsListView_Click;
            _listItemUpgradeToPremium = FindViewById<View>(Resource.Id.SettingsListItemUpgradeToPremium);
            _listItemUpgradeToPremium.Click += delegate { ViewModel.OpenPremiumVersion(); };

#if DEBUG
            GC.Collect();
#endif
        }

        private void SettingsListView_Click(object sender, EventArgs e)
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_OpenHelp");

                var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(SettingsListViewModel.HelpUrl));

                Context.StartActivity(browserIntent);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Toast.MakeText(Context, "Web browser couldn't be launched.", ToastLength.Short).Show();
            }
        }

        private void GoogleCalendar_Click(object sender, EventArgs e)
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_OpenGoogleCalendarIntegration");

                if (ViewModel.AlertIfGoogleCalendarIntegrationNotPossible())
                {
                    return;
                }

                var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(GoogleCalendarIntegrationViewModel.Url));

                // Pass in headers
                // https://stackoverflow.com/questions/28868734/open-browser-with-a-url-with-extra-headers-for-android
                var headersBundle = new Bundle();
                headersBundle.PutString("AccountId", ViewModel.Account.AccountId.ToString());
                headersBundle.PutString("Username", ViewModel.Account.Username);
                headersBundle.PutString("Session", ViewModel.Account.Token);

                browserIntent.PutExtra(Android.Provider.Browser.ExtraHeaders, headersBundle);

                Context.StartActivity(browserIntent);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Toast.MakeText(Context, "Web browser couldn't be launched.", ToastLength.Short).Show();
            }
        }

        private void ShowCustomViewModel<T>() where T : BaseViewModel
        {
            var newViewModel = (T)Activator.CreateInstance(typeof(T), SettingsListViewModel.GetParentForSubviews(ViewModel));

            SettingsListViewModel.Show(newViewModel);
        }

        public override void OnViewModelSetOverride()
        {
            ViewModel.ShowAsPopups = true;

            base.OnViewModelSetOverride();
        }

        public override void OnViewModelLoadedOverride()
        {
            if (!ViewModel.HasAccount)
            {
                FindViewById<View>(Resource.Id.SettingsListItemWidgets).Visibility = ViewStates.Gone;
            }

            UpdateUpgradeToPremiumVisibility();
        }

        private async void UpdateUpgradeToPremiumVisibility()
        {
            try
            {
                if (await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    _listItemUpgradeToPremium.Visibility = ViewStates.Gone;
                }
                else
                {
                    _listItemUpgradeToPremium.Visibility = ViewStates.Visible;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}