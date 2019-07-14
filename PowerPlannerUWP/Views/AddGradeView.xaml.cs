using PowerPlannerSending;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using InterfacesUWP;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddGradeView : PopupViewHostGeneric
    {
        public new AddGradeViewModel ViewModel
        {
            get { return base.ViewModel as AddGradeViewModel; }
            set { base.ViewModel = value; }
        }

        private FrameworkElementExtender _textBoxNameEx;
        private FrameworkElementExtender _textBoxGradeReceived;

        public AddGradeView()
        {
            this.InitializeComponent();

            _textBoxNameEx = new FrameworkElementExtender(tbName);
            _textBoxGradeReceived = new FrameworkElementExtender(tbGradeReceived);
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            switch (ViewModel.State)
            {
                case AddGradeViewModel.OperationState.Adding:
                    this.Title = LocalizedResources.GetString("EditGradePage_HeaderAddString");
                    break;

                case AddGradeViewModel.OperationState.AddingWhatIf:
                    this.Title = LocalizedResources.GetString("EditGradePage_HeaderAddWhatIfString");
                    break;

                case AddGradeViewModel.OperationState.Editing:
                    this.Title = LocalizedResources.GetString("EditGradePage_HeaderEditString");
                    break;

                case AddGradeViewModel.OperationState.EditingWhatIf:
                    this.Title = LocalizedResources.GetString("EditGradePage_HeaderEditWhatIfString");
                    break;
            }

            datePickerDate.Date = ViewModel.Date;

            if (ViewModel.State == AddGradeViewModel.OperationState.Adding
                || ViewModel.State == AddGradeViewModel.OperationState.AddingWhatIf
                || ViewModel.IsUnassignedItem)
            {
                if (string.IsNullOrWhiteSpace(ViewModel.Name))
                {
                    _textBoxNameEx.OnLoaded(delegate
                    {
                        tbName.Focus(FocusState.Programmatic);
                    });
                }
                else
                {
                    _textBoxGradeReceived.OnLoaded(delegate
                    {
                        tbGradeReceived.Focus(FocusState.Programmatic);
                    });
                }
            }
        }

        private void tbName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbGradeReceived.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void Save()
        {
            ViewModel.Date = datePickerDate.Date.GetValueOrDefault(DateTime.Today).DateTime;
            ViewModel.Save();
        }

        private void tbGradeReceived_GotFocus(object sender, RoutedEventArgs e)
        {
            tbGradeReceived.SelectAll();
        }

        private void tbGradeTotal_GotFocus(object sender, RoutedEventArgs e)
        {
            tbGradeTotal.SelectAll();
        }

        private void tbGradeReceived_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                tbGradeTotal.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }
        
        private void GridDateAndClasses_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 460)
            {
                DisplayDateAndWeightOnMultipleRows();
            }
            else
            {
                DisplayDateAndWeightOnSingleRow();
            }
        }

        private bool _isSingleRow = true;
        private void DisplayDateAndWeightOnSingleRow()
        {
            if (_isSingleRow)
            {
                return;
            }

            _isSingleRow = true;

            Grid.SetColumnSpan(datePickerDate, 1);
            datePickerDate.Margin = new Thickness(0, 0, 6, 0);

            Grid.SetRow(comboBoxWeightCategories, 0);
            Grid.SetColumnSpan(comboBoxWeightCategories, 1);
            Grid.SetColumn(comboBoxWeightCategories, 1);
            comboBoxWeightCategories.Margin = new Thickness(6, 0, 0, 0);
        }

        private void DisplayDateAndWeightOnMultipleRows()
        {
            if (!_isSingleRow)
            {
                return;
            }

            _isSingleRow = false;

            Grid.SetColumnSpan(datePickerDate, 2);
            datePickerDate.Margin = new Thickness(0);

            Grid.SetRow(comboBoxWeightCategories, 1);
            Grid.SetColumnSpan(comboBoxWeightCategories, 2);
            Grid.SetColumn(comboBoxWeightCategories, 0);
            comboBoxWeightCategories.Margin = new Thickness(0, 12, 0, 0);
        }

        private void tbGradeTotal_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Save();
            }
        }
    }
}
