using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddHolidayView : PopupViewHostGeneric
    {
        public new AddHolidayViewModel ViewModel
        {
            get { return base.ViewModel as AddHolidayViewModel; }
            set { base.ViewModel = value; }
        }

        public AddHolidayView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateStartDate();
            UpdateEndDate();

            switch (ViewModel.State)
            {
                case AddHolidayViewModel.OperationState.Adding:
                    this.Title = LocalizedResources.GetString("String_AddHoliday").ToUpper();
                    break;

                case AddHolidayViewModel.OperationState.Editing:
                    this.Title = LocalizedResources.GetString("String_EditHoliday").ToUpper();
                    break;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.StartDate):
                    UpdateStartDate();
                    break;

                case nameof(ViewModel.EndDate):
                    UpdateEndDate();
                    break;
            }
        }

        private void UpdateStartDate()
        {
            DatePickerStart.Date = ViewModel.StartDate;
        }

        private void UpdateEndDate()
        {
            DatePickerEnd.Date = ViewModel.EndDate;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            ViewModel.Save();
        }

        private void tbName_Loaded(object sender, RoutedEventArgs e)
        {
            tbName.SelectAll();
            tbName.Focus(FocusState.Programmatic);
        }

        private void tbName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                this.Save();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            PopupMenuConfirmDelete.Show(ButtonDelete, ViewModel.Delete);
        }

        private void DatePickerEnd_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != null)
            {
                ViewModel.EndDate = args.NewDate.Value.DateTime;
            }
        }

        private void DatePickerStart_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != null)
            {
                ViewModel.StartDate = args.NewDate.Value.DateTime;
            }
        }
    }
}
