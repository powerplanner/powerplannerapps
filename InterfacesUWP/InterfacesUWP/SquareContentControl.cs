using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public class SquareContentControl : ContentControl
    {
        public static readonly DependencyProperty StretchOrientationProperty = DependencyProperty.Register("StretchOrientation", typeof(Orientation), typeof(SquareContentControl), new PropertyMetadata(Orientation.Horizontal, OnStretchPropertyChanged));

        private static void OnStretchPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SquareContentControl).OnStretchPropertyChanged(e);
        }

        private void OnStretchPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (StretchOrientation)
            {
                case Orientation.Horizontal:
                    base.Height = base.ActualWidth;
                    break;

                case Orientation.Vertical:
                    base.Width = base.ActualHeight;
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the orientation that the control should stretch to
        /// </summary>
        public Orientation StretchOrientation
        {
            get { return (Orientation)GetValue(StretchOrientationProperty); }
            set { SetValue(StretchOrientationProperty, value); }
        }

        public SquareContentControl()
        {
            base.SizeChanged += SquareContentControl_SizeChanged;
        }

        private void SquareContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            switch (StretchOrientation)
            {
                case Orientation.Horizontal:
                    base.Height = e.NewSize.Width;
                    break;

                case Orientation.Vertical:
                    base.Width = e.NewSize.Height;
                    break;
            }
        }
    }
}
