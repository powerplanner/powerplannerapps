using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using ToolsPortable;
using System.Collections;
using PowerPlannerAppDataLibrary.ViewItemsGroups;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassTasksOrEventsViewModel : BaseClassContentViewModel
    {
        public TaskOrEventType Type { get; private set; }

        private IReadOnlyList<object> _itemsWithHeaders;
        public IReadOnlyList<object> ItemsWithHeaders
        {
            get
            {
                if (_itemsWithHeaders == null)
                {
                    switch (Type)
                    {
                        case TaskOrEventType.Task:
                            _itemsWithHeaders = ClassViewModel.ViewItemsGroupClass.Tasks.ToHeaderedList(ItemToGroupHeader);
                            break;

                        case TaskOrEventType.Event:
                            _itemsWithHeaders = ClassViewModel.ViewItemsGroupClass.Events.ToHeaderedList(ItemToGroupHeader);
                            break;

                        default:
                            throw new NotImplementedException("Unknown type");
                    }
                }

                return _itemsWithHeaders;
            }
        }

        private DateTime ItemToGroupHeader(ViewItemTaskOrEvent item)
        {
            return item.Date.Date;
        }

        public ClassTasksOrEventsViewModel(ClassViewModel parent, TaskOrEventType type) : base(parent)
        {
            Type = type;
        }

        protected override async Task LoadAsyncOverride()
        {
            ClassViewModel.ViewItemsGroupClass.PropertyChanged += ViewItemsGroupClass_PropertyChanged;

            await ClassViewModel.ViewItemsGroupClass.LoadTasksAndEventsTask;
        }

        private void ViewItemsGroupClass_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ClassViewItemsGroup.IsPastCompletedTasksDisplayed):
                    if (Type == TaskOrEventType.Task)
                    {
                        OnPropertyChanged(nameof(IsPastCompletedItemsDisplayed));
                    }
                    break;

                case nameof(ClassViewItemsGroup.IsPastCompletedEventsDisplayed):
                    if (Type == TaskOrEventType.Event)
                    {
                        OnPropertyChanged(nameof(IsPastCompletedItemsDisplayed));
                    }
                    break;

                case nameof(ClassViewItemsGroup.PastCompletedTasks):
                    if (Type == TaskOrEventType.Task)
                    {
                        OnPropertyChanged(nameof(PastCompletedItemsWithHeaders));
                    }
                    break;

                case nameof(ClassViewItemsGroup.PastCompletedEvents):
                    if (Type == TaskOrEventType.Event)
                    {
                        OnPropertyChanged(nameof(PastCompletedItemsWithHeaders));
                    }
                    break;

                case nameof(ClassViewItemsGroup.HasPastCompletedTasks):
                    if (Type == TaskOrEventType.Task)
                    {
                        OnPropertyChanged(nameof(HasPastCompletedItems));
                    }
                    break;

                case nameof(ClassViewItemsGroup.HasPastCompletedEvents):
                    if (Type == TaskOrEventType.Event)
                    {
                        OnPropertyChanged(nameof(HasPastCompletedItems));
                    }
                    break;
            }
        }

        public bool IsPastCompletedItemsDisplayed
        {
            get
            {
                if (Type == TaskOrEventType.Task)
                {
                    return ClassViewModel.ViewItemsGroupClass.IsPastCompletedTasksDisplayed;
                }
                else
                {
                    return ClassViewModel.ViewItemsGroupClass.IsPastCompletedEventsDisplayed;
                }
            }
        }

        public bool HasPastCompletedItems
        {
            get
            {
                if (Type == TaskOrEventType.Task)
                {
                    return ClassViewModel.ViewItemsGroupClass.HasPastCompletedTasks;
                }
                else
                {
                    return ClassViewModel.ViewItemsGroupClass.HasPastCompletedEvents;
                }
            }
        }

        private IReadOnlyList<object> _pastCompletedItemsWithHeaders;
        public IReadOnlyList<object> PastCompletedItemsWithHeaders
        {
            get
            {
                if (Type == TaskOrEventType.Task)
                {
                    if (ClassViewModel.ViewItemsGroupClass.PastCompletedTasks == null)
                    {
                        return null;
                    }

                    if (_pastCompletedItemsWithHeaders == null)
                    {
                        _pastCompletedItemsWithHeaders = ClassViewModel.ViewItemsGroupClass.PastCompletedTasks.ToSortedList().ToHeaderedList<ViewItemTaskOrEvent, DateTime>(ItemToGroupHeader);
                    }

                    return _pastCompletedItemsWithHeaders;
                }
                else
                {
                    if (ClassViewModel.ViewItemsGroupClass.PastCompletedEvents == null)
                    {
                        return null;
                    }

                    if (_pastCompletedItemsWithHeaders == null)
                    {
                        _pastCompletedItemsWithHeaders = ClassViewModel.ViewItemsGroupClass.PastCompletedEvents.ToSortedList().ToHeaderedList<ViewItemTaskOrEvent, DateTime>(ItemToGroupHeader);
                    }

                    return _pastCompletedItemsWithHeaders;
                }
            }
        }

        public void TogglePastCompletedItems()
        {
            if (Type == TaskOrEventType.Task)
            {
                if (ClassViewModel.ViewItemsGroupClass.IsPastCompletedTasksDisplayed)
                {
                    ClassViewModel.ViewItemsGroupClass.HidePastCompletedTasks();
                }
                else
                {
                    ClassViewModel.ViewItemsGroupClass.ShowPastCompletedTasks();
                }
            }
            else
            {
                if (ClassViewModel.ViewItemsGroupClass.IsPastCompletedEventsDisplayed)
                {
                    ClassViewModel.ViewItemsGroupClass.HidePastCompletedEvents();
                }
                else
                {
                    ClassViewModel.ViewItemsGroupClass.ShowPastCompletedEvents();
                }
            }
        }

        public void ShowPastCompletedItems()
        {
            if (Type == TaskOrEventType.Task)
            {
                ClassViewModel.ViewItemsGroupClass.ShowPastCompletedTasks();
            }
            else
            {
                ClassViewModel.ViewItemsGroupClass.ShowPastCompletedEvents();
            }
        }

        public void HidePastCompletedItems()
        {
            if (Type == TaskOrEventType.Task)
            {
                ClassViewModel.ViewItemsGroupClass.HidePastCompletedTasks();
            }
            else
            {
                ClassViewModel.ViewItemsGroupClass.HidePastCompletedEvents();
            }
        }

        public void ShowItem(ViewItemTaskOrEvent item)
        {
            MainScreenViewModel.ShowItem(item);
        }

        public void Add()
        {
            MainScreenViewModel.ShowPopup(AddTaskOrEventViewModel.CreateForAdd(MainScreenViewModel, new AddTaskOrEventViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                Classes = new List<ViewItemClass>() { ClassViewModel.ViewItemsGroupClass.Class },
                SelectedClass = ClassViewModel.ViewItemsGroupClass.Class,
                Type = Type,
                HideClassPicker = true
            }));
        }

        public override bool GoBack()
        {
            if (IsPastCompletedItemsDisplayed)
            {
                HidePastCompletedItems();
                return true;
            }

            return base.GoBack();
        }
    }
}
