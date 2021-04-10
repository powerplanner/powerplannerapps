using PowerPlannerApp.Views.CalendarViews;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages
{
    public class CalendarPage : VxPage
    {
        private VxState<DateTime> _month = new VxState<DateTime>(DateTools.GetMonth(DateTime.Today));

        protected override async void Initialize()
        {
            base.Initialize();

            //while (true)
            //{
            //    await Task.Delay(300);

            //    _month.Value = DateTools.GetMonth(_month.Value.AddMonths(1));
            //}
        }

        private static DateTime[] months = new DateTime[]
        {
            DateTools.GetMonth(DateTime.Today),
            DateTools.GetMonth(DateTime.Today.AddMonths(1)),
            DateTools.GetMonth(DateTime.Today.AddMonths(2)),
            DateTools.GetMonth(DateTime.Today.AddMonths(3)),
            DateTools.GetMonth(DateTime.Today.AddMonths(4)),
            DateTools.GetMonth(DateTime.Today.AddMonths(5)),
        };

        protected override View Render()
        {
            return new CalendarView();
            //return new CarouselView()
            //{
            //    ItemsSource = months,
            //    ItemTemplate = CreateItemTemplate<DateTime>("monthTemplate", month =>
            //    {
            //        return new MonthView { Month = month };
            //    })
            //};
        }
    }
}
