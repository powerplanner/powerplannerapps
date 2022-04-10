using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid
{
    public class DroidNativeComponent : Android.Widget.FrameLayout, INativeComponent
    {
        public DroidNativeComponent(Context context, VxComponent component) : base(context)
        {
            Component = component;
        }

        public VxComponent Component { get; private set; }

        public SizeF ComponentSize { get; private set; }

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;
        public event EventHandler<bool> MouseOverChanged;

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            ComponentSize = new SizeF(ThemeHelper.FromPxPrecise(Context, w), ThemeHelper.FromPxPrecise(Context, h));
            ComponentSizeChanged?.Invoke(this, ComponentSize);
        }

        public void ChangeView(Vx.Views.View view)
        {
            base.RemoveAllViews();

            if (view != null)
            {
                base.AddView(view.CreateDroidView(null));
            }
        }
    }
}