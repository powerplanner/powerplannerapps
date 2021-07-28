using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SchoolTimeZoneSettingsView : ViewHostGeneric
    {
        public new SchoolTimeZoneSettingsViewModel ViewModel
        {
            get { return base.ViewModel as SchoolTimeZoneSettingsViewModel; }
            set { base.ViewModel = value; }
        }

        public SchoolTimeZoneSettingsView()
        {
            this.InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }
    }

    public class TimeZoneToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeZoneInfo tz)
            {
#if DEBUG
                return tz.DisplayName;
#else
                return SchoolTimeZoneSettingsViewModel.FormatWindows(tz);
#endif
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
