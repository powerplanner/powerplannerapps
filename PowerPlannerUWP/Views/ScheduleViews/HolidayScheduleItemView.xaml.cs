using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public sealed partial class HolidayScheduleItemView : UserControl
    {
        public HolidayScheduleItemView()
        {
            this.InitializeComponent();
        }

        public ViewItemHoliday Holiday
        {
            get { return (ViewItemHoliday)GetValue(HolidayProperty); }
            set { SetValue(HolidayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Holiday.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HolidayProperty =
            DependencyProperty.Register(nameof(Holiday), typeof(ViewItemHoliday), typeof(HolidayScheduleItemView), new PropertyMetadata(null));

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ViewHoliday(Holiday);

            e.Handled = true;
        }
    }
}
