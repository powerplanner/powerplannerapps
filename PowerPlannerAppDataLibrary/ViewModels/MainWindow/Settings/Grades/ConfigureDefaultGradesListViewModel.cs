using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureDefaultGradesListViewModel : ComponentViewModel
    {
        [VxSubscribe]
        public AccountDataItem Account { get; private set; }

        public ConfigureDefaultGradesListViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_DefaultGradeOptions.Title");
            Account = MainScreenViewModel.CurrentAccount;
        }

        protected override View Render()
        {
            var layout = new LinearLayout
            {
                Margin = new Thickness(0, Theme.Current.PageMargin - 12, 0, Theme.Current.PageMargin - 12),
                Children =
                {
                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("Settings_DefaultGradeOptions_Explanation"),
                        WrapText = true,
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
                }
            };

            if (VxPlatform.Current == Platform.Uwp)
            {
                layout.Children.Insert(0, new TextBlock
                {
                    Text = Title.ToUpper(),
                    WrapText = true,
                    Margin = new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, 0)
                }.TitleStyle());
            }

            return new ScrollView
            {
                Content = layout
            };
        }

        private View RenderOption(string icon, string title, string subtitle, Action action)
        {
            return SettingsListViewModel.RenderOption(icon, title, subtitle, action);
        }

        private void ConfigureAverageGrades()
        {
            ShowViewModel<ConfigureDefaultAverageGradesViewModel>();
        }

        private void ConfigureRoundGradesUp()
        {
            ShowViewModel<ConfigureDefaultRoundGradesUpViewModel>();
        }

        private void ConfigureGradeScale()
        {
            ShowViewModel<ConfigureDefaultGradeScaleViewModel>();
        }

        private void ShowViewModel<T>() where T : BaseViewModel
        {
            if (VxPlatform.Current != Platform.iOS)
            {
                ShowPopup(Activator.CreateInstance(typeof(T), this) as BaseViewModel);
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(Activator.CreateInstance(typeof(T), this) as BaseViewModel);
            }
        }
    }
}
