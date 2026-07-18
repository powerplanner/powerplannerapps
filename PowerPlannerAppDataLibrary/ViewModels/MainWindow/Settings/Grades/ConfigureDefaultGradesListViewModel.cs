using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureDefaultGradesListViewModel : PopupComponentViewModel
    {
        public AccountDataItem Account { get; private set; }

        public ConfigureDefaultGradesListViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_DefaultGradeOptions.Title");
            Account = MainScreenViewModel.CurrentAccount;
        }

        protected override void RegisterPropertySubscriptions()
        {
            base.RegisterPropertySubscriptions();
            Subscribe(Account);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(new Thickness(0, Theme.Current.PageMargin - 12, 0, Theme.Current.PageMargin - 12),
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_DefaultGradeOptions_Explanation"),
                    TextColor = Theme.Current.SubtleForegroundColor,
                    Margin = new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, 12)
                },

                RenderOption(
                    MaterialDesign.MaterialDesignIcons.Calculate,
                    PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Title"),
                    PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Subtitle"),
                    ConfigureGradeScale),

                RenderOption(
                    MaterialDesign.MaterialDesignIcons.Refresh,
                    PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text"),
                    BoolToEnabledStringConverter.Convert(Account.DefaultDoesAverageGradeTotals),
                    ConfigureAverageGrades),

                RenderOption(
                    MaterialDesign.MaterialDesignIcons.ArrowUpward,
                    PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text"),
                    BoolToEnabledStringConverter.Convert(Account.DefaultDoesRoundGradesUp),
                    ConfigureRoundGradesUp)
            );
        }

        private View RenderOption(string icon, string title, string subtitle, Action action)
        {
            return SettingsListViewModel.RenderOption(icon, title, subtitle, action);
        }

        private void ConfigureAverageGrades()
        {
            ShowViewModel(parent => new ConfigureDefaultAverageGradesViewModel(parent));
        }

        private void ConfigureRoundGradesUp()
        {
            ShowViewModel(parent => new ConfigureDefaultRoundGradesUpViewModel(parent));
        }

        private void ConfigureGradeScale()
        {
            ShowViewModel(parent => new ConfigureDefaultGradeScaleViewModel(parent));
        }

        private void ShowViewModel(Func<BaseViewModel, BaseViewModel> createViewModel)
        {
            ShowPopup(createViewModel(this));
        }
    }
}
