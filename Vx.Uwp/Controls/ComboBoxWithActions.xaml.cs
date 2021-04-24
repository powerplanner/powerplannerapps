using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace InterfacesUWP.Controls
{
    public sealed partial class ComboBoxWithActions : UserControl
    {
        public event SelectionChangedEventHandler SelectionChanged;

        public ComboBoxWithActions()
        {
            this.InitializeComponent();

            UpdateItemTemplateSelector();
        }

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(ComboBoxWithActions), new PropertyMetadata(null));

        public IEnumerable<ComboBoxAction> Actions
        {
            get { return (IEnumerable<ComboBoxAction>)GetValue(ActionsProperty); }
            set { SetValue(ActionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Actions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof(IEnumerable<ComboBoxAction>), typeof(ComboBoxWithActions), new PropertyMetadata(null, OnActionsChanged));

        private static void OnActionsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as ComboBoxWithActions).OnActionsChanged(args);
        }

        private void OnActionsChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateItemsAndActions();
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ComboBoxWithActions), new PropertyMetadata(null, OnItemTemplateChanged));

        private static void OnItemTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as ComboBoxWithActions).OnItemTemplateChanged(args);
        }

        private void OnItemTemplateChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateItemTemplateSelector();
        }

        private void UpdateItemTemplateSelector()
        {
            ComboBox.ItemTemplateSelector = new ComboBoxWithActionsItemTemplateSelector()
            {
                ActionTemplate = Resources["ActionTemplate"] as DataTemplate,
                ItemTemplate = ItemTemplate
            };
        }



        public IEnumerable ItemsSource
        {
            get { return (IEnumerable<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<object>), typeof(ComboBoxWithActions), new PropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as ComboBoxWithActions).OnItemsChanged(args);
        }

        private void OnItemsChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateItemsAndActions();
        }

        private void UpdateItemsAndActions()
        {
            IEnumerable list;

            if (ItemsSource != null && Actions != null)
            {
                list = new MyAppendedObservableLists<object>(ItemsSource, Actions);
            }

            else if (ItemsSource != null)
            {
                list = ItemsSource;
            }

            else if (Actions != null)
            {
                list = Actions;
            }

            else
            {
                list = new object[0];
            }

            ComboBox.ItemsSource = list;
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ComboBoxWithActions), new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as ComboBoxWithActions).OnSelectedItemChanged(args);
        }

        private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateComboBoxSelectedItem();
        }

        private void UpdateComboBoxSelectedItem()
        {
            if (ComboBox.SelectedItem != SelectedItem)
            {
                int index = -1;
                int i = 0;
                foreach (var el in ItemsSource)
                {
                    if (object.Equals(el, SelectedItem))
                    {
                        index = i;
                        break;
                    }

                    i++;
                }

                // Setting SelectedItem doesn't seem to work well, so using index instead
                if (ComboBox.SelectedIndex != index)
                {
                    ComboBox.SelectedIndex = index;
                }
            }
        }

        private class ComboBoxWithActionsItemTemplateSelector : DataTemplateSelector
        {
            public DataTemplate ItemTemplate { get; set; }

            public DataTemplate ActionTemplate { get; set; }

            protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
            {
                if (item is ComboBoxAction)
                {
                    return ActionTemplate;
                }

                if (ItemTemplate != null)
                {
                    return ItemTemplate;
                }

                return base.SelectTemplateCore(item, container);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox.SelectedItem is ComboBoxAction action)
            {
                // Revert to previous selection
                UpdateComboBoxSelectedItem();

                // Perform the action
                action.Action.Invoke();

                return;
            }

            if (SelectedItem != ComboBox.SelectedItem)
            {
                var removed = SelectedItem == null ? new object[0] : new object[] { SelectedItem };
                SelectedItem = ComboBox.SelectedItem;
                var added = SelectedItem == null ? new object[0] : new object[] { SelectedItem };
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(removed, added));
            }
        }
    }

    public class ComboBoxAction
    {
        public string Title { get; set; }

        public Symbol Symbol { get; set; }

        public Action Action { get; set; }
    }

    
}
