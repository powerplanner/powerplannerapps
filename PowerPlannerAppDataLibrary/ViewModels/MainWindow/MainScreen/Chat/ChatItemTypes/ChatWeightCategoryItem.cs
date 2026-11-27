using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatWeightCategoryItem : BaseChatItem
    {
        [Description("The name of the weight category.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Relative to WeightTotal, this is the current score achieved in this weight category. For example, if the weight total is 50 and the user had 90% of the points in this weight category, this would be 45.")]
        [NotNull]
        [DisallowNull]
        public double WeightAchieved { get; set; }

        [Description("The total weight of this weight category.")]
        [NotNull]
        [DisallowNull]
        public double WeightValue { get; set; }

        [Description("The grades associated with this weight category.")]
        [NotNull]
        [DisallowNull]
        public List<ChatGradeItem> Grades { get; set; } = new List<ChatGradeItem>();

        public ChatWeightCategoryItem(ViewItemWeightCategory item) : base(item)
        {
            Name = item.Name;
            WeightAchieved = item.WeightAchieved;
            WeightValue = item.WeightValue;
            Grades = item.Grades.Select(i => new ChatGradeItem(i)).ToList();
        }
    }
}
