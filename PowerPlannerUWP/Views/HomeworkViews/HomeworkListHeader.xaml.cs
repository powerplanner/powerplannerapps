using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerUWP.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.HomeworkViews
{
    public sealed partial class HomeworkListHeader : UserControl
    {
        public event EventHandler<RoutedEventArgs> OnHeaderClicked;

        public HomeworkListHeader()
        {
            this.InitializeComponent();
        }



        private DateTime? _dateToUseForNewItems;
        public DateTime? DateToUseForNewItems
        {
            get { return _dateToUseForNewItems; }
            set
            {
                _dateToUseForNewItems = value;
            }
        }

        public string Header
        {
            get { return header.Content as string; }
            set { header.Content = value; }
        }

        public bool AllowHolidays { get; set; }

        public static readonly DependencyProperty ClassesProperty = DependencyProperty.Register("Classes", typeof(IEnumerable<ViewItemClass>), typeof(HomeworkListHeader), null);

        public IEnumerable<ViewItemClass> Classes
        {
            get { return GetValue(ClassesProperty) as IEnumerable<ViewItemClass>; }
            set { SetValue(ClassesProperty, value); }
        }

        private void buttonAddHomework_Click(object sender, RoutedEventArgs e)
        {
            initializeAdding(TaskOrEventType.Task);
        }

        private void buttonAddExam_Click(object sender, RoutedEventArgs e)
        {
            initializeAdding(TaskOrEventType.Event);
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            //if only adding homework is enabled, directly show adding homework
            if (!IsAddExamEnabled && IsAddHomeworkEnabled)
                initializeAdding(TaskOrEventType.Task);

            //else if only adding exam is enabled, directly show adding exam
            else if (!IsAddHomeworkEnabled && IsAddExamEnabled)
                initializeAdding(TaskOrEventType.Event);

            //otherwise show the options
            else
            {
                App.ShowFlyoutAddHomeworkOrExam(
                    elToCenterFrom: buttonAdd,
                    addHomeworkAction: delegate { initializeAdding(TaskOrEventType.Task); },
                    addExamAction: delegate { initializeAdding(TaskOrEventType.Event); },
                    addHolidayAction: AllowHolidays ? new Action(delegate { AddHoliday(); }) : null);
            }
        }

        private ViewItemClass[] GetClassesForAdding()
        {
            var classes = Classes;
            if (classes == null)
                return new ViewItemClass[] { ClassForNewItems };

            return classes.ToArray();
        }

        private void initializeAdding(TaskOrEventType type)
        {
            var viewModel = PowerPlannerApp.Current.GetMainScreenViewModel();
            if (viewModel != null)
            {
                viewModel.ShowPopup(AddHomeworkViewModel.CreateForAdd(viewModel, new AddHomeworkViewModel.AddParameter()
                {
                    Classes = GetClassesForAdding(),
                    DueDate = DateToUseForNewItems,
                    HideClassPicker = IsClassPickerHidden,
                    HideDatePicker = IsDatePickerHidden,
                    SelectedClass = ClassForNewItems,
                    Type = type
                }));
            }
        }

        private void AddHoliday()
        {
            var viewModel = PowerPlannerApp.Current.GetMainScreenViewModel();
            if (viewModel != null)
            {
                viewModel.ShowPopup(AddHolidayViewModel.CreateForAdd(viewModel, new AddHolidayViewModel.AddParameter()
                {
                    SemesterIdentifier = viewModel.CurrentSemester.Identifier,
                    StartDate = DateToUseForNewItems != null ? DateToUseForNewItems.Value : DateTime.Today,
                    EndDate = DateToUseForNewItems != null ? DateToUseForNewItems.Value : DateTime.Today
                }));
            }
        }

        private void header_Click(object sender, RoutedEventArgs e)
        {
            if (OnHeaderClicked != null)
                OnHeaderClicked(this, e);
        }

        private bool _isAddHomeworkEnabled = true;
        public bool IsAddHomeworkEnabled
        {
            get { return _isAddHomeworkEnabled; }
            set { _isAddHomeworkEnabled = value; }
        }

        private bool _isAddExamEnabled = true;
        public bool IsAddExamEnabled
        {
            get { return _isAddExamEnabled; }
            set { _isAddExamEnabled = value; }
        }

        public static readonly DependencyProperty ClassForNewItemsProperty = DependencyProperty.Register("ClassForNewItems", typeof(ViewItemClass), typeof(HomeworkListHeader), null);
        
        public ViewItemClass ClassForNewItems
        {
            get { return GetValue(ClassForNewItemsProperty) as ViewItemClass; }
            set { SetValue(ClassForNewItemsProperty, value); }
        }

        public bool IsClassPickerHidden { get; set; }

        public bool IsDatePickerHidden { get; set; }
    }
}
