using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemHolidayScheduleView : TextView
    {
        public ListItemHolidayScheduleView(Context context) : base(context)
        {
            base.Background = new ColorDrawable(new Color(228, 0, 137));
            base.Gravity = GravityFlags.CenterVertical;

            var padding = ThemeHelper.AsPx(context, 6);
            base.SetPadding(padding, padding, 0, padding);

            base.SetTextColor(Color.White);

            base.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, ThemeHelper.AsPx(context, 29))
            {
                BottomMargin = ThemeHelper.AsPx(context, 2)
            };

            base.SetMaxLines(1);

            base.Click += ListItemHolidayScheduleView_Click;
        }

        private void ListItemHolidayScheduleView_Click(object sender, EventArgs e)
        {
            if (Holiday != null)
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.ViewHoliday(Holiday);
            }
        }

        private ViewItemHoliday _holiday;
        public ViewItemHoliday Holiday
        {
            get { return _holiday; }
            set
            {
                _holiday = value;

                if (value != null)
                {
                    base.Text = value.Name;
                }
            }
        }
    }
}