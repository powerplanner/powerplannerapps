using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace PowerPlannerUWP.Views.GradeViews
{
    public class GradeWhatIfProperties : DependencyObject
    {
        public static readonly DependencyProperty IsInWhatIfModeProperty = DependencyProperty.Register("IsInWhatIfMode", typeof(bool?), typeof(GradeWhatIfProperties), null);

        public bool? IsInWhatIfMode
        {
            get { return GetValue(IsInWhatIfModeProperty) as bool?; }
            set { SetValue(IsInWhatIfModeProperty, value); }
        }

        public static bool GetIsInWhatIfMode(DependencyObject obj)
        {
            bool? val = obj.GetValue(IsInWhatIfModeProperty) as bool?;

            if (val != null)
                return val.Value;

            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            if (parent == null)
                return false;

            return GetIsInWhatIfMode(parent);
        }

        public static void SetIsInWhatIfMode(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsInWhatIfModeProperty, value);
        }
    }
}
