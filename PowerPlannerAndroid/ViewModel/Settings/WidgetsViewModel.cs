using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAndroid.ViewModel.Settings
{
    public class WidgetsViewModel : BaseViewModel
    {
        private PagedViewModelWithPopups _popupsHost;
        public WidgetsViewModel(PagedViewModel parent) : base(parent)
        {
            _popupsHost = GetPopupViewModelHost();
        }

        public void OpenAgendaWidgetSettings()
        {
            _popupsHost.ShowPopup(new WidgetAgendaViewModel(_popupsHost));
        }

        public void OpenScheduleWidgetSettings()
        {
            _popupsHost.ShowPopup(new WidgetScheduleViewModel(_popupsHost));
        }
    }
}