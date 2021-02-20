using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Google.Android.Material.AppBar;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.Controls
{
    public class PowerPlannerToolbar : MaterialToolbar
    {
        public PowerPlannerToolbar(Context context) : base(context)
        {
            this.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.powerPlannerBlue)));
            this.ForegroundTintList = ColorTools.GetColorStateList(Color.White);
            this.SetTitleTextColor(ColorTools.GetColorStateList(Color.White));
        }
    }
}