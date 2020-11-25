using PowerPlannerAppDataLibrary.ViewItems;
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
                    _itemsWithHeaders = AgendaViewItemsGroup.Items.ToSortedList().ToHeaderedList<ViewItemTaskOrEvent, ItemsGroupHeader>(new GroupHeaderProvider(Today, Classes).GetHeader);
                }

                return _itemsWithHeaders;
            }
        }

        private class GroupHeaderProvider
        {
            public DateTime Today { get; private set; }
            public IEnumerable<ViewItemClass> Classes { get; private set; }
            public GroupHeaderProvider(DateTime today, IEnumerable<ViewItemClass> classes)
            {
                Today = today;
                Classes = classes;

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

                _withinSevenDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.WithinSevenDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateInXDays(7),
                    DateToUseForNewItems = Today.AddDays(3)
                };

                _withinFourteenDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.WithinFourteenDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateInXDays(14),
                    DateToUseForNewItems = Today.AddDays(8)
                };

                _withinThirtyDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.WithinThirtyDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateInXDays(30),
                    DateToUseForNewItems = Today.AddDays(15)
                };

                _withinSixtyDays = new ItemsGroupHeader()
                {
                    Group = ItemsGroup.WithinSixtyDays,
                    Classes = Classes,
                    Header = PowerPlannerResources.GetRelativeDateInXDays(60),
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
            private ItemsGroupHeader _withinSevenDays;
            private ItemsGroupHeader _withinFourteenDays;
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

                if (itemDate <= todayAsUtc.AddDays(7))
                    return _withinSevenDays;

                if (itemDate <= todayAsUtc.AddDays(14))
                    return _withinFourteenDays;

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
            WithinSevenDays,
            WithinFourteenDays,
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
