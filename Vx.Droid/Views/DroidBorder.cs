using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
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
    public class DroidBorder : DroidView<Vx.Views.Border, FrameLayout>
    {
        public DroidBorder() : base(new FrameLayout(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(Vx.Views.Border oldView, Vx.Views.Border newView)
        {
            base.ApplyProperties(oldView, newView);

            var border = new GradientDrawable();
            border.SetColor(newView.BackgroundColor.ToDroid());
            if (newView.BorderThickness.Top > 0)
            {
                border.SetStroke(AsPx(newView.BorderThickness.Top), newView.BorderColor.ToDroid());
            }
            View.Background = border;

            View.SetPadding(AsPx(newView.Padding.Left), AsPx(newView.Padding.Top), AsPx(newView.Padding.Right), AsPx(newView.Padding.Bottom));

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