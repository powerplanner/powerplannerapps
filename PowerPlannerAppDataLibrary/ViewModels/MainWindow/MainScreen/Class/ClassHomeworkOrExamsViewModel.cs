using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using ToolsPortable;
using System.Collections;
using PowerPlannerAppDataLibrary.ViewItemsGroups;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassHomeworkOrExamsViewModel : BaseClassContentViewModel
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
                            _itemsWithHeaders = ClassViewModel.ViewItemsGroupClass.Homework.ToHeaderedList(ItemToGroupHeader);
                            break;

                        case TaskOrEventType.Event:
                            _itemsWithHeaders = ClassViewModel.ViewItemsGroupClass.Exams.ToHeaderedList(ItemToGroupHeader);
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

        public ClassHomeworkOrExamsViewModel(ClassViewModel parent, TaskOrEventType type) : base(parent)
        {
            Type = type;
        }

        protected override async Task LoadAsyncOverride()
        {
            ClassViewModel.ViewItemsGroupClass.PropertyChanged += ViewItemsGroupClass_PropertyChanged;

            await ClassViewModel.ViewItemsGroupClass.LoadHomeworkAndExamsTask;
        }

        private void ViewItemsGroupClass_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ClassViewItemsGroup.IsPastCompletedHomeworkDisplayed):
                    if (Type == TaskOrEventType.Task)
                    {
                        OnPropertyChanged(nameof(IsPastCompletedItemsDisplayed));
                    }
                    break;

                case nameof(ClassViewItemsGroup.IsPastCompletedExamsDisplayed):
                    if (Type == TaskOrEventType.Event)
                    {
                        OnPropertyChanged(nameof(IsPastCompletedItemsDisplayed));
                    }
                    break;

                case nameof(ClassViewItemsGroup.PastCompletedHomework):
                    if (Type == TaskOrEventType.Task)
                    {
                        OnPropertyChanged(nameof(PastCompletedItemsWithHeaders));
                    }
                    break;

                case nameof(ClassViewItemsGroup.PastCompletedExams):
                    if (Type == TaskOrEventType.Event)
                    {
                        OnPropertyChanged(nameof(PastCompletedItemsWithHeaders));
                    }
                    break;

                case nameof(ClassViewItemsGroup.HasPastCompletedHomework):
                    if (Type == TaskOrEventType.Task)
                    {
                        OnPropertyChanged(nameof(HasPastCompletedItems));
                    }
                    break;

                case nameof(ClassViewItemsGroup.HasPastCompletedExams):
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
                    return ClassViewModel.ViewItemsGroupClass.IsPastCompletedHomeworkDisplayed;
                }
                else
                {
                    return ClassViewModel.ViewItemsGroupClass.IsPastCompletedExamsDisplayed;
                }
            }
        }

        public bool HasPastCompletedItems
        {
            get
            {
                if (Type == TaskOrEventType.Task)
                {
                    return ClassViewModel.ViewItemsGroupClass.HasPastCompletedHomework;
                }
                else
                {
                    return ClassViewModel.ViewItemsGroupClass.HasPastCompletedExams;
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
                    if (ClassViewModel.ViewItemsGroupClass.PastCompletedHomework == null)
                    {
                        return null;
                    }

                    if (_pastCompletedItemsWithHeaders == null)
                    {
                        _pastCompletedItemsWithHeaders = ClassViewModel.ViewItemsGroupClass.PastCompletedHomework.ToSortedList().ToHeaderedList<ViewItemTaskOrEvent, DateTime>(ItemToGroupHeader);
                    }

                    return _pastCompletedItemsWithHeaders;
                }
                else
                {
                    if (ClassViewModel.ViewItemsGroupClass.PastCompletedExams == null)
                    {
                        return null;
                    }

                    if (_pastCompletedItemsWithHeaders == null)
                    {
                        _pastCompletedItemsWithHeaders = ClassViewModel.ViewItemsGroupClass.PastCompletedExams.ToSortedList().ToHeaderedList<ViewItemTaskOrEvent, DateTime>(ItemToGroupHeader);
                    }

                    return _pastCompletedItemsWithHeaders;
                }
            }
        }

        public void ShowPastCompletedItems()
        {
            if (Type == TaskOrEventType.Task)
            {
                ClassViewModel.ViewItemsGroupClass.ShowPastCompletedHomework();
            }
            else
            {
                ClassViewModel.ViewItemsGroupClass.ShowPastCompletedExams();
            }
        }

        public void HidePastCompletedItems()
        {
            if (Type == TaskOrEventType.Task)
            {
                ClassViewModel.ViewItemsGroupClass.HidePastCompletedHomework();
            }
            else
            {
                ClassViewModel.ViewItemsGroupClass.HidePastCompletedExams();
            }
        }

        public void ShowItem(ViewItemTaskOrEvent item)
        {
            MainScreenViewModel.ShowItem(item);
        }

        public void Add()
        {
            MainScreenViewModel.ShowPopup(AddHomeworkViewModel.CreateForAdd(MainScreenViewModel, new AddHomeworkViewModel.AddParameter()
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
