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

        // The source list that _wrappedItemsSource was created from. Used so we only rebuild the
        // wrapper when the underlying list reference actually changes, not on every render.
        private object _wrappedItemsSourceSource;
        private MyAppendedObservableLists<object> _wrappedItemsSource;

        protected override void ApplyProperties(Vx.Views.ListView oldView, Vx.Views.ListView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Background = newView.BackgroundColor.ToUwpBrush();

            View.IsItemClickEnabled = newView.ItemClicked != null;

            View.Padding = newView.Padding.ToUwp();

            View.DataContext = newView.ItemTemplate;

            if (!object.ReferenceEquals(oldView?.Items, newView.Items))
            {
                // For some reason we need to cast this for it to work correctly in .NET 10...
                if (newView.Items is MyAppendedObservableLists<object> appendedObservableList)
                {
                    _wrappedItemsSourceSource = null;
                    _wrappedItemsSource = null;
                    View.ItemsSource = appendedObservableList;
                }
                else if (newView.Items is MyObservableList<object> observableList)
                {
                    // CsWinRT in .NET 10 fails to marshal types that derive from ObservableCollection<T>
                    // (such as MyObservableList and MyHeaderedObservableList) when they're assigned to a
                    // WinRT ItemsSource, throwing "Value does not fall within the expected range." right
                    // inside set_ItemsSource (before any item is even enumerated).
                    //
                    // MyAppendedObservableLists is a plain IList + INotifyCollectionChanged implementation
                    // (it does NOT derive from ObservableCollection), and CsWinRT projects it correctly.
                    // So we wrap the observable list in one (appending an empty list to satisfy its
                    // minimum of two lists) to get reliable projection while preserving change tracking.
                    // We cache the wrapper against its source list so we don't create a new wrapper (and
                    // re-assign ItemsSource) unnecessarily.
                    if (!object.ReferenceEquals(_wrappedItemsSourceSource, observableList))
                    {
                        _wrappedItemsSourceSource = observableList;
                        _wrappedItemsSource = new MyAppendedObservableLists<object>(observableList, System.Array.Empty<object>());
                    }

                    View.ItemsSource = _wrappedItemsSource;
                }
                else
                {
                    _wrappedItemsSourceSource = null;
                    _wrappedItemsSource = null;
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
