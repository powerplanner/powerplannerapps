using InterfacesUWP;
using InterfacesUWP.Controls;
using InterfacesUWP.Converters;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerUWP.Flyouts;
using PowerPlannerUWP.Views.CalendarViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public abstract class MyBaseEventVisual : Grid
    {
        private MyAdditionalItemsVisual _additionalItemsVisual;
        private Grid _normalGrid;
        private Border _expandedContainer;
        private const int EXPANDED_CONTAINER_MARGIN = -3;
        private const int EXPANDED_CONTAINER_TOP_BOTTOM_PADDING = 6;

        public const string TELEMETRY_ON_CLICK_EVENT_NAME = "Click_ScheduleEventItem";

        public MyBaseEventVisual()
        {
            _normalGrid = new Grid()
            {
                ColumnDefinitions =
                    {
                        new ColumnDefinition() { Width = new GridLength(MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM) },
                        new ColumnDefinition() { Width = GridLength.Auto }
                    }
            };

            _normalGrid.VerticalAlignment = VerticalAlignment.Top;

            base.VerticalAlignment = VerticalAlignment.Top;
            base.HorizontalAlignment = HorizontalAlignment.Stretch;

            _additionalItemsVisual = new MyAdditionalItemsVisual();
            Grid.SetColumn(_additionalItemsVisual, 1);
            _normalGrid.Children.Add(_additionalItemsVisual);

            base.Children.Add(_normalGrid);

            var borderBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            borderBrush = new SolidColorBrush(borderBrush.Color)
            {
                Opacity = 0.3
            };

            _expandedContainer = new Border()
            {
                Visibility = Visibility.Collapsed,
                Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as Brush,
                CornerRadius = new CornerRadius(12),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(3),
                Padding = new Thickness(0, EXPANDED_CONTAINER_TOP_BOTTOM_PADDING, 0, EXPANDED_CONTAINER_TOP_BOTTOM_PADDING),
                Margin = new Thickness(EXPANDED_CONTAINER_MARGIN)
            };
            base.Children.Add(_expandedContainer);
        }

        public bool IsFullItem
        {
            set
            {
                if (value)
                {
                    _normalGrid.ColumnDefinitions[0] = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
                }
                else
                {
                    _normalGrid.ColumnDefinitions[0] = new ColumnDefinition() { Width = new GridLength(MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM) };
                }
            }
        }

        private DayScheduleItemsArranger.EventItem _item;
        public DayScheduleItemsArranger.EventItem Item
        {
            get { return _item; }
            set
            {
                _item = value;

                _additionalItemsVisual.AdditionalItems = null;

                if (_normalGrid.Children.Count == 2)
                {
                    _normalGrid.Children.RemoveAt(0);
                }

                if (value != null)
                {
                    var content = GenerateContent(value);
                    _normalGrid.Children.Insert(0, content);
                    _additionalItemsVisual.AdditionalItems = value.AdditionalItems;
                    _normalGrid.Height = value.Height;
                    _expandedContainer.MinHeight = value.Height;

                    if (value.CanExpand())
                    {
                        base.Tapped += MyBaseEventVisual_Tapped;
                        base.PointerEntered += MyFullEventItem_PointerEntered;
                        base.PointerExited += MyFullEventItem_PointerExited;
                        base.PointerCanceled += MyFullEventItem_PointerExited;
                        base.PointerCaptureLost += MyFullEventItem_PointerExited;
                    }
                    else
                    {
                        base.Tapped += MyBaseEventVisual_TappedForOpen;
                    }
                }
            }
        }

        private void MyBaseEventVisual_TappedForOpen(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item.Item);
            TelemetryExtension.Current?.TrackEvent(TELEMETRY_ON_CLICK_EVENT_NAME);
        }

        private bool _ignoreNextPointerExit;
        private void MyBaseEventVisual_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ShowFull())
            {
                e.Handled = true;
                _ignoreNextPointerExit = true;
            }
        }

        private void MyFullEventItem_PointerExited(object sender, object e)
        {
            if (_ignoreNextPointerExit)
            {
                _ignoreNextPointerExit = false;
                return;
            }
            HideFull();
        }

        private void MyFullEventItem_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
            ShowFull();
        }

        private bool _isFullShown = false;

        private bool ShowFull()
        {
            if (Item == null || !Item.CanExpand())
            {
                return false;
            }

            if (_expandedContainer.Child == null)
            {
                _expandedContainer.Child = GenerateFullContent(Item);
                (_expandedContainer.Child as FrameworkElement).SizeChanged += FullContent_SizeChanged;
            }

            _isFullShown = true;

            var dontWait = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
            {
                if (_isFullShown)
                {
                    _expandedContainer.Visibility = Visibility.Visible;
                }
            });

            return true;
        }

        private void FullContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double diff = (e.NewSize.Height + EXPANDED_CONTAINER_TOP_BOTTOM_PADDING + EXPANDED_CONTAINER_TOP_BOTTOM_PADDING - EXPANDED_CONTAINER_MARGIN) - _expandedContainer.ActualHeight;
            if (diff > 0)
            {
                _expandedContainer.Margin = new Thickness(
                    EXPANDED_CONTAINER_MARGIN,
                    EXPANDED_CONTAINER_MARGIN - diff,
                    EXPANDED_CONTAINER_MARGIN,
                    EXPANDED_CONTAINER_MARGIN);
            }
            else
            {
                _expandedContainer.Margin = new Thickness(EXPANDED_CONTAINER_MARGIN);
            }
        }

        public bool HideFull()
        {
            if (_isFullShown)
            {
                _isFullShown = false;
                _expandedContainer.Visibility = Visibility.Collapsed;
                return true;
            }

            return false;
        }

        protected abstract FrameworkElement GenerateContent(DayScheduleItemsArranger.EventItem item);

        protected FrameworkElement GenerateFullContent(DayScheduleItemsArranger.EventItem item)
        {
            StackPanel sp = new StackPanel();
            sp.Children.Add(new MainCalendarItemView()
            {
                DataContext = item.Item,
                TelemetryOnClickEventName = TELEMETRY_ON_CLICK_EVENT_NAME
            });
            if (item.AdditionalItems != null)
            {
                foreach (var i in item.AdditionalItems)
                {
                    sp.Children.Add(new MainCalendarItemView()
                    {
                        DataContext = i,
                        TelemetryOnClickEventName = TELEMETRY_ON_CLICK_EVENT_NAME
                    });
                }
            }

            return sp;
        }
    }

    public class MyFullEventItem : MyBaseEventVisual
    {
        public MyFullEventItem()
        {
            IsFullItem = true;
            ContextRequested += ShowContextFlyout;
        }

        protected override FrameworkElement GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new Grid();

            var rectangle = new Rectangle()
            {
                RadiusX = 12,
                RadiusY = 12,
                Fill = MyCollapsedEventItem.GetBackgroundBrush(item.Item)
            };
            grid.Children.Add(rectangle);

            var tb = new TextBlock()
            {
                Foreground = Brushes.White,
                Margin = new Windows.UI.Xaml.Thickness(6, 6, 0, 0),
                Text = item.Item.Name
            };
            if (item.Item.IsComplete)
            {
                TextBlockCompat.SetStrikethrough(tb, true);
            }
            grid.Children.Add(tb);

            return grid;
        }

        private void ShowContextFlyout(UIElement sender, Windows.UI.Xaml.Input.ContextRequestedEventArgs args)
        {
            MenuFlyout flyout = new TaskOrEventFlyout(Item.Item, new TaskOrEventFlyoutOptions
            {
                ShowGoToClass = true
            }).GetFlyout();

            // Show context flyout
            if (args.TryGetPosition(sender, out Point point))
            {
                flyout.ShowAt(sender as FrameworkElement, point);
            }
            else
            {
                flyout.ShowAt(sender as FrameworkElement);
            }
        }
    }

    public class MyCollapsedEventItem : MyBaseEventVisual
    {
        public const double WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly double SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly double SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 14;

        protected override FrameworkElement GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new Grid()
            {
                Width = WIDTH_OF_COLLAPSED_ITEM,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var rectangle = new Rectangle()
            {
                RadiusX = 12,
                RadiusY = 12,
                Fill = GetBackgroundBrush(item.Item)
            };
            grid.Children.Add(rectangle);

            var tb = new TextBlock()
            {
                Foreground = Brushes.White,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Windows.UI.Xaml.Thickness(0, 6, 0, 0),
                Text = item.Item.Name.Substring(0, 1)
            };
            grid.Children.Add(tb);

            return grid;
        }

        public static Brush GetBackgroundBrush(ViewItemTaskOrEvent item)
        {
            if (item.IsComplete)
            {
                return new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
            }

            else
            {
                return new ColorArrayToBrushConverter().Convert(item.Class?.Color, null, null, null) as Brush;
            }
        }
    }

    public class MyAdditionalItemsVisual : StackPanel
    {
        public MyAdditionalItemsVisual()
        {
            Margin = new Thickness(2, 0, 0, 0);
            Visibility = Visibility.Collapsed;
        }

        private IEnumerable<ViewItemTaskOrEvent> _additionalItems;
        public IEnumerable<ViewItemTaskOrEvent> AdditionalItems
        {
            get { return _additionalItems; }
            set
            {
                _additionalItems = value;

                if (value == null)
                {
                    base.Visibility = Visibility.Collapsed;
                    return;
                }

                foreach (var additional in value)
                {
                    this.Children.Add(new Ellipse()
                    {
                        Width = 9,
                        Height = 9,
                        Margin = new Thickness(0, 0, 0, 2),
                        Fill = MyCollapsedEventItem.GetBackgroundBrush(additional)
                    });
                }

                base.Visibility = this.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
