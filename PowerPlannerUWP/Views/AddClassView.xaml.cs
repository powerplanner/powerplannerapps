using InterfacesUWP;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsUniversal;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddClassView : PopupViewHostGeneric
    {
        public new AddClassViewModel ViewModel
        {
            get { return base.ViewModel as AddClassViewModel; }
            set { base.ViewModel = value; }
        }

        private AppBarButton _appBarButtonDelete;
        public AppBarButton AppBarButtonDelete
        {
            get
            {
                if (_appBarButtonDelete == null)
                {
                    _appBarButtonDelete = new AppBarButton()
                    {
                        Icon = new SymbolIcon(Symbol.Delete),
                        Label = LocalizedResources.GetString("String_DeleteClass")
                    };

                    _appBarButtonDelete.Click += _appBarButtonDelete_Click;
                }

                return _appBarButtonDelete;
            }
        }

        private async void _appBarButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.State == AddClassViewModel.OperationState.Editing)
            {
                if (await App.ConfirmDelete(LocalizedResources.GetString("String_ConfirmDeleteClassMessage"), LocalizedResources.GetString("String_ConfirmDeleteClassHeader")))
                {
                    ViewModel.Delete();
                    return;
                }

                return;
            }

            ViewModel.GoBack();
        }

        public AddClassView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateStartDate();
            UpdateEndDate();

            switch (ViewModel.State)
            {
                case AddClassViewModel.OperationState.Adding:
                    this.Title = LocalizedResources.GetString("AddClassPage_AddTitle");
                    if (_appBarButtonDelete != null)
                        this.PrimaryCommands.Remove(_appBarButtonDelete);
                    break;

                case AddClassViewModel.OperationState.Editing:
                    this.Title = LocalizedResources.GetString("AddClassPage_EditTitle");
                    if (!this.PrimaryCommands.Contains(AppBarButtonDelete))
                        this.PrimaryCommands.Add(AppBarButtonDelete);
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

        private void Save()
        {
            ViewModel.Save();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void tbName_Loaded(object sender, RoutedEventArgs e)
        {
            if (tbName.Text.Length == 0)
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
