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
using PowerPlannerAndroid.ViewModel.Settings;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAndroid.Helpers;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using AndroidX.AppCompat.Widget;

namespace PowerPlannerAndroid.Views
{
    public class SettingsWidgetAgendaView : PopupViewHost<WidgetAgendaViewModel>
    {
        private SwitchCompat _switchTasks;
        private SwitchCompat _switchEvents;
        private EditText _editShowDays;

        public SettingsWidgetAgendaView(ViewGroup root) : base(Resource.Layout.SettingsWidgetAgenda, root)
        {
            Title = PowerPlannerResources.GetString("String_AgendaWidget");

            FindViewById<TextView>(Resource.Id.SettingsWidgetAgendaSkipItemsExplanation).Text = PowerPlannerResources.GetString("Tile_SkipItemsExplanation.Text").Replace("live tiles", "widget");
        }

        public override void OnViewModelLoadedOverride()
        {
            if (ViewModel.Account == null)
            {
                return;
            }

            _switchTasks = FindViewById<SwitchCompat>(Resource.Id.SettingsWidgetAgendaShowTasksSwitch);
            _switchTasks.Checked = ViewModel.Account.MainTileSettings.ShowHomework;
            _switchTasks.CheckedChange += new WeakEventHandler<CompoundButton.CheckedChangeEventArgs>(Switch_CheckedChange).Handler;

            _switchEvents = FindViewById<SwitchCompat>(Resource.Id.SettingsWidgetAgendaShowEventsSwitch);
            _switchEvents.Checked = ViewModel.Account.MainTileSettings.ShowExams;
            _switchTasks.CheckedChange += new WeakEventHandler<CompoundButton.CheckedChangeEventArgs>(Switch_CheckedChange).Handler;

            _editShowDays = FindViewById<EditText>(Resource.Id.SettingsWidgetAgendaSkipDaysEditText);
            _editShowDays.TextChanged += new WeakEventHandler<Android.Text.TextChangedEventArgs>(_editShowDays_TextChanged).Handler;

            base.OnViewModelLoadedOverride();
        }

        private void _editShowDays_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            UpdateSettings();
        }

        private void Switch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            UpdateSettings();
        }

        private async void UpdateSettings()
        {
            try
            {
                if (ViewModel.Account == null)
                {
                    return;
                }

                ViewModel.Account.MainTileSettings.ShowHomework = _switchTasks.Checked;
                ViewModel.Account.MainTileSettings.ShowExams = _switchEvents.Checked;
                ViewModel.Account.MainTileSettings.SkipItemsOlderThan = GetSkipNumber();

                await AccountsManager.Save(ViewModel.Account);

                WidgetsHelper.UpdateAgendaWidget();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private int GetSkipNumber()
        {
            int skipNumber = 0;

            if (string.IsNullOrWhiteSpace(_editShowDays.Text))
            {
                return int.MinValue;
            }

            else if (int.TryParse(_editShowDays.Text, out skipNumber))
            {
                return skipNumber;
            }

            // Invalid entry
            else
            {
                return int.MinValue;
            }
        }
    }
}