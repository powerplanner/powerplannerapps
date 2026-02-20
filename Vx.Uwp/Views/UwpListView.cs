using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpListView : UwpView<Vx.Views.ListView, ListView>
    {
        private static long _listViewNum;
        public UwpListView()
        {
            View.Name = "ListView" + _listViewNum;
            _listViewNum++;
            View.SelectionMode = ListViewSelectionMode.None;
            View.ItemClick += View_ItemClick;

            var style = new Style();
            style.TargetType = typeof(ListViewItem);
            style.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(ListViewItem.PaddingProperty, new Thickness(0)));
            style.Setters.Add(new Setter(ListViewItem.MinHeightProperty, 0));
            View.ItemContainerStyle = style;
        }

        private void View_ItemClick(object sender, ItemClickEventArgs e)
        {
            VxView.ItemClicked?.Invoke(e.ClickedItem);
        }

        protected override void ApplyProperties(Vx.Views.ListView oldView, Vx.Views.ListView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.IsItemClickEnabled = newView.ItemClicked != null;

            View.Padding = newView.Padding.ToUwp();

            View.DataContext = newView.ItemTemplate;

            if (!object.ReferenceEquals(oldView?.Items, newView.Items))
            {
                // For some reason we need to cast this for it to work correctly in .NET 10...
                if (newView.Items is MyAppendedObservableLists<object> appendedObservableList)
                {
                    View.ItemsSource = appendedObservableList;
                }
                else
                {
                    View.ItemsSource = newView.Items;
                }
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
