using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class WidgetSettingsViewModel : PopupComponentViewModel
    {
        public WidgetSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("String_Widgets");
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = NookInsets.Combine(new Thickness(0, Theme.Current.PageMargin / 2, 0, Theme.Current.PageMargin / 2)),
                    Children =
                    {
                        SettingsListViewModel.RenderOption(
                            MaterialDesign.MaterialDesignIcons.Assignment,
                            R.S("String_AgendaWidget"),
                            R.S("Settings_Widgets_Agenda_Subtitle"),
                            OpenAgendaSettings),

                        SettingsListViewModel.RenderOption(
                            MaterialDesign.MaterialDesignIcons.DateRange,
                            R.S("String_ScheduleWidget"),
                            R.S("Settings_Widgets_Schedule_Subtitle"),
                            OpenScheduleSettings)
                    }
                }
            };
        }

        private void OpenAgendaSettings()
        {
            ShowPopup(new WidgetAgendaSettingsViewModel(Parent));
        }

        private void OpenScheduleSettings()
        {
            ShowPopup(new WidgetScheduleSettingsViewModel(Parent));
        }
    }
}
