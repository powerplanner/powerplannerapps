using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public sealed partial class AddClassTimeGroupExpanded : UserControl
    {
        private AddClassTimesViewModel.ClassTimeGroup ViewModel => DataContext as AddClassTimesViewModel.ClassTimeGroup;

        public AddClassTimeGroupExpanded()
        {
            this.InitializeComponent();

            checkBoxMonday.Content = DateTools.ToLocalizedString(DayOfWeek.Monday);
            checkBoxTuesday.Content = DateTools.ToLocalizedString(DayOfWeek.Tuesday);
            checkBoxWednesday.Content = DateTools.ToLocalizedString(DayOfWeek.Wednesday);
            checkBoxThursday.Content = DateTools.ToLocalizedString(DayOfWeek.Thursday);
            checkBoxFriday.Content = DateTools.ToLocalizedString(DayOfWeek.Friday);
            checkBoxSaturday.Content = DateTools.ToLocalizedString(DayOfWeek.Saturday);
            checkBoxSunday.Content = DateTools.ToLocalizedString(DayOfWeek.Sunday);
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveThisGroup();
        }
    }
}
