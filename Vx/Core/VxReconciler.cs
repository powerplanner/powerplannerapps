using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace Vx
{
    public static class VxReconciler
    {
        /// <summary>
        /// Helps reconcile. Note that the changeView action can return null views (when view cleared).
        /// </summary>
        /// <param name="oldView"></param>
        /// <param name="newView"></param>
        /// <param name="changeView"></param>
        /// <param name="transferView"></param>
        public static void Reconcile(View oldView, View newView, Action<View> changeView, Action<View> transferView = null)
        {
            if (oldView == null && newView == null)
            {
                return;
            }

            if (newView == null && oldView != null)
            {
                changeView(null);
            }

            else if (oldView == null || oldView.GetType() != newView.GetType())
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
