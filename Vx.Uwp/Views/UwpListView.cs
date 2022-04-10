using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpListView : UwpView<Vx.Views.ListView, ListView>
    {
        public UwpListView()
        {
            View.Name = "ListView" + GetHashCode();
            View.SelectionMode = ListViewSelectionMode.None;
            View.IsItemClickEnabled = true;
            View.ItemClick += View_ItemClick;
        }

        private void View_ItemClick(object sender, ItemClickEventArgs e)
        {
            VxView?.OnItemClicked?.Invoke(e.ClickedItem);
        }

        protected override void ApplyProperties(Vx.Views.ListView oldView, Vx.Views.ListView newView)
        {
            base.ApplyProperties(oldView, newView);

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
