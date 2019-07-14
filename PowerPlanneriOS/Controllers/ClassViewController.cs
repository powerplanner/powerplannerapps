using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Binding;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Views;
using PowerPlanneriOS.Controllers.ClassViewControllers;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlanneriOS.Helpers;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlanneriOS.Controllers
{
    public class ClassViewController : PopupViewController<ClassViewModel>
    {
        private BindingHost _classBindingHost = new BindingHost();

        private ClassDetailsViewController _detailsViewController;
        private ClassTimesViewController _timesViewController;
        private ClassHomeworkOrExamsViewController _homeworkViewController;
        private ClassHomeworkOrExamsViewController _examsViewController;
        private ClassGradesViewController _gradesViewController;
        private CAPSPageMenuController _pageMenuController;

        private UIBarButtonItem _navButtonEditDetails;
        private UIBarButtonItem _navButtonEditTimes;
        private UIBarButtonItem _navButtonAddHomework;
        private UIBarButtonItem _navButtonAddExam;
        private UIBarButtonItem _navButtonAddGrade;

        public override void OnViewModelLoadedOverride()
        {
            _navButtonEditDetails = new UIBarButtonItem(UIBarButtonSystemItem.Edit)
            {
                Title = "Edit details"
            };
            _navButtonEditDetails.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.EditClassWithDetails(); }).Handler;

            _navButtonEditTimes = new UIBarButtonItem(UIBarButtonSystemItem.Edit)
            {
                Title = "Edit times"
            };
            _navButtonEditTimes.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.EditTimes(); }).Handler;

            _navButtonAddHomework = new UIBarButtonItem(UIBarButtonSystemItem.Add)
            {
                Title = "Add task"
            };
            _navButtonAddHomework.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.HomeworkViewModel.Add(); }).Handler;

            _navButtonAddExam = new UIBarButtonItem(UIBarButtonSystemItem.Add)
            {
                Title = "Add event"
            };
            _navButtonAddExam.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.ExamsViewModel.Add(); }).Handler;

            _navButtonAddGrade = new UIBarButtonItem(UIBarButtonSystemItem.Add)
            {
                Title = "Add grade"
            };
            _navButtonAddGrade.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.GradesViewModel.Add(); }).Handler;

            _detailsViewController = new ClassDetailsViewController()
            {
                ViewModel = ViewModel.DetailsViewModel
            };
            _timesViewController = new ClassTimesViewController()
            {
                ViewModel = ViewModel.TimesViewModel
            };
            _homeworkViewController = new ClassHomeworkOrExamsViewController()
            {
                Title = "Tasks",
                ViewModel = ViewModel.HomeworkViewModel
            };
            _examsViewController = new ClassHomeworkOrExamsViewController()
            {
                Title = "Events",
                ViewModel = ViewModel.ExamsViewModel
            };
            _gradesViewController = new ClassGradesViewController()
            {
                ViewModel = ViewModel.GradesViewModel
            };

            _pageMenuController = new CAPSPageMenuController(new UIViewController[]
            {
                _detailsViewController,
                _timesViewController,
                _homeworkViewController,
                _examsViewController,
                _gradesViewController
            }, new CAPSPageMenuConfiguration()
            {
                SelectedMenuItemLabelColor = ColorResources.PowerPlannerAccentBlue,
                SelectionIndicatorColor = ColorResources.PowerPlannerAccentBlue
            });
            _pageMenuController.SelectionChanged += new WeakEventHandler<CAPSPageMenuSelectionChangedEventArgs>(_pageMenuController_SelectionChanged).Handler;
            _pageMenuController.View.TranslatesAutoresizingMaskIntoConstraints = false;
            AddChildViewController(_pageMenuController);
            ContentView.Add(_pageMenuController.View);
            _pageMenuController.View.StretchWidthAndHeight(ContentView);
            UpdatePage(_detailsViewController);

            _classBindingHost.BindingObject = ViewModel.ViewItemsGroupClass.Class;
            _classBindingHost.SetBinding<string>(nameof(ViewItemClass.Name), (name) =>
            {
                Title = name;
            });

            BackButtonText = "Back";

            base.OnViewModelLoadedOverride();
        }

        protected override void OnViewReturnedTo()
        {
            base.OnViewReturnedTo();

            try
            {
                if (IsViewModelLoaded)
                {
                    var changedItem = ViewModel.GetLastChangedTaskOrEvent();
                    if (changedItem != null)
                    {
                        // Homework
                        if (_pageMenuController.CurrentPageIndex == 2)
                        {
                            _homeworkViewController.ScrollToItem(changedItem);
                        }

                        // Exam
                        else if (_pageMenuController.CurrentPageIndex == 3)
                        {
                            _examsViewController.ScrollToItem(changedItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public override void DidMoveToParentViewController(UIViewController parent)
        {
            if (parent == null && _pageMenuController != null)
            {
                _pageMenuController.WillMoveToParentViewController(null);
                _pageMenuController.View.RemoveFromSuperview();
                _pageMenuController.RemoveFromParentViewController();
                _pageMenuController.DidMoveToParentViewController(null);
                _pageMenuController = null;
            }

            base.DidMoveToParentViewController(parent);
        }

        private void _pageMenuController_SelectionChanged(object sender, CAPSPageMenuSelectionChangedEventArgs e)
        {
            UpdatePage(e.Controller);
        }

        private void UpdatePage(UIViewController currentController)
        {
            if (currentController == _detailsViewController)
            {
                NavItem.RightBarButtonItems = new UIBarButtonItem[] { _navButtonEditDetails };
            }
            else if (currentController == _timesViewController)
            {
                NavItem.RightBarButtonItems = new UIBarButtonItem[] { _navButtonEditTimes };
            }
            else if (currentController == _homeworkViewController)
            {
                NavItem.RightBarButtonItems = new UIBarButtonItem[] { _navButtonAddHomework };
                ViewModel.ViewItemsGroupClass.LoadHomeworkAndExams();
            }
            else if (currentController == _examsViewController)
            {
                NavItem.RightBarButtonItems = new UIBarButtonItem[] { _navButtonAddExam };
                ViewModel.ViewItemsGroupClass.LoadHomeworkAndExams();

            }
            else if (currentController == _gradesViewController)
            {
                NavItem.RightBarButtonItems = new UIBarButtonItem[] { _navButtonAddGrade };
                ViewModel.ViewItemsGroupClass.LoadGrades();
            }
        }

        protected override void BackButtonClicked()
        {
            if (ViewModel.MainScreenViewModel.GetMostRecentFromBackStack() != null)
            {
                ViewModel.MainScreenViewModel.GoBack();
            }
            else
            {
                ViewModel.MainScreenViewModel.SelectClassWithinSemester(null);
            }
        }
    }
}