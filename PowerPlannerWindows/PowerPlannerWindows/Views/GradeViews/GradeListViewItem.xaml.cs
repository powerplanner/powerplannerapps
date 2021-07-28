using InterfacesUWP;
using InterfacesUWP.Converters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.GradeViews
{
    public sealed partial class GradeListViewItem : UserControl
    {
        public event EventHandler<BaseViewItemMegaItem> OnRequestViewGrade;

        public GradeListViewItem()
        {
            this.InitializeComponent();

            base.Loaded += GradeListViewItem_Loaded;
        }

        private void GradeListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (!GradeWhatIfProperties.GetIsInWhatIfMode(this))
                this.SetBinding(ColorProperty, new Binding()
                {
                    Path = new PropertyPath("WeightCategory.Class.Color"),
                    Converter = new ColorArrayToColorConverter()
                });
            else
                this.SetBinding(ColorProperty, new Binding()
                {
                    Path = new PropertyPath("ColorWhenInWhatIfMode"),
                    Converter = new ColorArrayToColorConverter()
                });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnRequestViewGrade?.Invoke(this, (BaseViewItemMegaItem)DataContext);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Windows.UI.Color), typeof(GradeListViewItem), null);

        public Windows.UI.Color Color
        {
            get { return (Windows.UI.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
