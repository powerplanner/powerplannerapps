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
    public class DroidLinearLayout : DroidView<Vx.Views.LinearLayout, Android.Widget.LinearLayout>
    {
        protected override void ApplyProperties(Vx.Views.LinearLayout oldView, Vx.Views.LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Orientation = Orientation.Vertical;

            ReconcileChildren(oldView?.Children, newView.Children, View);
        }
    }
}