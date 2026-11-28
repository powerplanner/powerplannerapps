using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class ClassInputParams
    {
        [Description("The id of the class to fetch data for.")]
        [NotNull]
        [DisallowNull]
        public required Guid ClassId { get; set; }
    }

    internal class GradesResponse
    {
        [Description("The actual calculated grade for the class, as a percent from 0.0 to 1.0.")]
        public double CalculatedGrade { get; set; }

        [Description("The overridden grade for the class, as a percent from 0.0 to 1.0. If there is no overridden grade, this will be null.")]
        public double? OverriddenGrade { get; set; }

        [Description("The calculated GPA for the class, as a number from 0.0 to 4.0, or higher based on custom grading scales.")]
        public double CalculatedGpa { get; set; }

        [Description("The overridden GPA for the class, as a number from 0.0 to 4.0, or higher based on custom grading scales. If there is no overridden GPA, this will be null.")]
        public double? OverriddenGpa { get; set; }

        [Description("Indicates whether the class rounds grades up or not.")]
        public bool DoesRoundGradesUp { get; set; }

        [Description("Indicates whether the class averages grade totals or not.")]
        public bool ShouldAverageGradeTotals { get; set; }

        [Description("The credits the class is worth, or null if no credits assigned.")]
        public double? Credits { get; set; }

        [Description("Whether the class uses GPA numbers, or if it's a pass/fail class.")]
        public GpaType GpaType { get; set; }

        [Description("If it's a pass/fail class, this specifies the passing grade for the class, as a percent from 0.0 to 1.0.")]
        public double? PassingGrade { get; set; }

        [Description("The list of weight categories in the class. Each weight category contains a list of grades.")]
        [NotNull]
        [DisallowNull]
        public List<ChatWeightCategoryItem> WeightCategories { get; set; }
    }

    internal class GetGradesTool : LocalTool<ClassInputParams, GradesResponse>
    {
        public override string Name => "list_grades_and_weight_categories";
        public override string Description => "Lists the weight categories and grades from the specified class, and grade-related properties of the class itself like the current grade and GPA and associated settings.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetGradesTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Get grades for a specific class";
        }

        public override async Task<GradesResponse> InvokeWithResultAsync(ClassInputParams parameters)
        {
            var classItem = _mainScreenViewModel.Classes.FirstOrDefault(i => i.Identifier == parameters.ClassId);
            if (classItem == null)
            {
                throw new ArgumentException($"Class with identifier {parameters.ClassId} not found.");
            }

            var viewItemsGroupClass = await _mainScreenViewModel.LoadClassViewItemsGroupAsync(parameters.ClassId, includeWeights: true);
            viewItemsGroupClass.LoadGrades();
            await viewItemsGroupClass.LoadGradesTask;

            return new GradesResponse
            {
                CalculatedGrade = viewItemsGroupClass.Class.CalculatedGrade,
                OverriddenGrade = viewItemsGroupClass.Class.OverriddenGrade == PowerPlannerSending.Grade.UNGRADED ? null : viewItemsGroupClass.Class.OverriddenGrade,
                CalculatedGpa = viewItemsGroupClass.Class.CalculatedGPA,
                OverriddenGpa = viewItemsGroupClass.Class.OverriddenGPA == PowerPlannerSending.Grade.UNGRADED ? null : viewItemsGroupClass.Class.OverriddenGPA,
                DoesRoundGradesUp = viewItemsGroupClass.Class.DoesRoundGradesUp,
                ShouldAverageGradeTotals = viewItemsGroupClass.Class.ShouldAverageGradeTotals,
                Credits = viewItemsGroupClass.Class.Credits == PowerPlannerSending.Grade.NO_CREDITS ? null : viewItemsGroupClass.Class.Credits,
                GpaType = viewItemsGroupClass.Class.GpaType,
                PassingGrade = viewItemsGroupClass.Class.PassingGrade == PowerPlannerSending.Grade.UNGRADED ? null : viewItemsGroupClass.Class.PassingGrade,
                WeightCategories = viewItemsGroupClass.Class.WeightCategories.Select(i => new ChatWeightCategoryItem(i)).ToList()
            };
        }
    }
}
