using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlannerUWP.Converters
{
    static class ClassToGradeWeightFlyoutItemListConverter
    {
        // This is so incredibly janky...
        public static List<RadioMenuFlyoutItem> GetMenuItems(ViewItemTaskOrEvent item)
        {
            // Set the new grade weight of item
            Action<object, RoutedEventArgs> setNewGradeWeight(ViewItemWeightCategory newWeightCategory) 
            {
                return delegate (object sender, RoutedEventArgs e)
                {
                    DataChanges changes = new DataChanges();

                    var dataItem = (DataItemMegaItem)item.DataItem;
                    dataItem.WeightCategoryIdentifier = newWeightCategory.Identifier;

                    changes.Add(dataItem);

                    PowerPlannerApp.Current.SaveChanges(changes);
                };
            };

            // Map grade weight categories to RadioMenuFlyoutItems
            return  GetWeightCategories(item.Class)
                    .Select(weight =>
                        {
                            var flyout = new RadioMenuFlyoutItem
                            {
                                Text = weight.Name,
                                GroupName = "GradeWeightCategories",
                                IsChecked = weight.Identifier == item.WeightCategory.Identifier,
                            };

                            flyout.Click += new RoutedEventHandler(setNewGradeWeight(weight));

                            return flyout;
                        }
                    ).ToList();
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
