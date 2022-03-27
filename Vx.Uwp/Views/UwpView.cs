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
            view.DragStarting += View_DragStarting;
            view.DragOver += View_DragOver;
            view.Drop += View_Drop;
        }

        private void View_Drop(object sender, DragEventArgs e)
        {
            if (VxView?.Drop != null)
            {
                var vxArgs = GetVxDragEventArgs(e);

                VxView.Drop(vxArgs);
            }
        }

        private void View_DragOver(object sender, DragEventArgs e)
        {
            if (VxView?.DragOver != null)
            {
                var vxArgs = GetVxDragEventArgs(e);

                VxView.DragOver(vxArgs);

                e.AcceptedOperation = (Windows.ApplicationModel.DataTransfer.DataPackageOperation)vxArgs.AcceptedOperation;
            }
        }

        private static Vx.Views.DragDrop.DragEventArgs GetVxDragEventArgs(DragEventArgs e)
        {
            var vxDataPackage = GetVxDataPackage(e);

            Vx.Views.DragDrop.DragDropModifiers vxModifiers = Vx.Views.DragDrop.DragDropModifiers.None;
            if ((e.Modifiers & Windows.ApplicationModel.DataTransfer.DragDrop.DragDropModifiers.Control) != 0)
            {
                vxModifiers |= Vx.Views.DragDrop.DragDropModifiers.Control;
            }

            return new Vx.Views.DragDrop.DragEventArgs(vxDataPackage, vxModifiers);
        }

        private static Vx.Views.DragDrop.DataPackage GetVxDataPackage(DragEventArgs e)
        {
            Vx.Views.DragDrop.DataPackage vxDataPackage = null;
            if (e.DataView.Properties.TryGetValue("VxDataPackage", out object o))
            {
                vxDataPackage = o as Vx.Views.DragDrop.DataPackage;
            }
            if (vxDataPackage == null)
            {
                vxDataPackage = new Vx.Views.DragDrop.DataPackage();
            }
            return vxDataPackage;
        }

        private void View_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            if (VxView?.DragStarting != null)
            {
                var vxArgs = new Vx.Views.DragDrop.DragStartingEventArgs();

                VxView.DragStarting(vxArgs);

                args.Data.Properties.Add("VxDataPackage", vxArgs.Data);
            }
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
            View.CanDrag = newView.CanDrag;
            View.AllowDrop = newView.AllowDrop;

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
