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

namespace PowerPlannerAndroid.Views.Controls
{
    public class BorderlessButton : MaterialButton
    {
        public BorderlessButton(Context context) : base(context, null, Resource.Attribute.borderlessButtonStyle)
        {
        }
    }
}