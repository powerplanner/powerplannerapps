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

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            base.Title = ViewModel.ClassName.ToUpper();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete();
        }
    }
}
