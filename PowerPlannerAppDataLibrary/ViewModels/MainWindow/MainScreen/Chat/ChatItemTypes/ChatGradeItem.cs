using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatGradeItem : BaseChatItem
    {
        [Description("The name of the grade.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [NotNull]
        [DisallowNull]
        public double GradeReceived { get; set; }

        [NotNull]
        [DisallowNull]
        public double GradeTotal { get; set; }

        [Description("Optional details for the grade.")]
        public string Details { get; set; }

        [Description("The date of the grade.")]
        [NotNull]
        [DisallowNull]
        public DateOnly Date { get; set; }

        [Description("Indicates whether the grade is dropped or not.")]
        [NotNull]
        [DisallowNull]
        public bool IsDropped { get; set; }

        public ChatGradeItem(BaseViewItemMegaItem item) : base(item)
        {
            Name = item.Name;
            GradeReceived = item.GradeReceived;
            GradeTotal = item.GradeTotal;
            Details = item.Details;
            Date = DateOnly.FromDateTime(item.Date);
            IsDropped = item.IsDropped;
        }
    }
}
