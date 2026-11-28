using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
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
    internal class GetYearsAndSemestersToolResponse
    {
        [Description("The student's current calculated GPA.")]
        public double Gpa { get; set; }

        [Description("How many credits the student has earned (excludes failed classes).")]
        public double CreditsEarned { get; set; }

        [Description("How many credits the student has attempted (includes failed classes).")]
        public double CreditsPossible { get; set; }

        [Description("The list of years, each containing a list of semesters, each containing a list of classes.")]
        [NotNull]
        [DisallowNull]
        public List<ChatYearItem> Years { get; set; }
    }

    internal class GetYearsAndSemestersTool : LocalTool<BlankParams, GetYearsAndSemestersToolResponse>
    {
        public override string Name => "get_academic_overview";
        public override string Description => "Returns the overall GPA and credits, plus all academic years, semesters, and classes with their dates, credits, grades, and GPA values.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetYearsAndSemestersTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Get years and semesters";
        }

        public override async Task<GetYearsAndSemestersToolResponse> InvokeWithResultAsync(BlankParams parameters)
        {
            var yearsGroup = await YearsViewItemsGroup.LoadAsync(_mainScreenViewModel.CurrentLocalAccountId);

            return new GetYearsAndSemestersToolResponse
            {
                CreditsEarned = yearsGroup.School.CreditsEarned,
                CreditsPossible = yearsGroup.School.CreditsAffectingGpa,
                Gpa = yearsGroup.School.GPA,
                Years = yearsGroup.School.Years.Select(y => new ChatYearItem(y)).ToList()
            };
        }
    }
}
