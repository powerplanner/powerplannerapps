using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatYearItem : BaseChatItem
    {
        [Description("The name of the year.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Optional details for the year.")]
        public string Details { get; set; }

        [Description("How many credits have been earned for the year, based on the total of the passing semester credits in the year.")]
        public double CalculatedCreditsEarned { get; set; }

        [Description("Optional user-specified override for how many credits have been earned for the year, replacing the calculated credits.")]
        public double? OverriddenCreditsEarned { get; set; }

        [Description("The student's current calculated GPA for the year.")]
        public double CalculatedGpa { get; set; }

        [Description("Optional user-specified override for the student's GPA for the year, replacing the calculated GPA.")]
        public double? OverriddenGpa { get; set; }

        [Description("The semesters in the year.")]
        [NotNull]
        [DisallowNull]
        public List<ChatSemesterItem> Semesters { get; set; }

        public ChatYearItem(ViewItemYear year) : base(year)
        {
            Name = year.Name;
            Details = year.Details;
            CalculatedCreditsEarned = year.CalculatedCreditsEarned;
            OverriddenCreditsEarned = year.OverriddenCredits == PowerPlannerSending.Grade.UNGRADED ? null : year.OverriddenCredits;
            CalculatedGpa = year.CalculatedGPA;
            OverriddenGpa = year.OverriddenGPA == PowerPlannerSending.Grade.UNGRADED ? null : year.OverriddenGPA;
            Semesters = year.Semesters.Select(s => new ChatSemesterItem(s)).ToList();
        }
    }
}
