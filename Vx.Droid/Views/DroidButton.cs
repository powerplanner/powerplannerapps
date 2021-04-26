using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidButton : DroidView<Vx.Views.Button, MaterialButton>
    {
        public DroidButton() : this(new MaterialButton(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialButtonOutlinedStyle))
        {
        }

        public DroidButton(MaterialButton button) : base(button)
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.Button oldView, Vx.Views.Button newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Text = newView.Text;
            View.Enabled = newView.IsEnabled;
        }
    }

    public class DroidTextButton : DroidButton
    {
        public DroidTextButton() : base(new MaterialButton(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialTextButtonStyle))
        {

        }
    }

    public class DroidAccentButton : DroidButton
    {
        public DroidAccentButton() : base(new MaterialButton(VxDroidExtensions.ApplicationContext))
        {

        }
    }
}