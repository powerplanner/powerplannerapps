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
    public class DroidScrollView : DroidView<Vx.Views.ScrollView, ScrollView>
    {
        protected override void ApplyProperties(Vx.Views.ScrollView oldView, Vx.Views.ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                View.RemoveAllViews();
                View.AddView(view.CreateDroidView(VxView));
            });
        }
    }
}