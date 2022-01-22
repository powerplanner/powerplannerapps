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

namespace Vx.Droid.Views
{
    public class DroidFrameLayout : DroidView<Vx.Views.FrameLayout, FrameLayout>
    {
        public DroidFrameLayout() : base(new FrameLayout(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(Vx.Views.FrameLayout oldView, Vx.Views.FrameLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.SetBackgroundColor(newView.BackgroundColor.ToDroid());

            ReconcileChildren(oldView?.Children, newView.Children, View);
        }
    }
}