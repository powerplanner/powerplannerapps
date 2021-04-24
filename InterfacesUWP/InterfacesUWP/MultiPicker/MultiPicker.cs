using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public class MultiPicker : Grid
    {
        private ListBox _listBox;

        private CustomMessageBoxWithContent _messageBox;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public MultiPicker()
        {
            base.Background = Brushes.White;
            base.Tapped += MultiPicker_Tapped;

            _messageBox = new CustomMessageBoxWithContent("", "OK");

            _listBox = new ListBox() { SelectionMode = Windows.UI.Xaml.Controls.SelectionMode.Multiple };
            _listBox.SelectionChanged += _listBox_SelectionChanged;
            _messageBox.Content = _listBox;
        }

        public object SelectedItem
        {
            get { return _listBox.SelectedItem; }
            set { _listBox.SelectedItem = value; }
        }

        public IList<object> SelectedItems
        {
            get { return _listBox.SelectedItems; }
        }

        void MultiPicker_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            _messageBox.Show();
        }

        void _listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            base.Children.Clear();

            base.Children.Add(GetMultipleDisplay(_listBox.SelectedItems));

            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        public object ItemsSource
        {
            get { return _listBox.ItemsSource; }
            set
            {
                _listBox.ItemsSource = value;
            }
        }

        public string FullTitle
        {
            get { return (_messageBox.Title as TextBlock).Text; }
            set { (_messageBox.Title as TextBlock).Text = value; }
        }

        protected virtual UIElement GetMultipleDisplay(IList<object> selectedItems)
        {
            TextBlock tb = new TextBlock()
            {
                FontSize = 18,
                Foreground = Brushes.Black,
                Margin = new Thickness(12)
            };

            if (selectedItems == null || selectedItems.Count == 0)
                tb.Text = "None Selected";

            if (selectedItems.Count == 1)
                tb.Text = selectedItems[0].ToString();

            else
            {
                string answer = "";

                for (int i = 0; i < selectedItems.Count; i++)
                {
                    if (selectedItems[i].ToString().Length > 3)
                        answer += selectedItems[i].ToString().Substring(0, 3);
                    else
                        answer += selectedItems[i].ToString();

                    answer += ", ";
                }

                tb.Text = answer.TrimEnd(' ', ',');
            }

            return tb;
        }
    }
}
