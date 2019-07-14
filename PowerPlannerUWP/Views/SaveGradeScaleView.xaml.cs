using PowerPlannerSending;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SaveGradeScaleView : PopupViewHostGeneric
    {
        public new SaveGradeScaleViewModel ViewModel
        {
            get { return base.ViewModel as SaveGradeScaleViewModel; }
            set { base.ViewModel = value; }
        }

        public SaveGradeScaleView()
        {
            this.InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void tbName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Save();
            }
        }

        private void Save()
        {
            ViewModel.Save();
        }

        private void tbName_Loaded(object sender, RoutedEventArgs e)
        {
            tbName.Focus(FocusState.Programmatic);
        }
    }
}
