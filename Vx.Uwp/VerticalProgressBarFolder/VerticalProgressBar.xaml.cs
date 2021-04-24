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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP
{
    public enum FillFrom
    {
        Top,
        Bottom,
        Center
    }

    public sealed partial class VerticalProgressBar : UserControl
    {
        public VerticalProgressBar()
        {
            this.InitializeComponent();

            Background = Application.Current.Resources["ProgressBarBackgroundThemeBrush"] as Brush;
            Foreground = Application.Current.Resources["ProgressBarForegroundThemeBrush"] as Brush;
        }

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(VerticalProgressBar), new PropertyMetadata(0.0, OnValueChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as VerticalProgressBar).OnValueChanged(args);
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs args)
        {
            Animate();
        }

        #endregion

        #region FillFrom

        public static readonly DependencyProperty FillFromProperty = DependencyProperty.Register("FillFrom", typeof(FillFrom), typeof(VerticalProgressBar), new PropertyMetadata(FillFrom.Bottom, OnFillFromChanged));

        public FillFrom FillFrom
        {
            get { return (FillFrom)GetValue(FillFromProperty); }
            set { SetValue(FillFromProperty, value); }
        }

        private static void OnFillFromChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as VerticalProgressBar).OnFillFromChanged(args);
        }

        private void OnFillFromChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateCenter();
        }

        #endregion


        private Storyboard _storyboard;
        private void Animate()
        {
            if (_storyboard != null)
            {
                _storyboard.Pause();
            }

            DoubleAnimation a = new DoubleAnimation()
            {
                EasingFunction = new CubicEase(),
                Duration = TimeSpan.FromSeconds(1),
                From = scaleTransform.ScaleY,
                To = Math.Min(Math.Max(Value, 0), 1)
            };

            Storyboard.SetTarget(a, scaleTransform);
            Storyboard.SetTargetProperty(a, "ScaleY");

            _storyboard = new Storyboard();
            _storyboard.Children.Add(a);

            _storyboard.Begin();
        }

        private void UpdateCenter()
        {
            switch (FillFrom)
            {
                case InterfacesUWP.FillFrom.Top:
                    scaleTransform.CenterY = 0;
                    break;

                case InterfacesUWP.FillFrom.Bottom:
                    scaleTransform.CenterY = rectangleBackground.ActualHeight;
                    break;

                case InterfacesUWP.FillFrom.Center:
                    scaleTransform.CenterY = rectangleBackground.ActualHeight / 2;
                    break;

                default:
                    throw new NotImplementedException(FillFrom.ToString());
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCenter();
        }
    }
}
