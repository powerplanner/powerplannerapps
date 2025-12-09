using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    public partial class IsCompleteToTitleForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
                return Application.Current.Resources["ApplicationSecondaryForegroundThemeBrush"];

            return Application.Current.Resources["ApplicationForegroundThemeBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
