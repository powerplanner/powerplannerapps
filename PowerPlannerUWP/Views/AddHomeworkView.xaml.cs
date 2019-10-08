using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddHomeworkView : PopupViewHostGeneric
    {
        public new AddHomeworkViewModel ViewModel
        {
            get { return base.ViewModel as AddHomeworkViewModel; }
            set { base.ViewModel = value; }
        }

        public AddHomeworkView()
        {
            this.InitializeComponent();

            TextBlockImageAttachments.Text = PowerPlannerResources.GetString("String_ImageAttachments");
        }

        private void updateHeaderText()
        {
            this.Title = getHeaderText();
        }

        private string getHeaderText()
        {
            switch (ViewModel.Type)
            {
                case PowerPlannerAppDataLibrary.ViewItems.TaskOrEventType.Event:
                    if (isEditing())
                        return LocalizedResources.GetString("String_EditEvent").ToUpper();
                    else
                        return LocalizedResources.GetString("String_AddEvent").ToUpper();

                default:
                    if (isEditing())
                        return LocalizedResources.GetString("String_EditTask").ToUpper();
                    else
                        return LocalizedResources.GetString("String_AddTask").ToUpper();
            }
        }

        private string GetStartTimeText()
        {
            switch (ViewModel.Type)
            {
                case PowerPlannerAppDataLibrary.ViewItems.TaskOrEventType.Event:
                    return LocalizedResources.GetString("String_StartTime");

                default:
                    return LocalizedResources.GetString("String_DueTime");
            }
        }

        private bool isEditing()
        {
            return ViewModel.State == AddHomeworkViewModel.OperationState.Editing;
        }

        public override void OnViewModelSetOverride()
        {
            TimePickerStartTime.Header = GetStartTimeText();
            TimePickerEndTime.Header = LocalizedResources.GetString("String_EndTime");
            ComboBoxTimeOptions.Header = LocalizedResources.GetString("String_Time");

            ViewModel.PropertyChanged += new WeakEventHandler<System.ComponentModel.PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Repeats):
                    // Ensure the recurrence control is loaded
                    FindName(nameof(RecurrenceControlContainer));
                    break;
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            updateHeaderText();

            datePickerDate.Date = ViewModel.Date;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private async void Save()
        {
            ViewModel.Save();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            cancel();
        }

        private void cancel()
        {
            ViewModel.RemoveViewModel();
        }

        private bool _needsFocus = true;
        private void tbName_Loaded(object sender, RoutedEventArgs e)
        {
            if (_needsFocus == false)
                return;

            _needsFocus = false;

            if (isEditing())
                return;

            tbName.Focus(FocusState.Programmatic);
        }

        private void datePickerDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (datePickerDate.Date != null)
            {
                ViewModel.Date = datePickerDate.Date.Value.Date;
            }
        }

        private void tbName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Save();
            }
        }

        private void ButtonUpgradeToPremiumForRepeating_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpgradeToPremiumForRepeating();
        }

        private async void EditImages_RequestedAddImage(object sender, EventArgs e)
        {
            await ViewModel.AddNewImageAttachmentAsync();
        }
    }
}
