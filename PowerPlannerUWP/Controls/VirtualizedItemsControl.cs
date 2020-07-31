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
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (_exceededHeight && e.NewStartingIndex >= Children.Count)
                    {
                        // Non-visible section, no reason to change anything
                        return;
                    }

                    // If inserting, we need to add the elements so our list matches with the actual list
                    if (e.NewStartingIndex < Children.Count)
                    {
                        int dontRenderIndex = int.MaxValue;

                        if (_exceededHeight)
                        {
                            dontRenderIndex = e.NewItems.Count;

                            // If we're pushing some other items off to be non-visible, we can recycle those first so they can be used below
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                RecycleElementAt(Children.Count - 1);
                            }
                        }

                        for (int i = 0; i < e.NewItems.Count && i + e.NewStartingIndex < dontRenderIndex; i++)
                        {
                            CreateElement(i + e.NewStartingIndex, e.NewItems[i]);
                        }
                    }

                    // Otherwise, nothing to recycle, just invalidate below
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (_exceededHeight && e.OldStartingIndex >= Children.Count)
                    {
                        // Non-visible section, no reason to change anything
                        return;
                    }
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldStartingIndex < Children.Count)
                        {
                            RecycleElementAt(e.OldStartingIndex);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (_exceededHeight && e.NewStartingIndex >= Children.Count)
                    {
                        // Non-visible section, no reason to change anything
                        return;
                    }
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldStartingIndex + i < Children.Count)
                        {
                            if (Children[e.OldStartingIndex + 1] is FrameworkElement frameworkElement)
                            {
                                frameworkElement.DataContext = e.NewItems[i];
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }

            InvalidateMeasure();
        }

        private UIElement GetOrCreateElementAt(int index, object item)
        {
            if (index < Children.Count)
            {
                return Children[index];
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

            if (el is FrameworkElement frameworkEl && frameworkEl.DataContext != item)
            {
                frameworkEl.DataContext = item;
            }

            return el;
        }

        private void RecycleElementAt(int index)
        {
            ItemTemplate.RecycleElement(new Windows.UI.Xaml.ElementFactoryRecycleArgs()
            {
                Element = Children[index],
                Parent = this
            });

            if (Children[index] is FrameworkElement fEl)
            {
                fEl.DataContext = null;
            }

            Children.RemoveAt(index);
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
        }

        private bool _exceededHeight;

        protected override Size MeasureOverride(Size availableSize)
        {
            _exceededHeight = false;

            if (ItemTemplate == null)
            {
                return new Size(availableSize.Width, 0);
            }

            IEnumerable items = ItemsSource as IEnumerable;
            if (items == null)
            {
                return new Size(availableSize.Width, 0);
            }

            var availableItemSize = new Size(availableSize.Width, double.PositiveInfinity);

            double y = 0;
            int i = 0;
            foreach (var item in items)
            {
                var el = GetOrCreateElementAt(i, item);

                el.Measure(availableItemSize);

                y += el.DesiredSize.Height;

                i++;

                if (y >= availableSize.Height)
                {
                    _exceededHeight = true;
                    break;
                }
            }

            // Recycle remaining
            while (i < Children.Count)
            {
                RecycleElementAt(i);
            }

            return new Size(availableSize.Width, Math.Min(y, availableItemSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (ItemTemplate == null)
            {
                return new Size(finalSize.Width, 0);
            }

            IEnumerable items = ItemsSource as IEnumerable;
            if (items == null)
            {
                return new Size(finalSize.Width, 0);
            }

            double y = 0;
            int i = 0;
            foreach (var item in items)
            {
                var el = GetOrCreateElementAt(i, item);

                el.Arrange(new Rect(
                    x: 0,
                    y: y,
                    width: finalSize.Width,
                    height: el.DesiredSize.Height));

                y += el.DesiredSize.Height;

                i++;

                if (y >= finalSize.Height)
                {
                    break;
                }
            }

            // Recycle remaining
            while (i < Children.Count)
            {
                RecycleElementAt(i);
            }

            return new Size(finalSize.Width, Math.Min(y, finalSize.Height));
        }
    }
}
