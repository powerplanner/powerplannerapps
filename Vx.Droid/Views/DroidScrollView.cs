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
        private FrameLayout _container;
        public DroidScrollView()
        {
            // Need to introduce second view so that bottom margin of the child view work correctly inside the scroll view
            _container = new FrameLayout(View.Context);
            View.AddView(_container);
        }

        protected override void ApplyProperties(Vx.Views.ScrollView oldView, Vx.Views.ScrollView newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view =>
            {
                _container.RemoveAllViews();
                _container.AddView(view.CreateDroidView(VxView));
            });
        }
    }
}