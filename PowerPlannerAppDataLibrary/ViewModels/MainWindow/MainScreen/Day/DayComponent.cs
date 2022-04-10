﻿using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day
{
    public class DayComponent : VxComponent
    {
        private DateTime _thisDay;
        private Func<int, View> _itemTemplate;

        public SemesterItemsViewGroup SemesterItemsViewGroup { get; set; }
        public BaseMainScreenViewModelDescendant ViewModel { get; set; }
        public DateTime Today { get; set; } = DateTime.Today;
        public DateTime DisplayDate { get; set; }
        public Action<DateTime> OnDisplayDateChanged { get; set; }

        public DayComponent()
        {
            _thisDay = DateTime.Today;
            _itemTemplate = RenderContent;
        }

        protected override View Render()
        {
            return new SlideView
            {
                Position = VxValue.Create((DisplayDate.Date - _thisDay).Days, i => OnDisplayDateChanged(_thisDay.AddDays(i))),
                ItemTemplate = _itemTemplate,
            };
        }

        private View RenderContent(int position)
        {
            var day = _thisDay.AddDays(position);

            return new SingleDayComponent
            {
                Today = Today,
                SemesterItemsViewGroup = SemesterItemsViewGroup,
                ViewModel = ViewModel,
                Date = day
            };
        }
    }
}