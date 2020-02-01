using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    public abstract class BaseSingleItemViewItemsGroup<TViewItem> : BaseAccountViewItemsGroup where TViewItem : BaseViewItemHomeworkExam
    {
        private ScheduleViewItemsGroup _scheduleViewItemsGroup;
        public TViewItem Item { get; private set; }

        protected BaseSingleItemViewItemsGroup(Guid localAccountId, bool trackChanges) : base(localAccountId, trackChanges)
        {
        }

        protected async Task LoadBlocking(Guid examId)
        {
            var dataStore = await GetDataStore();

            if (dataStore == null)
            {
                throw new NullReferenceException("Account doesn't exist");
            }

            DataItemMegaItem dataItem;

            // We need ALL classes loaded, including their schedules, and their weight categories.
            // Might as well just use ScheduleViewItemsGroup therefore.

            Guid semesterId;
            Guid classId = Guid.Empty;

            using (await Locks.LockDataForReadAsync("BaseSingleItemViewItemsGroup.LoadBlocking"))
            {
                dataItem = dataStore.TableMegaItems.FirstOrDefault(i => i.Identifier == examId);
                if (dataItem == null)
                {
                    TelemetryExtension.Current?.TrackEvent("Error_LoadSingleItem_CouldNotFind", new Dictionary<string, string>()
                    {
                        { "ItemId", examId.ToString() }
                    });

                    // Leave the Item set to null
                    return;
                }

                if (dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Task || dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Event)
                {
                    semesterId = dataItem.UpperIdentifier;
                    classId = dataItem.UpperIdentifier;
                }
                else
                {
                    var dataClass = dataStore.TableClasses.FirstOrDefault(i => i.Identifier == dataItem.UpperIdentifier);
                    if (dataClass == null)
                    {
                        throw new NullReferenceException("Class not found. Item id " + examId);
                    }
                    semesterId = dataClass.UpperIdentifier;
                    classId = dataClass.Identifier;
                }
            }

            _scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(LocalAccountId, semesterId: semesterId, trackChanges: trackChanges, includeWeightCategories: true);

            // Grab the class for the item
            ViewItemClass viewClass;
            if (dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Task || dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Event)
            {
                viewClass = _scheduleViewItemsGroup.Semester.NoClassClass;
            }
            else
            {
                viewClass = _scheduleViewItemsGroup.Semester.Classes.FirstOrDefault(i => i.Identifier == classId);
                if (viewClass == null)
                {
                    throw new NullReferenceException("ViewItemClass not found. Item id " + examId);
                }
            }

            // And create the item
            Item = CreateItem(dataItem, viewClass);
        }

        protected abstract TViewItem CreateItem(DataItemMegaItem dataItem, ViewItemClass c);

        private ViewItemClass CreateClass(DataItemClass c)
        {
            return new ViewItemClass(c, createScheduleMethod: CreateSchedule, createWeightMethod: CreateWeight);
        }

        private ViewItemSchedule CreateSchedule(DataItemSchedule s)
        {
            return new ViewItemSchedule(s);
        }

        private ViewItemWeightCategory CreateWeight(DataItemWeightCategory w)
        {
            return new ViewItemWeightCategory(w);
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            ViewItemSemester s = Item?.GetClassOrNull()?.Semester;
            if (s != null)
            {
                s.HandleDataChangedEvent(e);
                Item.HandleDataChangedEvent(e);

                // And update the class
                var newClass = s.Classes.FirstOrDefault(c => c.Identifier == (Item.DataItem as DataItemMegaItem).UpperIdentifier);
                Item.GetType().GetRuntimeProperty("Class").SetValue(Item, newClass);
            }
        }
    }
}
