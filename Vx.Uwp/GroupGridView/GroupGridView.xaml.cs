using InterfacesUWP.Grouping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public sealed partial class GroupGridView : UserControl
    {
        private class MyItemTemplateSelector : DataTemplateSelector
        {
            private GroupGridView _groupGridView;
            public MyItemTemplateSelector(GroupGridView gridView)
            {
                _groupGridView = gridView;
            }

            protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            {
                if (item is IGroupHeader)
                {
                    DataTemplateSelector headerSelector = _groupGridView.GroupHeaderTemplateSelector;
                    if (headerSelector != null)
                        return headerSelector.SelectTemplate(item, container);

                    return _groupGridView.GroupHeaderTemplate;
                }

                else if (item is IGroupFooter)
                {
                    DataTemplateSelector footerTemplateSelector = _groupGridView.GroupFooterTemplateSelector;
                    if (footerTemplateSelector != null)
                        return footerTemplateSelector.SelectTemplate(item, container);

                    return _groupGridView.GroupFooterTemplate;
                }

                else
                {
                    DataTemplateSelector itemTemplateSelector = _groupGridView.ItemTemplateSelector;

                    if (itemTemplateSelector != null)
                        return itemTemplateSelector.SelectTemplate(item, container);

                    return _groupGridView.ItemTemplate;
                }
            }
        }

        private class MyStyleSelector : StyleSelector
        {
            private GroupGridView _gridView;

            public MyStyleSelector(GroupGridView gridView)
            {
                _gridView = gridView;
            }

            protected override Style SelectStyleCore(object item, DependencyObject container)
            {
                if (item is IGroupHeader)
                    return _gridView.GroupHeaderContainerStyle;

                else if (item is IGroupFooter)
                {
                    return _gridView.GroupFooterContainerStyle;
                }

                else
                {
                    StyleSelector itemSelector = _gridView.ItemContainerStyleSelector;
                    if (itemSelector != null)
                        return itemSelector.SelectStyle(item, container);

                    else if (_gridView.ItemContainerStyle != null)
                        return _gridView.ItemContainerStyle;

                    return base.SelectStyleCore(item, container);
                }
            }
        }



        #region Properties

        private static void resetListCallback(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as GroupGridView).reset();
        }

        #region GroupHeaderTemplate

        public static readonly DependencyProperty GroupHeaderTemplateProperty = DependencyProperty.Register("GroupHeaderTemplate", typeof(DataTemplate), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public DataTemplate GroupHeaderTemplate
        {
            get { return GetValue(GroupHeaderTemplateProperty) as DataTemplate; }
            set { SetValue(GroupHeaderTemplateProperty, value); }
        }

        #endregion

        #region GroupHeaderTemplateSelector

        public static readonly DependencyProperty GroupHeaderTemplateSelectorProperty = DependencyProperty.Register("GroupHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public DataTemplateSelector GroupHeaderTemplateSelector
        {
            get { return GetValue(GroupHeaderTemplateSelectorProperty) as DataTemplateSelector; }
            set { SetValue(GroupHeaderTemplateSelectorProperty, value); }
        }

        #endregion

        #region GroupFooterTemplate

        public static readonly DependencyProperty GroupFooterTemplateProperty = DependencyProperty.Register("GroupFooterTemplate", typeof(DataTemplate), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public DataTemplate GroupFooterTemplate
        {
            get { return GetValue(GroupFooterTemplateProperty) as DataTemplate; }
            set { SetValue(GroupFooterTemplateProperty, value); }
        }

        #endregion

        #region GroupFooterTemplateSelector

        public static readonly DependencyProperty GroupFooterTemplateSelectorProperty = DependencyProperty.Register("GroupFooterTemplateSelector", typeof(DataTemplateSelector), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public DataTemplateSelector GroupFooterTemplateSelector
        {
            get { return GetValue(GroupFooterTemplateSelectorProperty) as DataTemplateSelector; }
            set { SetValue(GroupFooterTemplateSelectorProperty, value); }
        }

        #endregion

        #region ItemTemplate

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public DataTemplate ItemTemplate
        {
            get { return GetValue(ItemTemplateProperty) as DataTemplate; }
            set { SetValue(ItemTemplateProperty, value); }
        }

        #endregion

        #region ItemTemplateSelector

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return GetValue(ItemTemplateSelectorProperty) as DataTemplateSelector; }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        #endregion

        #region ItemsSource

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void onItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as GroupGridView).onItemsSourceChanged(args);
        }

        private void onItemsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue is INotifyCollectionChanged)
                (args.OldValue as INotifyCollectionChanged).CollectionChanged -= ItemsSource_CollectionChanged;

            if (args.NewValue is INotifyCollectionChanged)
                (args.NewValue as INotifyCollectionChanged).CollectionChanged += ItemsSource_CollectionChanged;
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object o in e.NewItems)
                        addItem(o);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object o in e.OldItems)
                        removeItem(o);
                    break;


                //both move and replace will be the same. Move means the item itself could have a different sorting I suppose.
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:

                    foreach (object o in e.OldItems)
                        removeItem(o);

                    foreach (object o in e.NewItems)
                        addItem(o);

                    break;

                case NotifyCollectionChangedAction.Reset:
                    reset();
                    break;
            }
        }

        #endregion

        #region GroupAdapter

        public static readonly DependencyProperty GroupAdapterProperty = DependencyProperty.Register("GroupAdapter", typeof(IGroupAdapter), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public IGroupAdapter GroupAdapter
        {
            get { return GetValue(GroupAdapterProperty) as IGroupAdapter; }
            set { SetValue(GroupAdapterProperty, value); }
        }

        #endregion

        #region GroupHeaderContainerStyle

        public static readonly DependencyProperty GroupHeaderContainerStyleProperty = DependencyProperty.Register("GroupHeaderContainerStyle", typeof(Style), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public Style GroupHeaderContainerStyle
        {
            get { return GetValue(GroupHeaderContainerStyleProperty) as Style; }
            set { SetValue(GroupHeaderContainerStyleProperty, value); }
        }

        #endregion

        #region GroupFooterContainerStyle

        public static readonly DependencyProperty GroupFooterContainerStyleProperty = DependencyProperty.Register("GroupFooterContainerStyle", typeof(Style), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public Style GroupFooterContainerStyle
        {
            get { return GetValue(GroupFooterContainerStyleProperty) as Style; }
            set { SetValue(GroupFooterContainerStyleProperty, value); }
        }

        #endregion

        #region ItemContainerStyle

        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public Style ItemContainerStyle
        {
            get { return GetValue(ItemContainerStyleProperty) as Style; }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        #endregion

        #region ItemContainerStyleSelector

        public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DependencyProperty.Register("ItemContainerStyleSelector", typeof(StyleSelector), typeof(GroupGridView), new PropertyMetadata(null, resetListCallback));

        public StyleSelector ItemContainerStyleSelector
        {
            get { return GetValue(ItemContainerStyleSelectorProperty) as StyleSelector; }
            set { SetValue(ItemContainerStyleSelectorProperty, value); }
        }

        #endregion

        #endregion




        private ObservableCollection<object> _collection = new ObservableCollection<object>();

        public GroupGridView()
        {
            this.InitializeComponent();


            GroupHeaderContainerStyle = (Style)Resources["UnstyledContainer"];
            GroupFooterContainerStyle = GroupHeaderContainerStyle;

            gridView.ItemTemplateSelector = new MyItemTemplateSelector(this);
            gridView.ItemContainerStyleSelector = new MyStyleSelector(this);
            gridView.ItemsSource = _collection;
            gridView.SelectionChanged += gridView_SelectionChanged;
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public object SelectedItem
        {
            get
            {
                if (gridView.SelectedItem is IGroupHeader || gridView.SelectedItem is IGroupFooter)
                    return null;

                return gridView.SelectedItem;
            }

            set { gridView.SelectedItem = value; }
        }

        void gridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged == null)
                return;

            removeHeadersAndFooters(e.AddedItems);
            removeHeadersAndFooters(e.RemovedItems);

            SelectionChanged(this, e);
        }

        private void removeHeadersAndFooters(IList<object> list)
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
                if (list[i] is IGroupHeader || list[i] is IGroupFooter)
                {
                    list.RemoveAt(i);
                    i--;
                }
        }

        public GridView GridView
        {
            get { return gridView; }
        }

        private void reset()
        {
            _collection.Clear();

            object itemsSource = ItemsSource;
            if (itemsSource is IEnumerable)
                foreach (object o in (itemsSource as IEnumerable))
                    addItem(o);
        }

        private void addItem(object item)
        {
            if (GroupAdapter == null)
                return;

            IGroupHeader correctHeader = GroupAdapter.GenerateHeader(item);

            for (int i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] is IGroupHeader)
                {
                    IGroupHeader header = _collection[i] as IGroupHeader;

                    int comp = correctHeader.CompareTo(header);

                    //if goes inside this header
                    if (comp == 0)
                    {
                        //binary search insert would have to walk to the end of the items anyways, so might as well do linear insert

                        //move to the first item
                        i++;

                        for (; i < _collection.Count; i++)
                        {
                            //if we're at the next header, insert at here
                            if (_collection[i] is IGroupFooter || _collection[i] is IGroupHeader)
                            {
                                _collection.Insert(i, item);
                                return;
                            }

                            //or if the item goes before the item in the list
                            else if (header.CompareInsideHeader(item, _collection[i]) < 0)
                            {
                                _collection.Insert(i, item);
                                return;
                            }

                            //otherwise keep looking through the list
                        }

                        //if it wasn't added, must be at the end of the list
                        _collection.Add(item);
                        return;
                    }

                    //if goes before this header
                    else if (comp < 0)
                    {
                        insert(i, correctHeader, item);
                        return;
                    }
                }
            }

            //item didn't find a matching header, needs a new header
            insert(_collection.Count, correctHeader, item);
            return;
        }

        private void insert(int index, IGroupHeader header, object item)
        {
            _collection.Insert(index, header);
            _collection.Insert(index + 1, item);

            if (GroupFooterTemplate != null)
                _collection.Insert(index + 2, new DefaultGroupFooter());
        }

        private void removeItem(object item)
        {
            for (int i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] == item)
                {
                    //remove the item
                    _collection.RemoveAt(i);

                    //if there's no items left in the header
                    if (i > 0 && _collection[i - 1] is IGroupHeader && (i == _collection.Count || _collection[i] is IGroupHeader || _collection[i] is IGroupFooter))
                    {
                        _collection.RemoveAt(i - 1);

                        if (_collection[i] is IGroupFooter)
                            _collection.RemoveAt(i - 1);
                    }

                    return;
                }
            }
        }
    }
}
