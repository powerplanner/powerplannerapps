using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    internal class UwpNativeContentContainer : UwpView<Vx.Views.NativeContentContainer, Border>
    {
        protected override void ApplyProperties(Vx.Views.NativeContentContainer oldView, Vx.Views.NativeContentContainer newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.NativeContent != null)
            {
                if (!object.ReferenceEquals(oldView?.NativeContent, newView.NativeContent))
                {
                    View.Child = (UIElement)newView.NativeContent;
                }
            }
            else
            {
                View.Child = null;
            }
        }
    }
}
