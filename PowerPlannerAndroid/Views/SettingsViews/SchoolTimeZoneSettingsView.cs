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

namespace PowerPlannerAndroid.Views
{
    public class SchoolTimeZoneSettingsView : PopupViewHost<SchoolTimeZoneSettingsViewModel>
    {
        public SchoolTimeZoneSettingsView(ViewGroup root) : base(Resource.Layout.SettingsSchoolTimeZoneSettings, root)
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
            var spinnerSelectedSchoolTimeZone = FindViewById<Spinner>(Resource.Id.SpinnerSelectedSchoolTimeZone);
            spinnerSelectedSchoolTimeZone.Adapter = ObservableAdapter.Create(
                items,
                itemResourceId: Resource.Layout.SpinnerItemSelectedSchoolTimeZonePreview,
                dropDownItemResourceId: Resource.Layout.SpinnerItemTimeZone);
            if (ViewModel.SelectedSchoolTimeZone != null)
            {
                spinnerSelectedSchoolTimeZone.SetSelection(ViewModel.AvailableTimeZones.IndexOf(ViewModel.SelectedSchoolTimeZone));
            }
            spinnerSelectedSchoolTimeZone.ItemSelected += SpinnerSelectedSchoolTimeZone_ItemSelected;

            FindViewById<Button>(Resource.Id.ButtonSaveChanges).Click += delegate { ViewModel.Save(); };
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