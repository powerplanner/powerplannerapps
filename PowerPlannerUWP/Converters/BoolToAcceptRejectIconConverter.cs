using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    // Converts a bool to either to a checkmark (true) or 'X' (false) icon
    class BoolToAcceptRejectIconConverter : IValueConverter
    {
        // Glyphs
        private const string CHECKMARK = "\xE73E";
        private const string CANCEL = "\xE711"; // (X)

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool icon = false;
            if (value is bool boolean)
            {
                // If parameter isn't null (i.e. 'Invert'), invert the value
                if (parameter != null) boolean = !boolean;

                icon = boolean;
            }

            return new FontIcon
            {
                Glyph = icon ? CHECKMARK : CANCEL
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
