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

namespace Vx.Droid.NativeViews
{
    public class VxDroidButton : VxDroidNativeView<VxButton, Button>, IVxButton
    {
        protected override void Initialize()
        {
            base.Initialize();

            NativeView.Click += NativeView_Click;
        }

        private void NativeView_Click(object sender, EventArgs e)
        {
            View.ClickAction?.Invoke();
        }

        public string Text { set => NativeView.Text = value; }
        public Action ClickAction { set { } }
    }
}