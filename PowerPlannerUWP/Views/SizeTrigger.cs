using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace PowerPlannerUWP.Views
{
    public class SizeTrigger : StateTriggerBase
    {
        public SizeTrigger()
        {
        }

        public static readonly DependencyProperty MinHeightProperty = DependencyProperty.Register("MinHeight", typeof(double), typeof(SizeTrigger), new PropertyMetadata(0, OnPropertyChanged));

        public double MinHeight
        {
            get { return (double)GetValue(MinHeightProperty); }
            set { SetValue(MinHeightProperty, value); }
        }

        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register("MinWidth", typeof(double), typeof(SizeTrigger), new PropertyMetadata(0, OnPropertyChanged));

        public double MinWidth
        {
            get { return (double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }

        public static readonly DependencyProperty ElementToWatchProperty = DependencyProperty.Register("ElementToWatch", typeof(FrameworkElement), typeof(SizeTrigger), new PropertyMetadata(null, OnElementToWatchChanged));

        public FrameworkElement ElementToWatch
        {
            get { return (FrameworkElement)GetValue(ElementToWatchProperty); }
            set { SetValue(ElementToWatchProperty, value); }
        }

        private static void OnElementToWatchChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SizeTrigger).OnElementToWatchChanged(e);
        }

        private void OnElementToWatchChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is FrameworkElement)
                (e.OldValue as FrameworkElement).SizeChanged -= SizeTrigger_SizeChanged;

            if (e.NewValue is FrameworkElement)
                (e.NewValue as FrameworkElement).SizeChanged += SizeTrigger_SizeChanged;

            Update();
        }

        private void SizeTrigger_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SizeTrigger).Update();
        }

        private void Update()
        {
            if (ElementToWatch == null)
                return;

            bool isActive = ElementToWatch.ActualWidth >= MinWidth && ElementToWatch.ActualHeight >= MinHeight;

            SetActive(isActive);
        }
    }
}
