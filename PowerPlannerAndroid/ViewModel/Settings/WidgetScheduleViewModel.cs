using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

namespace PowerPlannerAndroid.ViewModel.Settings
{
    public class WidgetScheduleViewModel : BaseSettingsViewModelWithAccount
    {
        public WidgetScheduleViewModel(BaseViewModel parent) : base(parent)
        {
            
        }
    }
}