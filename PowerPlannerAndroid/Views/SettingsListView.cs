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
using PowerPlannerAndroid.Vx;
using Google.Android.Material.Button;
using PowerPlannerAndroid.Views.Controls;

namespace PowerPlannerAndroid.Views
{
    public class SettingsListView : VxViewHost<SettingsListViewModel>
    {
        private View _listItemUpgradeToPremium;

        public SettingsListView(Context context) : base(context)
        {
            View = VxVerticalScrollView(0, 8, 0, 8,

                // Current semester text
                new TextView(Context, null, 0, Resource.Style.TextAppearance_AppCompat_Medium)
                    .VxText(Binding(nameof(ViewModel.CurrentSemesterText)))
                    .VxVisibility(Binding(nameof(ViewModel.IsViewYearsAndSemestersVisible)))
                    .VxMaxLines(1)
                    .VxPadding(16, 8, 16, 0),

                // Button to view years/semesters
                new MaterialButton(Context, null, Resource.Attribute.borderlessButtonStyle)
                    .VxTextLocalized("String_ViewYearsAndSemesters")
                    .VxLayoutParams().AutoWidth().Gravity(GravityFlags.Start).Margins(8, 0, 8, 8).Apply()
                    .VxVisibility(Binding(nameof(ViewModel.IsViewYearsAndSemestersVisible)))
                    .VxClick(delegate { ViewModel.OpenYears(); }),

                new Divider(context)
                    .VxLayoutParams().Margins(0, 0, 0, 8).Apply()
                    .VxVisibility(Binding(nameof(ViewModel.IsViewYearsAndSemestersVisible))),

                // Sync status and sync now
                new TextView(Context, null, 0, Resource.Style.TextAppearance_AppCompat_Medium)
                    .VxText(Binding(nameof(ViewModel.SyncStatusText)))
                    .VxVisibility(Binding(nameof(ViewModel.IsSyncOptionsVisible)))
                    .VxMaxLines(1)
                    .VxPadding(16, 8, 16, 0),

                new LinearLayout(Context)
                    .VxLayoutParams().Margins(8, 0, 8, 8).Apply()
                    .VxVisibility(Binding(nameof(ViewModel.IsSyncOptionsVisible)))
                    .VxChildren(

                        new MaterialButton(Context, null, Resource.Attribute.borderlessButtonStyle)
                            .VxText("View errors")
                            .VxLayoutParams().AutoWidth().Gravity(GravityFlags.Start).Apply()
                            .VxVisibility(Binding(nameof(ViewModel.SyncHasError)))
                            .VxClick(delegate { ViewModel.ViewSyncErrors(); }),

                        new MaterialButton(Context, null, Resource.Attribute.borderlessButtonStyle)
                            .VxText(Binding(nameof(ViewModel.SyncButtonText)))
                            .VxEnabled(Binding(nameof(ViewModel.SyncButtonIsEnabled)))
                            .VxClick(delegate { ViewModel.StartSync(); })

                    ),

                new Divider(Context)
                    .VxLayoutParams().Margins(0, 0, 0, 8).Apply()
                    .VxVisibility(Binding(nameof(ViewModel.IsSyncOptionsVisible))),

                // SETTINGS ITEMS

                // Upgrade to premium
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_UpgradeToPremiumItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_UpgradeToPremiumItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_shop_black_48dp,
                    Visibility = ViewStates.Gone
                }
                .VxClick(delegate { ViewModel.OpenPremiumVersion(); })
                .VxReference(ref _listItemUpgradeToPremium),

                // Create account
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_CreateAccountItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_CreateAccountItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_account_circle_black_48dp
                }
                .VxClick(delegate { ViewModel.OpenCreateAccount(); })
                .VxVisibility(Binding(nameof(ViewModel.IsCreateAccountVisible))),

                // Log in
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_LogInItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_LogInItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_input_black_48dp
                }
                .VxClick(delegate { ViewModel.OpenLogIn(); })
                .VxVisibility(Binding(nameof(ViewModel.IsLogInVisible))),

                // My account
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_MyAccountItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_account_circle_black_48dp
                }
                .VxClick(delegate { ViewModel.OpenMyAccount(); })
                .VxVisibility(Binding(nameof(ViewModel.IsMyAccountVisible))),

                // Widgets
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("String_Widgets"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_Widgets_Subtitle"),
                    IconResource = Resource.Drawable.ic_dashboard_black_48dp
                }
                .VxClick(delegate { ShowCustomViewModel<WidgetsViewModel>(); })
                .VxVisibility(Binding(nameof(ViewModel.HasAccount))),

                // Reminders
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_RemindersItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_RemindersItem.Subtitle"),
                    IconResource = Resource.Drawable.baseline_notifications_black_48
                }
                .VxClick(delegate { ViewModel.OpenReminderSettings() })
                .VxVisibility(Binding(nameof(ViewModel.IsRemindersVisible))),

                // Sync options
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_SyncOptionsItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_SyncOptionsItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_cached_black_48dp
                }
                .VxClick(delegate { ViewModel.OpenSyncOptionsSimple(); })
                .VxVisibility(Binding(nameof(ViewModel.IsSyncOptionsVisible))),

                // Google Calendar
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_GoogleCalendarIntegrationItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_GoogleCalendarIntegrationItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_gcal_black_48dp
                }
                .VxClick(GoogleCalendar_Click)
                .VxVisibility(Binding(nameof(ViewModel.IsGoogleCalendarIntegrationVisible))),

                // Two week schedule
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_TwoWeekScheduleItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_TwoWeekScheduleItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_date_range_black_48dp
                }
                .VxClick(delegate { ViewModel.OpenTwoWeekScheduleSettings(); })
                .VxVisibility(Binding(nameof(ViewModel.IsTwoWeekScheduleVisible))),

                // School time zone
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_SchoolTimeZoneItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_SchoolTimeZoneItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_access_time_48dp
                }
                .VxClick(delegate { ViewModel.OpenSchoolTimeZone(); })
                .VxVisibility(Binding(nameof(ViewModel.IsSchoolTimeZoneVisible))),

                // Help
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_HelpItem.Title"),
                    SettingSubtitle = PowerPlannerResources.GetString("Settings_MainPage_HelpItem.Subtitle"),
                    IconResource = Resource.Drawable.ic_help_black_48dp
                }
                .VxClick(Help_Click),

                // About
                new SettingsListItem(Context)
                {
                    SettingTitle = PowerPlannerResources.GetString("Settings_MainPage_AboutItem.Title"),
                    SettingSubtitle = "BareBones Dev",
                    IconResource = Resource.Drawable.outline_info_black_48
                }
                .VxClick(delegate { ViewModel.OpenAbout(); })
                );

#if DEBUG
            GC.Collect();
#endif
        }

        private void Help_Click(object sender, EventArgs e)
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