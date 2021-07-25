using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx
{
    public static class VxReconciler
    {
        public static void Reconcile(View oldView, View newView, Action<View> changeView, Action<View> transferView = null)
        {
            if (oldView == null || oldView.GetType() != newView.GetType())
            {
                changeView(newView);
            }

            else
            {
                // Transfer over the properties
                oldView.NativeView.Apply(newView);

                transferView?.Invoke(newView);
            }
        }
    }
}
