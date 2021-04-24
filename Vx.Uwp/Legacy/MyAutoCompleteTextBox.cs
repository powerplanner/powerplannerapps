using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public class Sample : TextBox
    {
        public Sample()
        {
        }
    }

    public sealed class MyAutoCompleteTextBlock : Control
    {
        TextBox tb = null;
        ListBox lb = null;
        Grid g = null;
        public MyAutoCompleteTextBlock()
        {
            this.DefaultStyleKey = typeof(MyAutoCompleteTextBlock);
        }


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.tb = GetTemplateChild("tbChild") as TextBox;
            this.lb = GetTemplateChild("lbChild") as ListBox;

            this.g = GetTemplateChild("spContainer") as Grid;

            if (tb != null && this.lb != null)
            {
                tb.TextChanged += tb_TextChanged;
            }

            if (this.ItemsSource != null)
                this.lb.ItemsSource = ItemsSource;

            this.g.MaxHeight = this.MaxHeight;
        }

        public ICollection<string> ItemsSource
        {
            get { return (ICollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string Text { get { return (this.tb == null ? null : this.tb.Text); } }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ICollection<string>), typeof(MyAutoCompleteTextBlock), new PropertyMetadata(null));

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lb.SelectionChanged -= lb_SelectionChanged;

            this.lb.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (String.IsNullOrWhiteSpace(this.tb.Text) || this.ItemsSource == null || this.ItemsSource.Count == 0)
                return;

            var sel = (from d in this.ItemsSource where d.StartsWith(this.tb.Text) select d);

            if (sel != null && sel.Count() > 0)
            {
                this.lb.ItemsSource = sel;
                this.lb.Visibility = Windows.UI.Xaml.Visibility.Visible;

                this.lb.SelectionChanged += lb_SelectionChanged;
            }

            //tb.TextChanged += tb_TextChanged;
        }

        void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tb.TextChanged -= tb_TextChanged;

            this.tb.Text = (string)this.lb.SelectedValue;

            this.tb.TextChanged += tb_TextChanged;

            this.lb.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }

}
