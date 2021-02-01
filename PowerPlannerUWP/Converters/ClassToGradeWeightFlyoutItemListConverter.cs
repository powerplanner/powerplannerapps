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
    class ClassToGradeWeightFlyoutItemListConverter : IValueConverter
    {
        // This is so incredibly janky...
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Make sure we have the right types
            if (!(value is ViewItemClass Class))
                throw new ArgumentException("'value' is not of type ViewItemClass");

            var weightCategories = GetWeightCategories(Class);
            //var currentWeightCategory = 

            // value (ViewItemClass)
            // parameter ()
            var gradeWeightFlyout = (from weight in weightCategories
                                                     select new MenuFlyoutItem
                                                     {
                                                         Text=weight.Name
                                                         //GroupName="GradeWeightCategories"
                                                     }).ToList();

            return gradeWeightFlyout;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        // From AddTaskOrEventViewModel. May be able to clean up with a reference
        private ViewItemWeightCategory[] GetWeightCategories(ViewItemClass c)
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
