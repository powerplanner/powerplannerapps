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
        public enum ItemType
        {
            Homework,
            Exams
        }

        public ItemType Type { get; private set; }

        private IReadOnlyList<object> _itemsWithHeaders;
        public IReadOnlyList<object> ItemsWithHeaders
        {
            get
            {
                if (_itemsWithHeaders == null)
                {
                    switch (Type)
                    {
                        case ItemType.Homework:
                            _itemsWithHeaders = ClassViewModel.ViewItemsGroupClass.Homework.ToHeaderedList<ViewItemHomework, DateTime>(ItemToGroupHeader);
                            break;

                        case ItemType.Exams:
                            _itemsWithHeaders = ClassViewModel.ViewItemsGroupClass.Exams.ToHeaderedList<ViewItemExam, DateTime>(ItemToGroupHeader);
                            break;

                        default:
                            throw new NotImplementedException("Unknown type");
                    }
                }

                return _itemsWithHeaders;
            }
        }

        private DateTime ItemToGroupHeader(BaseViewItemHomeworkExam item)
        {
            return item.Date.Date;
        }

        public ClassHomeworkOrExamsViewModel(ClassViewModel parent, ItemType type) : base(parent)
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
                    if (Type == ItemType.Homework)
                    {
                        OnPropertyChanged(nameof(IsPastCompletedItemsDisplayed));
                    }
                    break;

                case nameof(ClassViewItemsGroup.IsPastCompletedExamsDisplayed):
                    if (Type == ItemType.Exams)
                    {
                        OnPropertyChanged(nameof(IsPastCompletedItemsDisplayed));
                    }
                    break;

                case nameof(ClassViewItemsGroup.PastCompletedHomework):
                    if (Type == ItemType.Homework)
                    {
                        OnPropertyChanged(nameof(PastCompletedItemsWithHeaders));
                    }
                    break;

                case nameof(ClassViewItemsGroup.PastCompletedExams):
                    if (Type == ItemType.Exams)
                    {
                        OnPropertyChanged(nameof(PastCompletedItemsWithHeaders));
                    }
                    break;

                case nameof(ClassViewItemsGroup.HasPastCompletedHomework):
                    if (Type == ItemType.Homework)
                    {
                        OnPropertyChanged(nameof(HasPastCompletedItems));
                    }
                    break;

                case nameof(ClassViewItemsGroup.HasPastCompletedExams):
                    if (Type == ItemType.Exams)
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
                if (Type == ItemType.Homework)
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
                if (Type == ItemType.Homework)
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
                if (Type == ItemType.Homework)
                {
                    if (ClassViewModel.ViewItemsGroupClass.PastCompletedHomework == null)
                    {
                        return null;
                    }

                    if (_pastCompletedItemsWithHeaders == null)
                    {
                        _pastCompletedItemsWithHeaders = ClassViewModel.ViewItemsGroupClass.PastCompletedHomework.ToSortedList().ToHeaderedList<ViewItemHomework, DateTime>(ItemToGroupHeader);
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
                        _pastCompletedItemsWithHeaders = ClassViewModel.ViewItemsGroupClass.PastCompletedExams.ToSortedList().ToHeaderedList<ViewItemExam, DateTime>(ItemToGroupHeader);
                    }

                    return _pastCompletedItemsWithHeaders;
                }
            }
        }

        public void TogglePastCompletedItems()
        {
            if (Type == ItemType.Homework)
            {
                if (ClassViewModel.ViewItemsGroupClass.IsPastCompletedHomeworkDisplayed)
                {
                    ClassViewModel.ViewItemsGroupClass.HidePastCompletedHomework();
                }
                else
                {
                    ClassViewModel.ViewItemsGroupClass.ShowPastCompletedHomework();
                }
            }
            else
            {
                if (ClassViewModel.ViewItemsGroupClass.IsPastCompletedExamsDisplayed)
                {
                    ClassViewModel.ViewItemsGroupClass.HidePastCompletedExams();
                }
                else
                {
                    ClassViewModel.ViewItemsGroupClass.ShowPastCompletedExams();
                }
            }
        }

        public void ShowPastCompletedItems()
        {
            if (Type == ItemType.Homework)
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
            if (Type == ItemType.Homework)
            {
                ClassViewModel.ViewItemsGroupClass.HidePastCompletedHomework();
            }
            else
            {
                ClassViewModel.ViewItemsGroupClass.HidePastCompletedExams();
            }
        }

        public void ShowItem(BaseViewItemHomeworkExam item)
        {
            MainScreenViewModel.ShowItem(item);
        }

        public void Add()
        {
            AddHomeworkViewModel.ItemType addType;

            switch (Type)
            {
                case ItemType.Homework:
                    addType = AddHomeworkViewModel.ItemType.Homework;
                    break;

                case ItemType.Exams:
                    addType = AddHomeworkViewModel.ItemType.Exam;
                    break;

                default:
                    throw new NotImplementedException("Unknown type");
            }
            
            MainScreenViewModel.ShowPopup(AddHomeworkViewModel.CreateForAdd(MainScreenViewModel, new AddHomeworkViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                Classes = new List<ViewItemClass>() { ClassViewModel.ViewItemsGroupClass.Class },
                SelectedClass = ClassViewModel.ViewItemsGroupClass.Class,
                Type = addType,
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
