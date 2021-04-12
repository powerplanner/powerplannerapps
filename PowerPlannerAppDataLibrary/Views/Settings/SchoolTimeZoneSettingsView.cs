using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Markup;

namespace PowerPlannerAppDataLibrary.Views.Settings
{
    public class SchoolTimeZoneSettingsView : BaseSettingPageView<SchoolTimeZoneSettingsViewModel>
    {
        protected override string Title => PowerPlannerResources.GetString("Settings_SchoolTimeZone_Header.Text");

        protected override View Content => new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_Description.Text")
                },

                new Picker
                {
                    Title = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ComboBoxTimeZone.Header"),
                    ItemsSource = ViewModel.AvailableTimeZones
                }
                .Bind(Picker.SelectedItemProperty, nameof(ViewModel.SelectedSchoolTimeZone), mode: BindingMode.TwoWay)
                .Bind(Picker.IsEnabledProperty, nameof(ViewModel.IsEnabled)),

                new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_ButtonSave.Content")
                }
                .Bind(Picker.IsEnabledProperty, nameof(ViewModel.IsEnabled)),

                new Label
                {
                    Text = PowerPlannerResources.GetString("Settings_SchoolTimeZone_RestartNote")
                }
            }
        };
    }
}
