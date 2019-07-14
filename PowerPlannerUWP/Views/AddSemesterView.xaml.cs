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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddSemesterView : PopupViewHostGeneric
    {
        public new AddSemesterViewModel ViewModel
        {
            get { return base.ViewModel as AddSemesterViewModel; }
            set { base.ViewModel = value; }
        }

        public AddSemesterView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            // Say that this view supports start/end
            ViewModel.SupportsStartEnd = true;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateStartDate();
            UpdateEndDate();

            switch (ViewModel.State)
            {
                case AddSemesterViewModel.OperationState.Adding:
                    this.Title = LocalizedResources.GetString("EditSemesterPage_Title_Adding");
                    break;

                case AddSemesterViewModel.OperationState.Editing:
                    this.Title = LocalizedResources.GetString("EditSemesterPage_Title_Editing");
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
            if (ViewModel.StartDate == null)
            {
                DatePickerStart.Date = null;
            }
            else
            {
                DatePickerStart.Date = ViewModel.StartDate.Value;
            }
        }

        private void UpdateEndDate()
        {
            if (ViewModel.EndDate == null)
            {
                DatePickerEnd.Date = null;
            }
            else
            {
                DatePickerEnd.Date = ViewModel.EndDate.Value;
            }
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

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ConfirmDelete(LocalizedResources.GetString("MessageDeleteSemester_Body"), LocalizedResources.GetString("MessageDeleteSemester_Title")))
            {
                ViewModel.Delete();
            }
        }

        private void DatePickerEnd_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate == null)
            {
                ViewModel.EndDate = null;
            }
            else
            {
                ViewModel.EndDate = args.NewDate.Value.DateTime;
            }
        }

        private void DatePickerStart_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate == null)
            {
                ViewModel.StartDate = null;
            }
            else
            {
                ViewModel.StartDate = args.NewDate.Value.DateTime;
            }
        }
    }
}
