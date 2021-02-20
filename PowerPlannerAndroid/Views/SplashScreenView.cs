using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Vx;

namespace PowerPlannerAndroid.Views
{
    public class SplashScreenView : VxView
    {
        public SplashScreenView(Context context) : base(context)
        {
            View = new FrameLayout(context)
                .VxLayoutParams().StretchHeight().Apply()
                .VxBackgroundResource(Resource.Color.powerPlannerBlue)
                .VxChildren(

                    new ImageView(context)
                        .VxImageResource(Resource.Drawable.logo)
                        .VxScaleType(ImageView.ScaleType.CenterInside)
                        .VxLayoutParams().Width(120).Height(120).Gravity(GravityFlags.Center).ApplyForFrameLayout()

                );
        }
    }
}