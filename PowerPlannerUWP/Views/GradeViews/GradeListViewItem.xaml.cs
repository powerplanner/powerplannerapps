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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.GradeViews
{
    public sealed partial class GradeListViewItem : UserControl
    {
        public event EventHandler<BaseViewItemHomeworkExamGrade> OnRequestViewGrade;

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
            OnRequestViewGrade?.Invoke(this, (BaseViewItemHomeworkExamGrade)DataContext);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(GradeListViewItem), null);

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
