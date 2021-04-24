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

namespace InterfacesUWP
{
    public sealed partial class ZoomableImage : UserControl
    {
        public ZoomableImage()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ZoomableImage), null);

        public ImageSource Source
        {
            get { return GetValue(SourceProperty) as ImageSource; }
            set { SetValue(SourceProperty, value); }
        }

        private void thisControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ImageContainer.Width = e.NewSize.Width;
            ImageContainer.Height = e.NewSize.Height;
        }
    }
}
