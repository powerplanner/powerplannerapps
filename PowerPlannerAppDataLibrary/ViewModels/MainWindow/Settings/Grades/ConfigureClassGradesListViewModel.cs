using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGradesListViewModel : PopupComponentViewModel
    {
        [VxSubscribe]
        public ViewItemClass Class { get; private set; }

        public ConfigureClassGradesListViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            Class = c;

            // Technically I should bind to this but I currently don't, that's okay though, super rare that class name will be changed.
            Title = c.Name;
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(0, Theme.Current.PageMargin - 12, 0, Theme.Current.PageMargin - 12),
                    Children =
                        {
                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.Star,
                                PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header"),
                                CreditsToStringConverter.ConvertWithCredits(Class.Credits).ToLower(),
                                ConfigureCredits),

                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.FilterAlt,
                                PowerPlannerResources.GetString("ConfigureClassGrades_Items_WeightCategories.Title"),
                                PowerPlannerResources.GetString("ConfigureClassGrades_Items_WeightCategories.Subtitle"),
                                ConfigureWeightCategories),

                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.SwapHoriz,
                                PowerPlannerResources.GetString("Settings_GradeOptions_ListItemGpaType.Title"),
                                GpaTypeToStringConverter.Convert(Class.GpaType),
                                ConfigureGpaType),

                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.Calculate,
                                PowerPlannerResources.GetString(Class.GpaType == PowerPlannerSending.GpaType.Standard ? "ConfigureClassGrades_Items_GradeScale.Title" : "Settings_GradeOptions_ListItemPassingGrade.Title"),
                                Class.GpaType == PowerPlannerSending.GpaType.Standard ? PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Subtitle") : Class.PassingGrade.ToString("0.##%"),
                                () =>
                                {
                                    if (Class.GpaType == PowerPlannerSending.GpaType.Standard)
                                    {
                                        ConfigureGradeScale();
                                    }
                                    else
                                    {
                                        ConfigurePassingGrade();
                                    }
                                }),

                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.Refresh,
                                PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text"),
                                BoolToEnabledStringConverter.Convert(Class.ShouldAverageGradeTotals),
                                ConfigureAverageGrades),

                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.ArrowUpward,
                                PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text"),
                                BoolToEnabledStringConverter.Convert(Class.DoesRoundGradesUp),
                                ConfigureRoundGradesUp),

                            RenderOption(
                                MaterialDesign.MaterialDesignIcons.PublishedWithChanges,
                                PowerPlannerResources.GetString("ConfigureClassGrades_Items_FinalGrade.Title"),
                                BoolToEnabledStringConverter.Convert(Class.OverriddenGrade != PowerPlannerSending.Grade.UNGRADED || Class.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED),
                                ConfigureOverrideFinalGradeGpa)
                        }
                }
            };
        }

        private View RenderOption(string icon, string title, string subtitle, Action action)
        {
            return SettingsListViewModel.RenderOption(icon, title, subtitle, action);
        }

        public void ConfigureOverrideFinalGradeGpa()
        {
            ShowClassViewModel<ConfigureClassFinalGradeGpaViewModel>();
        }

        public void ConfigureCredits()
        {
            ShowClassViewModel<ConfigureClassCreditsViewModel>();
        }

        public void ConfigureGradeScale()
        {
            ShowClassViewModel<ConfigureClassGradeScaleViewModel>();
        }

        public void ConfigureWeightCategories()
        {
            ShowClassViewModel<ConfigureClassWeightCategoriesViewModel>();
        }

        public void ConfigureAverageGrades()
        {
            ShowClassViewModel<ConfigureClassAverageGradesViewModel>();
        }

        public void ConfigureRoundGradesUp()
        {
            ShowClassViewModel<ConfigureClassRoundGradesUpViewModel>();
        }

        public void ConfigureGpaType()
        {
            ShowClassViewModel<ConfigureClassGpaTypeViewModel>();
        }

        public void ConfigurePassingGrade()
        {
            ShowClassViewModel<ConfigureClassPassingGradeViewModel>();
        }

        private void ShowClassViewModel<T>() where T : BaseViewModel
        {
            FindAncestor<PagedViewModelWithPopups>().ShowPopup(Activator.CreateInstance(typeof(T), FindAncestor<PagedViewModelWithPopups>(), Class) as BaseViewModel);
        }

        public static void ShowViewModel<T>(BaseViewModel current) where T : BaseViewModel
        {
            current.FindAncestor<PagedViewModelWithPopups>().ShowPopup(Activator.CreateInstance(typeof(T), current.FindAncestor<PagedViewModelWithPopups>()) as BaseViewModel);
        }
    }
}
