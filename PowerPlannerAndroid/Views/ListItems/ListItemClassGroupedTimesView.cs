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
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.DataTemplates;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Themes;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemClassGroupedTimesView : InflatedViewWithBinding
    {
        private ItemsControlWrapper _itemsControlWrapper;

        public ListItemClassGroupedTimesView(ViewGroup root) : base(Resource.Layout.ListItemClassGroupedTimes, root)
        {
            _itemsControlWrapper = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.GroupedTimesViewGroup))
            {
                ItemTemplate = new CustomDataTemplate<ViewItemSchedule>(CreateTimeView)
            };
        }

        private View CreateTimeView(ViewGroup root, ViewItemSchedule schedule)
        {
            LinearLayout layout = new LinearLayout(root.Context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    LinearLayout.LayoutParams.WrapContent)
            };

            layout.SetPadding(0, 0, 0, ThemeHelper.AsPx(root.Context, 4));

            layout.AddView(new TextView(root.Context)
            {
                Text = PowerPlannerResources.GetStringTimeToTime(DateHelper.ToShortTimeString(schedule.StartTime), DateHelper.ToShortTimeString(schedule.EndTime))
            });

            if (!string.IsNullOrWhiteSpace(schedule.Room))
            {
                layout.AddView(new TextView(root.Context)
                {
                    // Autolink needs to be set first before setting text
                    AutoLinkMask = Android.Text.Util.MatchOptions.All,
                    Text = schedule.Room.Trim()
                });
            }

            return layout;
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            ClassTimesViewModel.GroupedDay groupedDay = newValue as ClassTimesViewModel.GroupedDay;

            _itemsControlWrapper.ItemsSource = groupedDay?.Times;
        }
    }
}