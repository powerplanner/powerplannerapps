using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
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

namespace PowerPlannerUWP.Views.TaskOrEventViews
{
    public sealed partial class TasksOrEventsListHeader : UserControl
    {
        public TasksOrEventsListHeader()
        {
            this.InitializeComponent();
        }

        public DateTime? DateToUseForNewItems
        {
            get { return (DateTime?)GetValue(DateToUseForNewItemsProperty); }
            set { SetValue(DateToUseForNewItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DateToUseForNewItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateToUseForNewItemsProperty =
            DependencyProperty.Register(nameof(DateToUseForNewItems), typeof(DateTime?), typeof(TasksOrEventsListHeader), new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(TasksOrEventsListHeader), new PropertyMetadata("TODAY"));

        public bool AllowHolidays { get; set; }

        public static readonly DependencyProperty ClassesProperty = DependencyProperty.Register("Classes", typeof(IEnumerable<ViewItemClass>), typeof(TasksOrEventsListHeader), null);

        public IEnumerable<ViewItemClass> Classes
        {
            get { return GetValue(ClassesProperty) as IEnumerable<ViewItemClass>; }
            set { SetValue(ClassesProperty, value); }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            //if only adding task is enabled, directly show adding task
            if (!IsAddEventEnabled && IsAddTaskEnabled)
                initializeAdding(TaskOrEventType.Task);

            //else if only adding event is enabled, directly show adding event
            else if (!IsAddTaskEnabled && IsAddEventEnabled)
                initializeAdding(TaskOrEventType.Event);

            //otherwise show the options
            else
            {
                App.ShowFlyoutAddTaskOrEvent(
                    elToCenterFrom: buttonAdd,
                    addTaskAction: delegate { initializeAdding(TaskOrEventType.Task); },
                    addEventAction: delegate { initializeAdding(TaskOrEventType.Event); },
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
                viewModel.ShowPopup(AddTaskOrEventViewModel.CreateForAdd(viewModel, new AddTaskOrEventViewModel.AddParameter()
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
            var collapsedProp = DataContext.GetType().GetProperty("Collapsed");
            if (collapsedProp != null && collapsedProp.PropertyType == typeof(bool))
            {
                collapsedProp.SetValue(DataContext, !(bool)collapsedProp.GetValue(DataContext));
            }
        }

        private bool _isAddTaskEnabled = true;
        public bool IsAddTaskEnabled
        {
            get { return _isAddTaskEnabled; }
            set { _isAddTaskEnabled = value; }
        }

        private bool _isAddEventEnabled = true;
        public bool IsAddEventEnabled
        {
            get { return _isAddEventEnabled; }
            set { _isAddEventEnabled = value; }
        }

        public static readonly DependencyProperty ClassForNewItemsProperty = DependencyProperty.Register("ClassForNewItems", typeof(ViewItemClass), typeof(TasksOrEventsListHeader), null);
        
        public ViewItemClass ClassForNewItems
        {
            get { return GetValue(ClassForNewItemsProperty) as ViewItemClass; }
            set { SetValue(ClassForNewItemsProperty, value); }
        }

        public bool IsClassPickerHidden { get; set; }

        public bool IsDatePickerHidden { get; set; }
    }
}
