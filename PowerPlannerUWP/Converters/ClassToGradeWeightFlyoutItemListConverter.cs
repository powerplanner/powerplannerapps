using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.Converters
{
    static class ClassToGradeWeightFlyoutItemListConverter
    {
        // This is so incredibly janky...
        public static List<RadioMenuFlyoutItem> GetMenuItems(ViewItemTaskOrEvent item)
        {

            var gradeWeightFlyout = (from weight in GetWeightCategories(item.Class)
                                     select new RadioMenuFlyoutItem {
                                         Text = weight.Name, 
                                         GroupName = "GradeWeightCategories", 
                                         IsChecked = weight.Identifier == item.WeightCategory.Identifier
                                     }).ToList();

            return gradeWeightFlyout;
        }

        // From AddTaskOrEventViewModel. May be able to clean up with a reference
        private static ViewItemWeightCategory[] GetWeightCategories(ViewItemClass c)
        {
            if (c.WeightCategories == null)
            {
                throw new NullReferenceException("ViewItemClass.WeightCategories was null. ClassId: " + c.Identifier + ".");
            }

            List<ViewItemWeightCategory> answer = new List<ViewItemWeightCategory>(c.WeightCategories.Count + 2);

            answer.Add(ViewItemWeightCategory.UNASSIGNED);

            answer.AddRange(c.WeightCategories);

            answer.Add(ViewItemWeightCategory.EXCLUDED);

            return answer.ToArray();
        }
    }
}
