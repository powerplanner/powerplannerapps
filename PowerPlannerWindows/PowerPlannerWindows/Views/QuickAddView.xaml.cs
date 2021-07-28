using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerUWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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

        private void ButtonAddTask_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddTask();
        }

        private void ButtonAddEvent_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddEvent();
        }
    }
}
