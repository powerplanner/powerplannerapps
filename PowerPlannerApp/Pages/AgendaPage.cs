using PowerPlannerApp.ViewItems;
using PowerPlannerApp.ViewItemsGroups;
using PowerPlannerApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages
{
    public class AgendaPage : VxPage
    {
        private VxState<AgendaViewItemsGroup> _agendaViewItemsGroup = new VxState<AgendaViewItemsGroup>();
        private MainScreenPage _mainScreenPage;

        public AgendaPage(MainScreenPage mainScreenPage)
        {
            _mainScreenPage = mainScreenPage;
        }

        protected override async void Initialize()
        {
            base.Initialize();

            _agendaViewItemsGroup.Value = await AgendaViewItemsGroup.LoadAsync(_mainScreenPage.Account.LocalAccountId, _mainScreenPage.ScheduleViewItemsGroup.Semester, DateTime.Today);
        }

        protected override View Render()
        {
            if (_agendaViewItemsGroup.Value == null)
            {
                return null;
            }

            return new ListView
            {
                ItemsSource = _agendaViewItemsGroup.Value.Items,
                ItemTemplate = CreateViewCellItemTemplate<ViewItemTaskOrEvent, ListItemTaskOrEvent>()
            };
        }
    }
}
