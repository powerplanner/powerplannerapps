using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Google.Android.Material.Button;
using PowerPlannerAndroid.Vx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.Controls
{
    /// <summary>
    /// A full width button styled similarly to full width spinners or edit texts
    /// </summary>
    public class FullWidthButton : MaterialButton
    {
        public FullWidthButton(Context context) : base(context, null, Resource.Style.Widget_MaterialComponents_Button_TextButton)
        {
            this.SetTextAppearance(Resource.Style.TextAppearance_AppCompat_Medium);

            this.VxLayoutParams()
                .HeightAsPx(Resources.GetDimensionPixelSize(Resource.Dimension.fullWidthItemHeight))
                .Margins(8, 0, 8, 0)
                .Apply();

            this.VxPadding(8, 0, 8, 0);

            this.VxGravity(GravityFlags.Left | GravityFlags.CenterVertical);

            this.VxTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.foregroundFull)));
            this.VxBackgroundResource(0);

            this.VxTextStyle(Typeface.Default);

            this.SetSupportAllCaps(false);
        }
    }
}