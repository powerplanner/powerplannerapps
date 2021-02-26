using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public class VxRecyclerView : RecyclerView
    {
        /// <summary>
        /// Initializes a RecyclerView with vertical scroll bars enabled
        /// </summary>
        /// <param name="context"></param>
        public VxRecyclerView(Context context) : base(new ContextThemeWrapper(context, Resource.Style.ScrollbarRecyclerView))
        {

        }
    }
}