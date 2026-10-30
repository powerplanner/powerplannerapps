using Foundation;
using InterfacesiOS.Binding;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlanneriOS.Controllers.ClassViewControllers;
using PowerPlanneriOS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using UIKit;
using Vx.iOS;
using Vx.Views;

namespace PowerPlanneriOS.Controllers
{
    public class ClassViewController : PopupViewController<ClassViewModel>
    {
        private BindingHost _classBindingHost = new BindingHost();

        private ClassDetailsViewController _detailsViewController;
        private ClassTimesViewController _timesViewController;
        private ClassTasksOrEventsViewController _tasksViewController;
        private ClassTasksOrEventsViewController _eventsViewController;
        private ClassGradesViewController _gradesViewController;
        private CAPSPageMenuController _pageMenuController;

        private UIBarButtonItem _navButtonEditDetails;
        private UIBarButtonItem _navButtonEditTimes;

        public ClassViewController()
        {
            // Revert toolbar height to default, since this is NOT rendered as a popup on iOS
            Toolbar.ToolbarHeight = Toolbar.TOOLBAR_HEIGHT;
        }

        public override void OnViewModelLoadedOverride()
        {
            _navButtonEditDetails = new UIBarButtonItem
            {
                Title = PowerPlannerResources.GetString("AppBarButtonEdit.Label")
            };
            _navButtonEditDetails.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.EditClassWithDetails(); }).Handler;

            _navButtonEditTimes = new UIBarButtonItem
            {
                Title = PowerPlannerResources.GetString("AppBarButtonEdit.Label")
            };
            _navButtonEditTimes.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.EditTimes(); }).Handler;

            _detailsViewController = new ClassDetailsViewController()
            {
                ViewModel = ViewModel.DetailsViewModel
            };
            _timesViewController = new ClassTimesViewController()
            {
                ViewModel = ViewModel.TimesViewModel
            };
            _tasksViewController = new ClassTasksOrEventsViewController()
            {
                Title = PowerPlannerResources.GetString("ClassPage_PivotItemTasks.Header"),
                ViewModel = ViewModel.TasksViewModel
            };
            _eventsViewController = new ClassTasksOrEventsViewController()
            {
                Title = PowerPlannerResources.GetString("ClassPage_PivotItemEvents.Header"),
                ViewModel = ViewModel.EventsViewModel
            };
            _gradesViewController = new ClassGradesViewController()
            {
                ViewModel = ViewModel.GradesViewModel
            };

            int initialPageIndex = ViewModel.InitialPage != null ? (int)ViewModel.InitialPage : 0;
            initialPageIndex--; // Subtract one since don't have the overview view
            if (initialPageIndex < 0)
            {
                initialPageIndex = 0;
            }
            _pageMenuController = new CAPSPageMenuController(new UIViewController[]
            {
                _detailsViewController,
                _timesViewController,
                _tasksViewController,
                _eventsViewController,
                _gradesViewController
            }, new CAPSPageMenuConfiguration()
            {
                SelectedMenuItemLabelColor = ColorResources.PowerPlannerAccentBlue,
                SelectionIndicatorColor = ColorResources.PowerPlannerAccentBlue,
                ViewBackgroundColor = Vx.Views.Theme.Current.BackgroundColor.ToUI(),
                ScrollMenuBackgroundColor = Vx.Views.Theme.Current.BackgroundAlt1Color.ToUI(),
                UnselectedMenuItemLabelColor = Vx.Views.Theme.Current.SubtleForegroundColor.ToUI()
            }, initialPageIndex);
            _pageMenuController.View.TranslatesAutoresizingMaskIntoConstraints = false;
            AddChildViewController(_pageMenuController);
            ContentView.Add(_pageMenuController.View);
            _pageMenuController.View.StretchWidthAndHeight(ContentView);
            switch (ViewModel.InitialPage)
            {
                case ClassViewModel.ClassPages.Times:
                    UpdatePage(_timesViewController);
                    break;

                case ClassViewModel.ClassPages.Tasks:
                    UpdatePage(_tasksViewController);
                    break;

                case ClassViewModel.ClassPages.Events:
                    UpdatePage(_eventsViewController);
                    break;

                case ClassViewModel.ClassPages.Grades:
                    UpdatePage(_gradesViewController);
                    break;

                default:
                    UpdatePage(_detailsViewController);
                    break;
            }
            _pageMenuController.SelectionChanged += new WeakEventHandler<CAPSPageMenuSelectionChangedEventArgs>(_pageMenuController_SelectionChanged).Handler;

            _classBindingHost.DataContext = ViewModel.ViewItemsGroupClass.Class;
            _classBindingHost.SetBinding<ViewItemClass, string>(nameof(ViewItemClass.Name), item => item.Name, (name) =>
            {
                Title = name;
            });

            BackButtonText = PowerPlannerResources.GetString("String_Back");

            base.OnViewModelLoadedOverride();
        }

        private Thickness _originalNookInsets;
        public override Thickness NookInsets
        {
            get
            {
                if (ViewModel?.MainScreenViewModel != null)
                {
                    return ViewModel.MainScreenViewModel.ContentNookInsets;
                }
                return _originalNookInsets;
            }
            protected set => _originalNookInsets = value;
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
                        // Task
                        if (_pageMenuController.CurrentPageIndex == 2)
                        {
                            //_tasksViewController.ScrollToItem(changedItem);
                        }

                        // Event
                        else if (_pageMenuController.CurrentPageIndex == 3)
                        {
                            //_eventsViewController.ScrollToItem(changedItem);
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
            var editClass = new Vx.Views.MenuItem
            {
                Text = R.S("String_EditClass"),
                Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                Click = ViewModel.EditClassWithDetails
            };

            var deleteClass = new Vx.Views.MenuItem
            {
                Text = R.S("String_DeleteClass"),
                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                Click = async () =>
                {
                    if (await PowerPlannerApp.ConfirmDeleteAsync(PowerPlannerResources.GetString("String_ConfirmDeleteClassMessage"), PowerPlannerResources.GetString("String_ConfirmDeleteClassHeader"), useConfirmationCheckbox: true))
                    {
                        ViewModel.DeleteClass();
                    }
                }
            };

            if (currentController == _detailsViewController)
            {
                ViewModel.CurrentPage = ClassViewModel.ClassPages.Details;
                Toolbar.PrimaryCommands = new List<Vx.Views.MenuItem>
                {
                    editClass
                };
                Toolbar.SecondaryCommands = new List<Vx.Views.MenuItem>
                {
                    deleteClass
                };
            }
            else if (currentController == _timesViewController)
            {
                ViewModel.CurrentPage = ClassViewModel.ClassPages.Times;
                Toolbar.PrimaryCommands = new List<Vx.Views.MenuItem>
                {
                    new Vx.Views.MenuItem
                    {
                        Text = R.S("String_EditTimes"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                        Click = ViewModel.EditTimes
                    }
                };
                Toolbar.SecondaryCommands = new List<Vx.Views.MenuItem>
                {
                    editClass,
                    deleteClass
                };
            }
            else if (currentController == _tasksViewController)
            {
                ViewModel.CurrentPage = ClassViewModel.ClassPages.Tasks;
                ViewModel.ViewItemsGroupClass.LoadTasksAndEvents();
            }
            else if (currentController == _eventsViewController)
            {
                ViewModel.CurrentPage = ClassViewModel.ClassPages.Events;
                ViewModel.ViewItemsGroupClass.LoadTasksAndEvents();

            }
            else if (currentController == _gradesViewController)
            {
                ViewModel.CurrentPage = ClassViewModel.ClassPages.Grades;
                ViewModel.ViewItemsGroupClass.LoadGrades();
            }

            if (currentController == _tasksViewController
                || currentController == _eventsViewController
                || currentController == _gradesViewController)
            {
                Toolbar.PrimaryCommands = new();
                Toolbar.SecondaryCommands = new List<Vx.Views.MenuItem>
                {
                    editClass,
                    deleteClass
                };
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