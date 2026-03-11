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
        // Disable the CS0067 warning
#pragma warning disable CS0067
        public event EventHandler ThemeChanged;
        public event EventHandler<bool> MouseOverChanged;
        // Re-enable the CS0067 warning
#pragma warning restore CS0067

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            ComponentSize = new SizeF(ThemeHelper.FromPxPrecise(Context, w), ThemeHelper.FromPxPrecise(Context, h));
            ComponentSizeChanged?.Invoke(this, ComponentSize);
        }

        private bool _rendered = false;

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (!_rendered)
            {
                _rendered = true;

                var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
                var width = MeasureSpec.GetSize(widthMeasureSpec);
                var height = MeasureSpec.GetSize(heightMeasureSpec);
                float widthF;
                float heightF;

                widthF = widthMode == MeasureSpecMode.Unspecified ? float.MaxValue : ThemeHelper.FromPxPrecise(Context, width);
                heightF = heightMode == MeasureSpecMode.Unspecified ? float.MaxValue : ThemeHelper.FromPxPrecise(Context, height);

                ComponentSize = new SizeF(widthF, heightF);
                Component.InitializeForDisplay(this);
            }

            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        public void ChangeView(Vx.Views.View view)
        {
            base.RemoveAllViews();

            if (view != null)
            {
                base.AddView(view.CreateDroidView(null));
            }
        }

        protected override void OnDetachedFromWindow()
        {
            Component?.PauseRendering();
            base.OnDetachedFromWindow();
        }

        protected override void OnAttachedToWindow()
        {
            Component?.ResumeRendering();
            base.OnAttachedToWindow();
        }
    }
}