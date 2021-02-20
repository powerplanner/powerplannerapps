using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Helpers;
using InterfacesDroid.Themes;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Converters;
using PowerPlannerAndroid.Vx;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemClassMenuItemView : VxView
    {
        public ListItemClassMenuItemView(Context context) : base(context)
        {
            View = new LinearLayout(context)
                .VxPadding(0, 6, 0, 6)
                .VxChildren(

                    // Circle with letter and color
                    new FrameLayout(context)
                        .VxLayoutParams().Width(40).Height(40).Gravity(GravityFlags.Center).Margins(20, 0, 0, 0).Apply()
                        .VxPadding(2)
                        .VxBackgroundResource(Resource.Drawable.circle)
                        .VxBackgroundTintList(Binding<byte[], ColorStateList>(nameof(ViewItemClass.Color), bytes => ColorTools.GetColorStateList(ColorTools.GetColor(bytes))))
                        .VxChildren(

                            new TextView(context)
                                .VxLayoutParams().AutoWidth().Gravity(GravityFlags.Center).ApplyForFrameLayout()
                                .VxTextColor(Color.White)
                                .VxText(Binding<string, string>(nameof(ViewItemClass.Name), name => Char.ToUpper(name.FirstOrDefault()).ToString()))
                                .VxTextSize(24)

                        ),

                    // Name
                    new TextView(context)
                        .VxText(Binding(nameof(ViewItemClass.Name)))
                        .VxLayoutParams().WidthWeight(1).Gravity(GravityFlags.CenterVertical).Apply()
                        .VxTextSize(18)
                        .VxMaxLines(1)
                        .VxTextColorResource(Resource.Color.foregroundFull)
                        .VxPadding(16, 0, 0, 0)

                );
        }
    }
}