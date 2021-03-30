using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxUwpNativeView<T, N> : VxNativeView<T, N> where T : VxView, IVxView
    {
        public VxUwpNativeView(T view) : base(view, Activator.CreateInstance<N>()) { }

        protected void SetListOfViewsOnCollection(VxView[] views, UIElementCollection collection, [CallerMemberName] string callerName = null)
        {
            SetListOfViews(views, (changeType, index, nativeView) =>
            {
                switch (changeType)
                {
                    case VxNativeViewListItemChange.Insert:
                        collection.Insert(index, nativeView.NativeView as UIElement);
                        break;

                    case VxNativeViewListItemChange.Replace:
                        collection[index] = nativeView.NativeView as UIElement;
                        break;

                    case VxNativeViewListItemChange.Remove:
                        collection.RemoveAt(index);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }, callerName);
        }

        public int GridRow { set => Grid.SetRow(NativeView as FrameworkElement, value); }

        public int GridColumn { set => Grid.SetColumn(NativeView as FrameworkElement, value); }
    }
}
