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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary;
using PowerPlannerAndroid.Vx;
using InterfacesDroid.Themes;
using PowerPlannerAndroid.Views.Controls;
using Google.Android.Material.Button;

namespace PowerPlannerAndroid.Views
{
    public class SchoolTimeZoneSettingsView : VxPopupViewHost<SchoolTimeZoneSettingsViewModel>
    {
        public SchoolTimeZoneSettingsView(Context context) : base(context)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_SchoolTimeZoneItem.Title");
        }

        private bool _includesBlank = false;
        public override void OnViewModelLoadedOverride()
        {
            var items = ViewModel.AvailableTimeZones.Select(i => SchoolTimeZoneSettingsViewModel.Format(i)).ToList();
            if (ViewModel.SelectedSchoolTimeZone == null)
            {
                items.Insert(0, ""); // Blank option
                _includesBlank = true;
            }

            View = VxVerticalScrollView(
                new TextView(Context, null, 0, Resource.Style.TextAppearance_AppCompat_Medium)
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_Description.Text")
                }
                .VxPadding(16),

                new Divider(Context),

                new FullWidthSpinner(Context)
                {
                    Adapter = ObservableAdapter.Create(
                        list: items,
                        itemResourceId: Resource.Layout.SpinnerItemSelectedSchoolTimeZonePreview,
                        dropDownItemResourceId: Resource.Layout.SpinnerItemTimeZone)
                }
                .VxSelection(ViewModel.SelectedSchoolTimeZone == null ? 0 : ViewModel.AvailableTimeZones.IndexOf(ViewModel.SelectedSchoolTimeZone))
                .VxItemSelected(SpinnerSelectedSchoolTimeZone_ItemSelected),

                new Divider(Context),

                new MaterialButton(Context)
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ButtonSave.Content")
                }
                .VxLayoutParams().Margins(16, 16, 16, 0).Apply()
                .VxClick(delegate { ViewModel.Save(); }),

                new TextView(Context, null, 0, Resource.Style.TextAppearance_AppCompat_Small)
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote.Text")
                }
                .VxPadding(16, 4, 16, 16));
        }

        private void SpinnerSelectedSchoolTimeZone_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (_includesBlank && e.Position == 0)
            {
                ViewModel.SelectedSchoolTimeZone = null;
            }
            else
            {
                ViewModel.SelectedSchoolTimeZone = ViewModel.AvailableTimeZones[_includesBlank ? e.Position - 1 : e.Position];
            }
        }
    }
}