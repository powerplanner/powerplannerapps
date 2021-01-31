using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    class ClassToGradeWeightFlyoutItemListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new List<MenuFlyoutItem>
            {
                new MenuFlyoutItem { Text = "1" },
                new MenuFlyoutItem { Text = "2" },
                new MenuFlyoutItem { Text = "3" }
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
