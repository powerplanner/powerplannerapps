using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerPlannerAppDataLibrary.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewTaskOrEventView : GenericPopupView
    {
        public new ViewTaskOrEventViewModel ViewModel
        {
            get => base.ViewModel as ViewTaskOrEventViewModel;
            set => base.ViewModel = value;
        }

        public ViewTaskOrEventView()
        {
            InitializeComponent();
        }
    }
}