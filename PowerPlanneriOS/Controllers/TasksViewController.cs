using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.ViewModelPresenters;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Tasks;
using InterfacesiOS.Views;
using PowerPlanneriOS.Helpers;
using BareMvvm.Core.ViewModels;
using ToolsPortable;
using System.ComponentModel;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlanneriOS.Views;

namespace PowerPlanneriOS.Controllers
{
    public class TasksViewController : PagedViewModelPresenter
    {
        public new TasksViewModel ViewModel
        {
            get { return base.ViewModel as TasksViewModel; }
            set { base.ViewModel = value; }
        }

        protected UINavigationBar NavBar { get; private set; }
        public UINavigationItem NavItem { get; private set; }

        private UIControl _viewSelectedItemAndArrow;
        private UILabel _labelSelectedItem;
        private UIImageView _imageSelectedItemDownArrow;

        public TasksViewController()
        {
            MyNavigationController.View.TranslatesAutoresizingMaskIntoConstraints = false;
            MyNavigationController.View.StretchWidth(base.View);

            var statusBar = UIStatusBarView.CreateAndAddTo(View);

            NavBar = new UINavigationBar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ColorResources.ConfigureNavBar(NavBar);
            base.View.Add(NavBar);
            NavBar.StretchWidth(base.View);

            NavItem = new UINavigationItem();
            NavBar.Items = new UINavigationItem[] { NavItem };

            _viewSelectedItemAndArrow = new UIControl();
            _labelSelectedItem = new UILabel()
            {
                TextColor = UIColor.White
            };
            _viewSelectedItemAndArrow.Add(_labelSelectedItem);
            _imageSelectedItemDownArrow = new UIImageView(UIImage.FromBundle("ToolbarDown").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate))
            {
                TintColor = UIColor.White,
                ContentMode = UIViewContentMode.ScaleAspectFit
            };
            _viewSelectedItemAndArrow.Add(_imageSelectedItemDownArrow);
            _viewSelectedItemAndArrow.TouchUpInside += new WeakEventHandler(ButtonSwitchView_Clicked).Handler;

            NavItem.Title = "";
            NavItem.LeftBarButtonItem = new UIBarButtonItem(_viewSelectedItemAndArrow);

            NavItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add)
            {
                Title = "Add item"
            };
            NavItem.RightBarButtonItem.Clicked += new WeakEventHandler<EventArgs>(ButtonAddItem_Clicked).Handler;

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[statusBar][navBar][mainView]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "statusBar", statusBar,
                "navBar", NavBar,
                "mainView", MyNavigationController.View));
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
        }

        protected override void OnViewModelChanged(PagedViewModel oldViewModel, PagedViewModel currentViewModel)
        {
            if (currentViewModel != null)
            {
                currentViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(CurrentViewModel_PropertyChanged).Handler;
                UpdateTitle();
            }

            base.OnViewModelChanged(oldViewModel, currentViewModel);
        }

        private void CurrentViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedItem):
                    UpdateTitle();
                    break;
            }
        }

        private void UpdateTitle()
        {
            var title = GetTitle();
            Title = title;
            _labelSelectedItem.Text = title;

            nfloat imageWidth = 20;
            nfloat spacing = 8;
            nfloat height = 44;

            _labelSelectedItem.SizeToFit();
            var labelFrame = _labelSelectedItem.Frame;
            labelFrame.Height = height;
            _labelSelectedItem.Frame = labelFrame;

            _imageSelectedItemDownArrow.Frame = new CoreGraphics.CGRect(
                _labelSelectedItem.Frame.Width + spacing,
                0,
                imageWidth,
                height);

            _viewSelectedItemAndArrow.Frame = new CoreGraphics.CGRect(
                0,
                0,
                _labelSelectedItem.Frame.Width + spacing + imageWidth,
                height);
        }

        private string GetTitle()
        {
            switch (ViewModel.SelectedItem)
            {
                case NavigationManager.MainMenuSelections.Agenda:
                    return "Agenda";

                case NavigationManager.MainMenuSelections.Calendar:
                    return "Calendar";

                case NavigationManager.MainMenuSelections.Day:
                    return "Day";

                default:
                    throw new NotImplementedException();
            }
        }

        private void ButtonSwitchView_Clicked(object sender, EventArgs e)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            if (ViewModel.SelectedItem != PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Calendar)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create("Calendar View", UIAlertActionStyle.Default, delegate { ViewModel.FindAncestor<MainScreenViewModel>().SelectedItem = NavigationManager.MainMenuSelections.Calendar; }));
            }
            if (ViewModel.SelectedItem != PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Day)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create("Day View", UIAlertActionStyle.Default, delegate { ViewModel.FindAncestor<MainScreenViewModel>().SelectedItem = NavigationManager.MainMenuSelections.Day; }));
            }
            if (ViewModel.SelectedItem != PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Agenda)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create("Agenda View", UIAlertActionStyle.Default, delegate { ViewModel.FindAncestor<MainScreenViewModel>().SelectedItem = NavigationManager.MainMenuSelections.Agenda; }));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = NavItem.LeftBarButtonItem;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            this.PresentViewController(actionSheetAlert, true, null);
        }

        private void ButtonAddItem_Clicked(object sender, EventArgs e)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            actionSheetAlert.AddAction(UIAlertAction.Create("Add Task", UIAlertActionStyle.Default, delegate { AddTask(); }));
            actionSheetAlert.AddAction(UIAlertAction.Create("Add Event", UIAlertActionStyle.Default, delegate { AddEvent(); }));

            if (ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Calendar || ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Day)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create("Add Holiday", UIAlertActionStyle.Default, delegate { AddHoliday(); }));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = NavItem.RightBarButtonItem;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            this.PresentViewController(actionSheetAlert, true, null);
        }

        private void AddTask()
        {
            if (ViewModel.Content is CalendarViewModel)
            {
                (ViewModel.Content as CalendarViewModel).AddHomework();
            }
            else if (ViewModel.Content is DayViewModel)
            {
                (ViewModel.Content as DayViewModel).AddHomework();
            }
            else if (ViewModel.Content is AgendaViewModel)
            {
                (ViewModel.Content as AgendaViewModel).AddHomework();
            }
        }

        private void AddEvent()
        {
            if (ViewModel.Content is CalendarViewModel)
            {
                (ViewModel.Content as CalendarViewModel).AddExam();
            }
            else if (ViewModel.Content is DayViewModel)
            {
                (ViewModel.Content as DayViewModel).AddExam();
            }
            else if (ViewModel.Content is AgendaViewModel)
            {
                (ViewModel.Content as AgendaViewModel).AddExam();
            }
        }

        private void AddHoliday()
        {
            if (ViewModel.Content is CalendarViewModel)
            {
                (ViewModel.Content as CalendarViewModel).AddHoliday();
            }
            else if (ViewModel.Content is DayViewModel)
            {
                (ViewModel.Content as DayViewModel).AddHoliday();
            }
        }
    }
}