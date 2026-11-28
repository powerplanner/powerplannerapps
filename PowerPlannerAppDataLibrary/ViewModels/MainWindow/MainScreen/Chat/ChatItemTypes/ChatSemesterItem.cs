using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatSemesterItem : BaseChatItem
    {
        [Description("The name of the semester.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Optional details for the semester.")]
        public string Details { get; set; }

        [Description("How many credits have been earned for the semester, based on the total of the passing classes in the semester.")]
        public double CalculatedCreditsEarned { get; set; }

        [Description("Optional user-specified override for how many credits have been earned for the semester, replacing the calculated credits.")]
        public double? OverriddenCreditsEarned { get; set; }

        [Description("The student's current calculated GPA for the semester.")]
        public double CalculatedGpa { get; set; }

        [Description("Optional user-specified override for the student's GPA for the semester, replacing the calculated GPA.")]
        public double? OverriddenGpa { get; set; }

        [Description("Optional start date of the semester.")]
        public DateOnly? StartDate { get; set; }

        [Description("Optional end date of the semester.")]
        public DateOnly? EndDate { get; set; }

        [Description("The classes within the semester.")]
        public List<ChatClassItemWithinSemester> Classes { get; set; }

        public ChatSemesterItem(ViewItemSemester item) : base(item)
        {
            Name = item.Name;
            Details = item.Details;
            CalculatedCreditsEarned = item.CalculatedCreditsEarned;
            OverriddenCreditsEarned = item.OverriddenCredits == PowerPlannerSending.Grade.UNGRADED ? null : item.OverriddenCredits;
            CalculatedGpa = item.CalculatedGPA;
            OverriddenGpa = item.OverriddenGPA == PowerPlannerSending.Grade.UNGRADED ? null : item.OverriddenGPA;
            StartDate = item.Start != PowerPlannerSending.DateValues.UNASSIGNED ? DateOnly.FromDateTime(item.Start) : null;
            EndDate = item.End != PowerPlannerSending.DateValues.UNASSIGNED ? DateOnly.FromDateTime(item.End) : null;
            Classes = item.Classes.Select(i => new ChatClassItemWithinSemester(i)).ToList();
        }
    }


    internal class ChatClassItemWithinSemester : BaseChatItem
    {
        [Description("The name of the class.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("The credits the class is worth, or null if no credits assigned.")]
        public double? Credits { get; set; }

        [Description("Whether the class uses GPA numbers, or if it's a pass/fail class.")]
        public GpaType GpaType { get; set; }

        [Description("If it's a pass/fail class, this specifies the passing grade for the class, as a percent from 0.0 to 1.0.")]
        public double? PassingGrade { get; set; }

        [Description("Optional start date of the class, if it starts later than the semester itself.")]
        public DateOnly? StartDate { get; set; }

        [Description("Optional end date of the class, if it ends earlier than the semester itself.")]
        public DateOnly? EndDate { get; set; }

        [Description("The student's current calculated grade percent for the class, expressed as a double from 0.0 to 1.0.")]
        public double CalculatedGrade { get; set; }

        [Description("Optional user-specified override for the student's grade percent for the class, replacing the calculated grade.")]
        public double? OverriddenGrade { get; set; }

        [Description("The student's current calculated GPA for the class.")]
        public double CalculatedGpa { get; set; }

        [Description("Optional user-specified override for the student's GPA for the class, replacing the calculated GPA.")]
        public double? OverriddenGpa { get; set; }

        public ChatClassItemWithinSemester(ViewItemClass c) : base(c)
        {
            Name = c.Name;
            Credits = c.Credits == PowerPlannerSending.Grade.NO_CREDITS ? null : c.Credits;
            StartDate = c.StartDate == DateValues.UNASSIGNED ? null : DateOnly.FromDateTime(c.StartDate);
            EndDate = c.EndDate == DateValues.UNASSIGNED ? null : DateOnly.FromDateTime(c.EndDate);
            CalculatedGrade = c.CalculatedGrade;
            OverriddenGrade = c.OverriddenGrade == PowerPlannerSending.Grade.UNGRADED ? null : c.OverriddenGrade;
            CalculatedGpa = c.CalculatedGPA;
            OverriddenGpa = c.OverriddenGPA == PowerPlannerSending.Grade.UNGRADED ? null : c.OverriddenGPA;
            GpaType = c.GpaType;
            PassingGrade = c.PassingGrade == PowerPlannerSending.Grade.UNGRADED ? null : c.PassingGrade;
        }
    }
}
