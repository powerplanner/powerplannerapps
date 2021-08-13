using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidListItemButton : DroidView<Vx.Views.ListItemButton, FrameLayout>
    {
        public DroidListItemButton() : base(new FrameLayout(VxDroidExtensions.ApplicationContext))
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(ListItemButton oldView, ListItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                View.RemoveAllViews();

                if (view != null)
                {
                    View.AddView(view.CreateDroidView(VxView));
                }
            });
        }
    }
}