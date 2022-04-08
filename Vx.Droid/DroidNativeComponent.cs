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
    public class DroidNativeComponent : Android.Widget.FrameLayout, INativeComponent, Android.Views.View.IOnHoverListener
    {
        public DroidNativeComponent(Context context, VxComponent component) : base(context)
        {
            Component = component;

            if (component.SubscribeToIsMouseOver)
            {
                SetOnHoverListener(this);
                //GenericMotion += DroidNativeComponent_GenericMotion;
                //Hover += DroidNativeComponent_Hover;
            }
        }

        private void DroidNativeComponent_Hover(object sender, HoverEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Hover: " + this.GetHashCode());
            Component.IsMouseOver.Value = true;
        }

        private void DroidNativeComponent_GenericMotion(object sender, GenericMotionEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.HoverEnter || e.Event.Action == MotionEventActions.HoverMove)
            {
                System.Diagnostics.Debug.WriteLine("HoverEnterOrMove: " + this.GetHashCode());
                Component.IsMouseOver.Value = true;
            }
            else if (e.Event.Action == MotionEventActions.HoverExit)
            {
                System.Diagnostics.Debug.WriteLine("HoverExit: " + this.GetHashCode());
                Component.IsMouseOver.Value = false;
            }
        }

        public VxComponent Component { get; private set; }

        public SizeF ComponentSize { get; private set; }

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;

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

        public bool OnHover(Android.Views.View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.HoverEnter)
            {
                Component.IsMouseOver.Value = true;
            }
            else if (e.Action == MotionEventActions.HoverExit)
            {
                Component.IsMouseOver.Value = false;
            }
            return true;
        }
    }
}