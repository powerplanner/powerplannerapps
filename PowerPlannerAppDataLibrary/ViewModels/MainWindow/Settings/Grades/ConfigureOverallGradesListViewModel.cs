using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureOverallGradesListViewModel : ComponentViewModel
    {
        public ConfigureOverallGradesListViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Grade options";
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
                        Text = "Configure the default grade options, applied to all classes unless overridden.",
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
                        BoolToEnabledStringConverter.Convert(false),
                        ConfigureAverageGrades),

                    RenderOption(
                        MaterialDesign.MaterialDesignIcons.ArrowUpward,
                        PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text"),
                        BoolToEnabledStringConverter.Convert(true),
                        ConfigureRoundGradesUp)
                }
            };

            if (VxPlatform.Current == Platform.Uwp)
            {
                layout.Children.Insert(0, new TextBlock
                {
                    Text = Title.ToUpper(),
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

        }

        private void ConfigureRoundGradesUp()
        {

        }

        private void ConfigureGradeScale()
        {
            ShowViewModel<ConfigureOverallGradeScaleViewModel>();
        }

        private void ShowViewModel<T>() where T : BaseViewModel
        {
            if (PowerPlannerApp.ShowSettingsPagesAsPopups)
            {
                ShowPopup(this);
            }
            else
            {
                FindAncestor<PagedViewModel>().Navigate(Activator.CreateInstance(typeof(T), this) as BaseViewModel);
            }
        }
    }
}
