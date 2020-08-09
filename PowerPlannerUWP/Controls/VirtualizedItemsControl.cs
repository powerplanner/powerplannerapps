using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PowerPlannerUWP.Controls
{
    /// <summary>
    /// Only works with items that are all the same height.
    /// </summary>
    public class VirtualizedItemsControl : Panel
    {
        public VirtualizedItemsControl()
        {
            VerticalAlignment = VerticalAlignment.Top;
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(VirtualizedItemsControl), new PropertyMetadata(null, OnItemTemplateChanged));

        private static void OnItemTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as VirtualizedItemsControl).OnItemTemplateChanged(e);
        }

        private void OnItemTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                throw new NotImplementedException("Changing item template isn't supported");
            }

            InvalidateMeasure();
        }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(VirtualizedItemsControl), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as VirtualizedItemsControl).OnItemsSourceChanged(e);
        }

        private void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged oldColChanged)
            {
                oldColChanged.CollectionChanged -= ItemsSource_CollectionChanged;
            }

            RecycleAllElements();

            if (e.NewValue is INotifyCollectionChanged itemsSource)
            {
                itemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            }

            InvalidateMeasure();
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    RecycleAllElements();
                    InvalidateMeasure();
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (_lastVisibleIndex != null && e.NewStartingIndex > _lastVisibleIndex.Value)
                    {
                        // Non-visible section, no reason to change anything
                        break;
                    }

                    // If inserting, we need to add the elements so our list matches with the actual list
                    if (e.NewStartingIndex < _childrenCopy.Count)
                    {
                        _childrenCopy.InsertRange(e.NewStartingIndex, new UIElement[e.NewItems.Count]);
                    }

                    InvalidateMeasure();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (_lastVisibleIndex != null && e.OldStartingIndex > _lastVisibleIndex.Value)
                    {
                        // Non-visible section, no reason to change anything
                        return;
                    }

                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldStartingIndex + 1 < _childrenCopy.Count)
                        {
                            RecycleElementAt(e.OldStartingIndex + i);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (_lastVisibleIndex != null && e.NewStartingIndex > _lastVisibleIndex.Value)
                    {
                        // Non-visible section, no reason to change anything
                        return;
                    }

                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldStartingIndex + i < _childrenCopy.Count)
                        {
                            if (_childrenCopy[e.OldStartingIndex + 1] is FrameworkElement frameworkElement)
                            {
                                frameworkElement.DataContext = e.NewItems[i];
                            }
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// This is a temporary copy of the children list which will include null values inserted when new data items are added.
        /// </summary>
        private List<UIElement> _childrenCopy = new List<UIElement>();
        private double? _itemHeight;
        private int? _lastVisibleIndex;

        private UIElement GetOrCreateElementAt(int index, object item)
        {
            if (index < _childrenCopy.Count)
            {
                var el = _childrenCopy[index];

                if (el != null)
                {
                    return el;
                }

                // For example, last visible index is 2, and there's 3 items
                if (_lastVisibleIndex != null && Children.Count > _lastVisibleIndex.Value)
                {
                    el = Children.Last();
                    Children.RemoveAt(Children.Count - 1);
                    _childrenCopy.RemoveAt(_childrenCopy.Count - 1);
                    Children.Insert(index, el);
                    _childrenCopy[index] = el;
                    SetDataContext(el, item);
                    return el;
                }

                return CreateElement(index, item);
            }

            if (index > Children.Count)
            {
                throw new InvalidOperationException("There must be a programming error, items not requested in sequential order.");
            }

            return CreateElement(index, item);
        }

        private UIElement CreateElement(int index, object item)
        {
            var el = ItemTemplate.GetElement(new Windows.UI.Xaml.ElementFactoryGetArgs()
            {
                Data = item,
                Parent = this
            });

            Children.Insert(index, el);

            if (index < _childrenCopy.Count)
            {
                _childrenCopy[index] = el;
            }
            else
            {
                _childrenCopy.Add(el);
            }

            SetDataContext(el, item);

            return el;
        }

        private void SetDataContext(UIElement el, object item)
        {
            if (el is FrameworkElement frameworkEl && frameworkEl.DataContext != item)
            {
                frameworkEl.DataContext = item;
            }
        }

        private void RecycleElementAt(int index)
        {
            var el = _childrenCopy[index];

            if (el != null)
            {
                ItemTemplate.RecycleElement(new Windows.UI.Xaml.ElementFactoryRecycleArgs()
                {
                    Element = el,
                    Parent = this
                });

                if (el is FrameworkElement fEl)
                {
                    fEl.DataContext = null;
                }

                Children.Remove(el);
            }

            _childrenCopy.RemoveAt(index);
        }

        private void RecycleAllElements()
        {
            foreach (var el in Children)
            {
                ItemTemplate.RecycleElement(new Windows.UI.Xaml.ElementFactoryRecycleArgs()
                {
                    Element = el,
                    Parent = this
                });

                if (el is FrameworkElement fEl)
                {
                    fEl.DataContext = null;
                }
            }

            Children.Clear();
            _childrenCopy.Clear();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _itemHeight = null;
            _lastVisibleIndex = null;

            if (double.IsPositiveInfinity(availableSize.Width))
            {
                throw new Exception("Invalid use of VirtualizedItemsControl, it only supports fixed width.");
            }

            if (ItemTemplate == null)
            {
                return new Size(availableSize.Width, 0);
            }

            IEnumerable items = ItemsSource as IEnumerable;
            if (items == null)
            {
                return new Size(availableSize.Width, 0);
            }

            Size availableItemSize;

            double y = 0;
            int i = 0;
            foreach (var item in items)
            {
                var el = GetOrCreateElementAt(i, item);

                if (_itemHeight == null)
                {
                    el.Measure(new Size(availableSize.Width, double.PositiveInfinity));

                    _itemHeight = el.DesiredSize.Height;

                    if (double.IsPositiveInfinity(availableSize.Height))
                    {
                        _lastVisibleIndex = int.MaxValue;
                    }
                    else
                    {
                        // If there's only 40 height and items are 50 high, last index would be 0... if there's 80 height and items are 50, last index would be 1, if there's 100 height and items are 50, last index would still be 1
                        _lastVisibleIndex = (int)Math.Floor(availableSize.Height / _itemHeight.Value);
                    }

                    availableItemSize = new Size(availableSize.Width, _itemHeight.Value);
                }
                else
                {
                    el.Measure(availableItemSize);
                }

                i++;

                if (i >= _lastVisibleIndex.Value)
                {
                    break;
                }
            }

            // Recycle remaining
            while (i < _childrenCopy.Count)
            {
                RecycleElementAt(i);
            }

            return new Size(availableSize.Width, _lastVisibleIndex.GetValueOrDefault(0) * _itemHeight.GetValueOrDefault(0));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children == null)
            {
                return new Size(finalSize.Width, 0);
            }

            double y = 0;

            foreach (var child in Children)
            {
                child.Arrange(new Rect(
                    x: 0,
                    y: y,
                    width: finalSize.Width,
                    height: _itemHeight.Value));

                y += _itemHeight.Value;
            }

            return new Size(finalSize.Width, y);
        }
    }
}
