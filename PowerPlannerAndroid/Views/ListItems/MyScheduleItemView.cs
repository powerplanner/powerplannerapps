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
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.Themes;
using Android.Graphics.Drawables;
using InterfacesDroid.Helpers;
using ToolsPortable;
using Android.Graphics;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Bindings.Programmatic;
using BareMvvm.Core.Binding;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class MyScheduleItemView : LinearLayout
    {
        public ViewItemSchedule Schedule { get; private set; }

        private BindingHost _classBindingHost = new BindingHost();

        public MyScheduleItemView(Context context, ViewItemSchedule s, DateTime date) : base(context)
        {
            Schedule = s;
            _classBindingHost.DataContext = s.Class;

            base.Orientation = Orientation.Vertical;
            base.SetPaddingRelative(ThemeHelper.AsPx(context, 5), ThemeHelper.AsPx(context, 5), 0, 0);

            _classBindingHost.SetBinding<byte[]>(nameof(ViewItemClass.Color), color =>
            {
                base.Background = new ColorDrawable(ColorTools.GetColor(color));
            });

            // Can't figure out how to let both class name and room wrap while giving more importance
            // to room like I did on UWP, so just limiting name to 2 lines for now till someone complains.
            var textViewName = CreateTextView("");

            _classBindingHost.SetBinding<string>(nameof(ViewItemClass.Name), className =>
            {
                textViewName.Text = className;
            });

            textViewName.SetMaxLines(2);
            base.AddView(textViewName);

            base.AddView(CreateTextView(GetStringTimeToTime(s, date)));

            if (!string.IsNullOrWhiteSpace(s.Room))
            {
                base.AddView(CreateTextView(s.Room, autoLink: true));
            }
        }

        protected override void OnDetachedFromWindow()
        {
            _classBindingHost.Detach();
            base.OnDetachedFromWindow();
        }

        protected override void OnAttachedToWindow()
        {
            _classBindingHost.DataContext = Schedule.Class;
            base.OnAttachedToWindow();
        }

        public static string GetStringTimeToTime(ViewItemSchedule s, DateTime date)
        {
            return PowerPlannerResources.GetStringTimeToTime(DateHelper.ToShortTimeString(s.StartTimeInLocalTime(date)), DateHelper.ToShortTimeString(s.EndTimeInLocalTime(date)));
        }

        private TextView CreateTextView(string text, bool autoLink = false)
        {
            var view = new TextView(Context);

            if (autoLink)
            {
                // Auto link needs to be set first before setting text
                view.AutoLinkMask = Android.Text.Util.MatchOptions.All;
                view.SetLinkTextColor(Color.White);
            }

            view.Text = text;

            view.SetTextColor(new Color(255, 255, 255));

            return view;
        }
    }
}