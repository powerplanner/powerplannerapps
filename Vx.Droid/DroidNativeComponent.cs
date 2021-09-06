using Android.App;
using Android.Content;
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

namespace Vx.Droid
{
    public class DroidNativeComponent : FrameLayout, INativeComponent
    {
        public DroidNativeComponent(Context context, VxComponent component) : base(context)
        {
            Component = component;
        }

        public VxComponent Component { get; private set; }

        public SizeF ComponentSize => new SizeF(Width, Height);

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            ComponentSizeChanged?.Invoke(this, new SizeF(w, h));
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