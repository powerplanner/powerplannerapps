using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Vx.Uwp.Views
{
    public class UwpSlideView : UwpView<Vx.Views.SlideView, UwpSlideView.MySlideView>
    {
        private static long _slideViewNum;
        public UwpSlideView()
        {
            View.Name = "SlideView" + _slideViewNum;
            _slideViewNum++;
            View.DeferUpdates = true;
            View.SelectionChanged += View_SelectionChanged;
            View.VerticalContentAlignment = VerticalAlignment.Stretch;
            View.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            View.Loaded += View_Loaded;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            if (VxView != null && !VxView.ShowMouseArrowIndicatorsOnHover)
            {
                var grid = (Grid)VisualTreeHelper.GetChild(View, 0);
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    if (grid.Children[i] is Button)
                    {
                        grid.Children.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private bool _ignoreSelectionChanged;
        private void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (View.SelectedItem == null || _ignoreSelectionChanged)
            {
                return;
            }

            int newPosition = (int)View.SelectedItem;
            if (VxView.Position != null && VxView.Position.Value != newPosition)
            {
                VxView.Position.ValueChanged?.Invoke(newPosition);
            }
        }

        public class MySlideView : FlipView
        {
            private int? _minPosition;
            public int? MinPosition
            {
                get => _minPosition;
                set
                {
                    if (value != _minPosition)
                    {
                        _minPosition = value;
                        UpdateItemsSource();
                    }
                }
            }

            private int? _maxPosition;
            public int? MaxPosition
            {
                get => _maxPosition;
                set
                {
                    if (value != _maxPosition)
                    {
                        _maxPosition = value;
                        UpdateItemsSource();
                    }
                }
            }

            public int ActualMinPosition => MinPosition ?? BUFFER * -1;
            private int ActualMaxPosition => MaxPosition ?? BUFFER;
            private int IndexOfZeroOffset
            {
                get
                {
                    // If min is 0, our array would start at zero with item 0, everything's 1-1 (so a 0 offset).
                    // [0, 1, 2, 3...]

                    // If min is 1, our array starts with 1, so its theoretical 0 value would be at index -1 (so a -1 offset).
                    // [1, 2, 3, 4...]

                    // If min is -1, our array starts with -1, so its 0 value would be at index 1 (a +1 offset)
                    // [-1, 0, 1, 2...]

                    return ActualMinPosition * -1;
                }
            }

            private const int BUFFER = 365 * 30; // Allow going back or forward 30 years

            private int _position;
            public int Position
            {
                get => _position;
                set
                {
                    if (value != _position)
                    {
                        _position = value;
                        UpdateSelectedIndex();
                    }
                }
            }

            private bool _hasDeferredUpdateSelectedIndex;
            private void UpdateSelectedIndex()
            {
                if (DeferUpdates)
                {
                    _hasDeferredUpdateSelectedIndex = true;
                    return;
                }

                if (ItemsSource == null)
                {
                    UpdateItemsSource();
                }

                SelectedIndex = _position + IndexOfZeroOffset;
                if (!object.Equals(SelectedItem, _position))
                {
                    SelectedItem = _position;
                }
            }

            public bool DeferUpdates { get; set; }

            public void ApplyDeferredUpdates()
            {
                var orig = DeferUpdates;
                DeferUpdates = false;

                if (_hasDeferredUpdateItemsSource)
                {
                    UpdateItemsSource();
                    _hasDeferredUpdateItemsSource = false;
                }

                if (_hasDeferredUpdateSelectedIndex)
                {
                    UpdateSelectedIndex();
                    _hasDeferredUpdateSelectedIndex = false;
                }

                DeferUpdates = orig;
            }

            private bool _hasDeferredUpdateItemsSource;
            private void UpdateItemsSource()
            {
                if (DeferUpdates)
                {
                    _hasDeferredUpdateItemsSource = true;
                    return;
                }

                int min = ActualMinPosition;
                int max = ActualMaxPosition;

                List<int> items = new List<int>();
                for (int i = min; i <= max; i++)
                {
                    items.Add(i);
                }

                ItemsSource = items;
            }
        }

        private Func<int, Vx.Views.View> _itemTemplate;
        protected override void ApplyProperties(Vx.Views.SlideView oldView, Vx.Views.SlideView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.MinPosition = newView.MinPosition;
            View.MaxPosition = newView.MaxPosition;
            View.Position = newView.Position.Value;
            View.Background = newView.BackgroundColor.ToUwpBrush();

            _ignoreSelectionChanged = true;
            View.ApplyDeferredUpdates();
            _ignoreSelectionChanged = false;

            if (_itemTemplate != newView.ItemTemplate)
            {
                // Our generic data template expects to take in an object, whereas the SlideView takes in an int (position),
                // so we need to adapt the template for that different type
                _itemTemplate = newView.ItemTemplate;
                Func<object, Vx.Views.View> genericItemTemplate = null;
                if (newView.ItemTemplate != null)
                {
                    genericItemTemplate = (obj) =>
                    {
                        int index = (int)obj;
                        if (newView.MinPosition == null && index == View.ActualMinPosition)
                        {
                            return null;
                        }

                        return newView.ItemTemplate(index);
                    };
                }
                View.DataContext = genericItemTemplate;
            }

            if (newView.ItemTemplate != null)
            {
                if (View.ItemTemplate == null)
                {
                    View.ItemTemplate = UwpDataTemplateView.GetDataTemplateWithVerticalContentStretch(View.Name);
                }
            }
            else
            {
                if (View.ItemTemplate != null)
                {
                    View.ItemTemplate = null;
                }
            }
        }
    }
}
