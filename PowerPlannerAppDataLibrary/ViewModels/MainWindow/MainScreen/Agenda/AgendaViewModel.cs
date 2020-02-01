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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using System.Collections;
using System.Collections.Specialized;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda
{
    public class AgendaViewModel : BaseMainScreenViewModelChild
    {
        public AgendaViewItemsGroup AgendaViewItemsGroup { get; set; }

        /// <summary>
        /// Returns true if there aren't any tasks/events. Supports binding.
        /// </summary>
        public bool HasNoItems { get; set; }

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
        public BaseViewItemHomeworkExam GetLastChangedItem()
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
                    _itemsWithHeaders = AgendaViewItemsGroup.Items.ToSortedList().ToHeaderedList<BaseViewItemHomeworkExam, ItemsGroup>(new GroupHeaderProvider(Today).GetHeader);
                }

                return _itemsWithHeaders;
            }
        }

        private class GroupHeaderProvider
        {
            public DateTime Today { get; private set; }
            public GroupHeaderProvider(DateTime today)
            {
                Today = today;
            }

            public ItemsGroup GetHeader(BaseViewItemHomeworkExam item)
            {
                DateTime todayAsUtc = DateTime.SpecifyKind(Today, DateTimeKind.Utc);
                DateTime itemDate = item.Date.Date;

                if (itemDate <= todayAsUtc.AddDays(-1))
                    return ItemsGroup.Overdue;

                if (itemDate <= todayAsUtc)
                    return ItemsGroup.Today;

                if (itemDate <= todayAsUtc.AddDays(1))
                    return ItemsGroup.Tomorrow;

                if (itemDate <= todayAsUtc.AddDays(2))
                    return ItemsGroup.InTwoDays;

                if (itemDate <= todayAsUtc.AddDays(7))
                    return ItemsGroup.WithinSevenDays;

                if (itemDate <= todayAsUtc.AddDays(14))
                    return ItemsGroup.WithinFourteenDays;

                if (itemDate <= todayAsUtc.AddDays(30))
                    return ItemsGroup.WithinThirtyDays;

                if (itemDate <= todayAsUtc.AddDays(60))
                    return ItemsGroup.WithinSixtyDays;

                return ItemsGroup.InTheFuture;
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

        public void AddHomework()
        {
            AddItem(AddHomeworkViewModel.ItemType.Homework);
        }

        public void AddExam()
        {
            AddItem(AddHomeworkViewModel.ItemType.Exam);
        }

        private void AddItem(AddHomeworkViewModel.ItemType type)
        {
            MainScreenViewModel.ShowPopup(AddHomeworkViewModel.CreateForAdd(MainScreenViewModel, new AddHomeworkViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                Classes = MainScreenViewModel.Classes,
                SelectedClass = null,
                Type = type
            }));
        }

        public void ShowItem(BaseViewItemHomeworkExam item)
        {
            MainScreenViewModel.ShowItem(item);
        }
    }
}
