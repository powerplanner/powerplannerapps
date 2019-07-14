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
using PowerPlannerAndroid.ViewModel.Settings;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAndroid.Views
{
    public class SettingsWidgetsView : InterfacesDroid.Views.PopupViewHost<WidgetsViewModel>
    {
        public SettingsWidgetsView(ViewGroup root) : base(Resource.Layout.SettingsWidgets, root)
        {
            FindViewById<View>(Resource.Id.SettingsWidgetAgenda).Click += delegate { NavigateToCustomViewModel<WidgetAgendaViewModel>(); };
            FindViewById<View>(Resource.Id.SettingsWidgetSchedule).Click += delegate { NavigateToCustomViewModel<WidgetScheduleViewModel>(); };
        }

        private void NavigateToCustomViewModel<T>() where T : BaseViewModel
        {
            var pagedViewModel = ViewModel.FindAncestor<PagedViewModel>();

            var newViewModel = (T)Activator.CreateInstance(typeof(T), pagedViewModel);

            pagedViewModel.Navigate(newViewModel);
        }
    }
}