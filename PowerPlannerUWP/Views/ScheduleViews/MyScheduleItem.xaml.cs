using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vx.Extensions;
using Vx.Uwp;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.DateTimeFormatting;
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
    public sealed partial class MyScheduleItem : UserControl
    {
        private ScheduleItemComponent _scheduleItemComponent;

        public MyScheduleItem()
        {
            this.InitializeComponent();
        }

        private DayScheduleItemsArranger.ScheduleItem _scheduleItem;
        public DayScheduleItemsArranger.ScheduleItem ScheduleItem
        {
            get => _scheduleItem;
            set
            {
                if (_scheduleItem != value)
                {
                    _scheduleItem = value;

                    if (value != null)
                    {
                        if (_scheduleItemComponent == null)
                        {
                            _scheduleItemComponent = new ScheduleItemComponent
                            {
                                ScheduleItem = value
                            };
                            ScheduleItemComponentContainer.Child = _scheduleItemComponent.Render();
                        }
                        else
                        {
                            _scheduleItemComponent.ScheduleItem = value;
                        }
                    }
                }
            }
        }

        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHighlighted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(MyScheduleItem), new PropertyMetadata(false, OnIsHighlightedChanged));

        private static void OnIsHighlightedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyScheduleItem).OnIsHighlightedChanged(e);
        }

        private void OnIsHighlightedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsHighlighted)
                VisualStateManager.GoToState(this, "HighlightedState", true);
            else
                VisualStateManager.GoToState(this, "DefaultState", true);
        }
    }
}
