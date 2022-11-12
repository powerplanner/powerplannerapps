using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpListView : UwpView<Vx.Views.ListView, ListView>
    {
        public UwpListView()
        {
            View.Name = "ListView" + GetHashCode();
            View.SelectionMode = ListViewSelectionMode.None;

            var style = new Style();
            style.TargetType = typeof(ListViewItem);
            style.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(ListViewItem.PaddingProperty, new Thickness(0)));
            style.Setters.Add(new Setter(ListViewItem.MinHeightProperty, 0));
            View.ItemContainerStyle = style;
        }

        protected override void ApplyProperties(Vx.Views.ListView oldView, Vx.Views.ListView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Padding = newView.Padding.ToUwp();

            View.DataContext = newView.ItemTemplate;

            if (!object.ReferenceEquals(oldView?.Items, newView.Items))
            {
                View.ItemsSource = newView.Items;
            }

            if (newView.ItemTemplate != null)
            {
                if (View.ItemTemplate == null)
                {
                    View.ItemTemplate = UwpDataTemplateView.GetDataTemplate(View.Name);
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
