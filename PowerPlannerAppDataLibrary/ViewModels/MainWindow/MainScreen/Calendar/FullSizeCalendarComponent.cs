using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar
{
    public class FullSizeCalendarComponent : VxComponent
    {
        [VxSubscribe]
        private CalendarViewModel _viewModel;

        private DateTime _thisMonth;
        private Func<int, View> _itemTemplate;

        public FullSizeCalendarComponent(CalendarViewModel viewModel)
        {
            _viewModel = viewModel;
            _thisMonth = DateTools.GetMonth(DateTime.Today);
            _itemTemplate = RenderContent;
        }

        protected override View Render()
        {
            return new SlideView
            {
                Position = VxValue.Create(DateTools.DifferenceInMonths(_viewModel.DisplayMonth, _thisMonth), i => _viewModel.DisplayMonth = _thisMonth.AddMonths(i)),
                ItemTemplate = _itemTemplate
            };

            //return new SlideView
            //{
            //    CurrentContent = RenderContent(_viewModel.DisplayMonth),
            //    PreviousContent = RenderContent(_viewModel.DisplayMonth.AddMonths(-1)),
            //    NextContent = RenderContent(_viewModel.DisplayMonth.AddMonths(1)),
            //    OnMovedNext = () => _viewModel.DisplayMonth = _viewModel.DisplayMonth.AddMonths(1),
            //    OnMovedPrevious = () => _viewModel.DisplayMonth = _viewModel.DisplayMonth.AddMonths(-1)
            //};
        }

        private View RenderContent(int position)
        {
            var month = _thisMonth.AddMonths(position);

            return new TextBlock
            {
                Text = month.ToString()
            };
        }
    }
}
