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
using InterfacesUWP.Converters;
using PowerPlannerAppDataLibrary.ViewItems;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.GradeViews
{
    public sealed partial class GradeSideBar : UserControl
    {
        public GradeSideBar()
        {
            this.InitializeComponent();

            base.Loaded += GradeSideBar_Loaded;
        }

        private void GradeSideBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (!GradeWhatIfProperties.GetIsInWhatIfMode(this))
                this.SetBinding(ColorProperty, new Binding()
                {
                    Path = new PropertyPath((DataContext is ViewItemGrade) ? "WeightCategory.Class.Color" : "Class.Color"),
                    Converter = new ColorArrayToColorConverter()
                });
            else
                this.SetBinding(ColorProperty, new Binding()
                {
                    Path = new PropertyPath("ColorWhenInWhatIfMode"),
                    Converter = new ColorArrayToColorConverter()
                });
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Windows.UI.Color), typeof(GradeSideBar), null);

        public Windows.UI.Color Color
        {
            get { return (Windows.UI.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }
}
