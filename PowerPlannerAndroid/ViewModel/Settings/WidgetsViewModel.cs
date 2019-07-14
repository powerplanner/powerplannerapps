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
        private PagedViewModel _pagedParent;
        public WidgetsViewModel(PagedViewModel parent) : base(parent)
        {
            _pagedParent = parent;
        }

        public void OpenAgendaWidgetSettings()
        {
            _pagedParent.Navigate(new WidgetAgendaViewModel(_pagedParent));
        }

        public void OpenScheduleWidgetSettings()
        {
            _pagedParent.Navigate(new WidgetScheduleViewModel(_pagedParent));
        }
    }
}