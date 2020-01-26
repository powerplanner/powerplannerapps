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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerUWP.Controls;
using PowerPlannerUWP.Controls.TimePickers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddClassTimeView : PopupViewHostGeneric
    {

        public new AddClassTimeViewModel ViewModel
        {
            get { return base.ViewModel as AddClassTimeViewModel; }
            set { base.ViewModel = value; }
        }

        public AddClassTimeView()
        {
            this.InitializeComponent();

            MaxWindowSize = new Size(450, double.MaxValue);

            checkBoxMonday.Content = DateTools.ToLocalizedString(DayOfWeek.Monday);
            checkBoxTuesday.Content = DateTools.ToLocalizedString(DayOfWeek.Tuesday);
            checkBoxWednesday.Content = DateTools.ToLocalizedString(DayOfWeek.Wednesday);
            checkBoxThursday.Content = DateTools.ToLocalizedString(DayOfWeek.Thursday);
            checkBoxFriday.Content = DateTools.ToLocalizedString(DayOfWeek.Friday);
            checkBoxSaturday.Content = DateTools.ToLocalizedString(DayOfWeek.Saturday);
            checkBoxSunday.Content = DateTools.ToLocalizedString(DayOfWeek.Sunday);
        }

        public override void OnViewModelSetOverride()
        {
            ViewModel.AutoAdjustEndTimes = false;

            base.OnViewModelSetOverride();
        }

        private DateTime? _startedAddingTime;
        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            base.Title = ViewModel.ClassName.ToUpper();

            // For tracking effectiveness of new time picker
            if (ViewModel.State == AddClassTimeViewModel.OperationState.Adding && TextBasedTimePicker.IsSupported)
            {
                _startedAddingTime = DateTime.UtcNow;
            }
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Clicking button doesn't take focus away from TextBasedTimePicker, so their edited value doesn't get committed...
            // Therefore we have to take focus away, and then wait for the next UI cycle to commit changes
            ButtonSave.Focus(FocusState.Programmatic);
            await System.Threading.Tasks.Task.Delay(1);

            ViewModel.Save(_startedAddingTime);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete();
        }
    }
}
