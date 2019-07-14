using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerUWP.Views;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuickAddView : PopupViewHostGeneric
    {
        public new QuickAddViewModel ViewModel
        {
            get { return base.ViewModel as QuickAddViewModel; }
            set { base.ViewModel = value; }
        }

        public QuickAddView()
        {
            this.InitializeComponent();
        }

        private void ButtonAddHomework_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddHomework();
        }

        private void ButtonAddExam_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddExam();
        }
    }
}
