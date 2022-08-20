using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Google.Android.Material.FloatingActionButton;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidFloatingActionButton : DroidView<Vx.Views.FloatingActionButton, FloatingActionButton>
    {
        public DroidFloatingActionButton() : base(new FloatingActionButton(VxDroidExtensions.ApplicationContext))
        {
            // Always add icon for now
            View.SetImageResource(Resource.Drawable.add_48px);

            ViewCompat.SetBackgroundTintList(View, ColorTools.GetColorStateList(Vx.Views.Theme.Current.ChromeColor.ToDroid()));
            View.ImageTintList = ColorTools.GetColorStateList(Android.Graphics.Color.White);

            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            VxView?.Click?.Invoke();
        }
    }
}