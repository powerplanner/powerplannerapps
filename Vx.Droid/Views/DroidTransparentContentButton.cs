using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidTransparentContentButton : DroidView<Vx.Views.TransparentContentButton, Android.Widget.FrameLayout>
    {
        public DroidTransparentContentButton() : base(new Android.Widget.FrameLayout(VxDroidExtensions.ApplicationContext))
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(TransparentContentButton oldView, TransparentContentButton newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.BackgroundColor != default(Color) || newView.CornerRadius != 0)
            {
                var border = new GradientDrawable();
                border.SetColor(newView.BackgroundColor.ToDroid());
                border.SetCornerRadius(AsPx(newView.CornerRadius));
                View.Background = border;
            }

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