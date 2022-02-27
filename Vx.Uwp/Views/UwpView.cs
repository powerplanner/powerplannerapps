using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml;

namespace Vx.Uwp.Views
{
    public abstract class UwpView<V, N> : NativeView<V, N> where V : View where N : FrameworkElement
    {
        public UwpView() : this(Activator.CreateInstance<N>()) { }

        public UwpView(N view)
        {
            View = view;

            view.Tapped += View_Tapped;
        }

        private void View_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (VxView?.Tapped != null)
            {
                VxView.Tapped();
                e.Handled = true;
            }
            else if (View.GetType().GetEvent("Click") != null)
            {
                e.Handled = true;
            }
        }

        internal static void ReconcileList(IList<View> oldList, IList<View> newList, IList<UIElement> nativeList)
        {
            ReconcileList(
                oldList,
                newList,
                insert: (i, v) => nativeList.Insert(i, v.CreateFrameworkElement()),
                remove: i => nativeList.RemoveAt(i),
                replace: (i, v) => nativeList[i] = v.CreateFrameworkElement(),
                clear: () => nativeList.Clear()
                );
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            View.Margin = new Windows.UI.Xaml.Thickness(newView.Margin.Left, newView.Margin.Top, newView.Margin.Right, newView.Margin.Bottom);
            View.Width = newView.Width;
            View.Height = newView.Height;
            View.Opacity = newView.Opacity;
            View.HorizontalAlignment = newView.HorizontalAlignment.ToUwp();
            View.VerticalAlignment = newView.VerticalAlignment.ToUwp();

            LinearLayout.SetWeight(View, Vx.Views.LinearLayout.GetWeight(newView));
        }
    }
}
