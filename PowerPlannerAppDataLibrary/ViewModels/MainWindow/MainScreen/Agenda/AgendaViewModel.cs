﻿using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using System.Collections;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.DataLayer;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda
{
    public class AgendaViewModel : BaseMainScreenViewModelChild
    {
        private AgendaViewItemsGroup _agendaViewItemsGroup;
        public AgendaViewItemsGroup AgendaViewItemsGroup
        {
            get { return _agendaViewItemsGroup; }
            set { SetProperty(ref _agendaViewItemsGroup, value, "AgendaViewItemsGroup"); }
        }

        private bool _hasNoItems;
        /// <summary>
        /// Returns true if there aren't any tasks/events. Supports binding.
        /// </summary>
        public bool HasNoItems
        {
            get { return _hasNoItems; }
            set { SetProperty(ref _hasNoItems, value, nameof(HasNoItems)); }
        }

        private ViewItemSemester _semester;

        public DateTime Today { get; private set; } = DateTime.Today;

        public MyObservableList<ViewItemClass> Classes
        {
            get { return _semester.Classes; }
        }

        public AgendaViewModel(BaseViewModel parent, Guid localAccountId, ViewItemSemester semester, DateTime today) : base(parent)
        {
            _semester = semester;
            Today = today.Date;
        }

        protected override async Task LoadAsyncOverride()
        {
            AgendaViewItemsGroup = await AgendaViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentLocalAccountId, MainScreenViewModel.CurrentSemester, Today);
            AgendaViewItemsGroup.Items.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Items_CollectionChanged).Handler;
            UpdateHasNoItems();

            ListenToLocalEditsFor<DataLayer.DataItems.DataItemMegaItem>().ChangedItems += ItemsLocallyEditedListener_ChangedItems;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateHasNoItems();
        }

        private void UpdateHasNoItems()
        {
            HasNoItems = AgendaViewItemsGroup.Items.Count == 0;
        }

        private Guid[] _lastChangedItemsIdentifiers;
        /// <summary>
        /// Gets the last changed item and then resets it to null
        /// </summary>
        /// <returns></returns>
        public ViewItemTaskOrEvent GetLastChangedItem()
        {
            // Grab reference of it since this could change in a background thread
            var lastIdentifiers = _lastChangedItemsIdentifiers;
            _lastChangedItemsIdentifiers = null;

            if (lastIdentifiers != null)
            {
                var item = AgendaViewItemsGroup.Items.FirstOrDefault(i => lastIdentifiers.Contains(i.Identifier));
                return item;
            }

            return null;
        }

        private void ItemsLocallyEditedListener_ChangedItems(object sender, Guid[] identifiers)
        {
            // Note that this executes on a background thread
            _lastChangedItemsIdentifiers = identifiers;
        }

        private IReadOnlyList<object> _itemsWithHeaders;
        public IReadOnlyList<object> ItemsWithHeaders
        {
            get
            {
                if (_itemsWithHeaders == null)
                {
                    _itemsWithHeaders = AgendaViewItemsGroup.Items.ToSortedList().ToHeaderedList<ViewItemTaskOrEvent, ItemsGroupHeader>(new GroupHeaderProvider(Today, Classes, MainScreenViewModel.CurrentAccount).GetHeader);
                }

                return _itemsWithHeaders;
            }
        }

        private class GroupHeaderProvider
        {
            public DateTime Today { get; private set; }
            public IEnumerable<ViewItemClass> Classes { get; private set; }
            public DateTime NextWeekStartDate { get; private set; }
            public GroupHeaderProvider(DateTime today, IEnumerable<ViewItemClass> classes, AccountDataItem account)
            {
                Today = today;
                Classes = classes;
                var weekChangesOn = account.WeekChangesOn;
                if (weekChangesOn == DayOfWeek.Sunday)
                {
                    // For Sunday, we use Monday since that's typically when the workweek starts. We don't use Monday for everybody though, since some countries like Afghanistan start their workweek on Wednesday, so they could set their week changes on date for Wednesday and have the weeks work correctly.
                    weekChangesOn = DayOfWeek.Monday;
                }
                NextWeekStartDate = DateTools.Next(weekChangesOn, today.AddDays(1));

                _overdue = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.Overdue,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateInThePast(),
                    DateToUseForNewItems = Today.AddDays(-1)
                };

                _today = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.Today,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateToday(),
                    DateToUseForNewItems = Today
                };

                _tomorrow = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.Tomorrow,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateTomorrow(),
                    DateToUseForNewItems = Today.AddDays(1)
                };

                _inTwoDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.InTwoDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateInXDays(2),
                    DateToUseForNewItems = Today.AddDays(2)
                };

                _thisWeek = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.ThisWeek,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateThisWeek(),
                    DateToUseForNewItems = Today.AddDays(3)
                };

                _nextWeek = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.NextWeek,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateNextWeek(),
                    DateToUseForNewItems = NextWeekStartDate
                };

                _withinThirtyDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.WithinThirtyDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateWithinXDays(30),
                    DateToUseForNewItems = NextWeekStartDate.AddDays(7)
                };

                _withinSixtyDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.WithinSixtyDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateWithinXDays(60),
                    DateToUseForNewItems = Today.AddDays(31)
                };

                _inTheFuture = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.InTheFuture,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateFuture(),
                    DateToUseForNewItems = Today.AddDays(61)
                };
            }

            private ItemsGroupHeader _overdue;
            private ItemsGroupHeader _today;
            private ItemsGroupHeader _tomorrow;
            private ItemsGroupHeader _inTwoDays;
            private ItemsGroupHeader _thisWeek;
            private ItemsGroupHeader _nextWeek;
            private ItemsGroupHeader _withinThirtyDays;
            private ItemsGroupHeader _withinSixtyDays;
            private ItemsGroupHeader _inTheFuture;

            public ItemsGroupHeader GetHeader(ViewItemTaskOrEvent item)
            {
                DateTime todayAsUtc = DateTime.SpecifyKind(Today, DateTimeKind.Utc);
                DateTime itemDate = item.EffectiveDateForDisplayInDateBasedGroups.Date;

                if (itemDate <= todayAsUtc.AddDays(-1))
                    return _overdue;

                if (itemDate <= todayAsUtc)
                    return _today;

                if (itemDate <= todayAsUtc.AddDays(1))
                    return _tomorrow;

                if (itemDate <= todayAsUtc.AddDays(2))
                    return _inTwoDays;

                if (itemDate < NextWeekStartDate)
                    return _thisWeek;

                if (itemDate < NextWeekStartDate.AddDays(7))
                    return _nextWeek;

                if (itemDate <= todayAsUtc.AddDays(30))
                    return _withinThirtyDays;

                if (itemDate <= todayAsUtc.AddDays(60))
                    return _withinSixtyDays;

                return _inTheFuture;
            }
        }

        public enum ItemsGroup
        {
            Overdue,
            Today,
            Tomorrow,
            InTwoDays,
            ThisWeek,
            NextWeek,
            WithinThirtyDays,
            WithinSixtyDays,
            InTheFuture
        }

        public class ItemsGroupHeader : BindableBase
        {
            public ItemsGroup Group { get; set; }

            public string Header { get; set; }

            public DateTime DateToUseForNewItems { get; set; }

            public IEnumerable<ViewItemClass> Classes { get; set; }

            private bool _collapsed;
            public bool Collapsed
            {
                get => _collapsed;
                set => SetProperty(ref _collapsed, value, nameof(Collapsed));
            }
        }

        public void AddTask()
        {
            AddItem(TaskOrEventType.Task);
        }

        public void AddEvent()
        {
            AddItem(TaskOrEventType.Event);
        }

        private void AddItem(TaskOrEventType type)
        {
            MainScreenViewModel.ShowPopup(AddTaskOrEventViewModel.CreateForAdd(MainScreenViewModel, new AddTaskOrEventViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                Classes = MainScreenViewModel.Classes,
                SelectedClass = null,
                Type = type
            }));
        }

        public void ShowItem(ViewItemTaskOrEvent item)
        {
            MainScreenViewModel.ShowItem(item);
        }
    }
}
