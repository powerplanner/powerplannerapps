using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using ToolsPortable;
using System.ComponentModel;
using PowerPlanneriOS.Views;
using InterfacesiOS.Views;
using System.Globalization;
using PowerPlannerAppDataLibrary;
using PowerPlanneriOS.Helpers;
using InterfacesiOS.Helpers;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public class ScheduleViewController : PopupViewController<ScheduleViewModel>
    {
        private UIBarButtonItem _editButton;
        private UIBarButtonItem _doneButton;
        private UIToolbar _toolbar;
        private UIBarButtonItem[] _toolbarItems;

        public ScheduleViewController()
        {
            Title = PowerPlannerResources.GetString("MainMenuItem_Schedule");
            HideBackButton();

            _editButton = new UIBarButtonItem
            {
                Title = PowerPlannerResources.GetString("AppBarButtonEdit.Label")
            };
            // Owned by this same controller (field), so a direct handler is safe: no external publisher could keep this alive longer than needed.
            _editButton.Clicked += (sender, e) => ViewModel.EnterEditMode();
            NavItem.RightBarButtonItem = _editButton;

            _doneButton = new UIBarButtonItem
            {
                Title = PowerPlannerResources.GetString("String_Done")
            };
            _doneButton.Clicked += (sender, e) => ViewModel.ExitEditMode();

            // Add a dummy scroll view that will get the auto content inset behavior
            ContentView.Add(new UIScrollView());
        }

        private UILabel _labelDateRange;
        private UIView _labelYearAndWeekContainer;
        private UILabel _labelYearAndWeek;
        private UIScheduleView _scheduleView;
        public override void OnViewModelAndViewLoadedOverride()
        {
            base.OnViewModelAndViewLoadedOverride();

            // Must be kept in a field: ViewModel only holds WeakEventHandler subscriptions to it, so
            // without a strong reference here it can be garbage collected and silently stop updating.
            _scheduleView = new UIScheduleView(ViewModel)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(_scheduleView);
            _scheduleView.StretchWidthAndHeight(ContentView);

            // Must be kept in a field: otherwise nothing roots the toolbar (or its UIBarButtonItems)
            // managed wrapper, so it can be garbage collected and its Clicked handlers stop firing
            // even though the native button remains visible on screen.
            _toolbar = new UIToolbar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            var toolbar = _toolbar;
            ContentView.Add(toolbar);
            toolbar.StretchWidth(ContentView);
            toolbar.SetHeight(44);
            _labelDateRange = new UILabel()
            {
                BackgroundColor = UIColor.Clear,
                Font = UIFont.PreferredCallout
            };
            _labelYearAndWeekContainer = new UIView();
            _labelYearAndWeek = new UILabel()
            {
                Font = UIFont.PreferredCaption1,
                TextColor = UIColor.Gray,
                BackgroundColor = UIColor.Clear
            };
            _labelYearAndWeekContainer.Add(_labelYearAndWeek);
            BindingHost.SetBindings<ScheduleViewModel>(new string[]
            {
                nameof(ViewModel.DisplayStartDate),
                nameof(ViewModel.DisplayEndDate),
                nameof(ViewModel.CurrentWeek),
                nameof(ViewModel.HasTwoWeekSchedule)
            }, UpdateToolbarLabels);
            _toolbarItems = new UIBarButtonItem[]
            {
                new UIBarButtonItem(_labelDateRange),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace)
                {
                    Width = 6
                },
                new UIBarButtonItem(_labelYearAndWeekContainer),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                // Owned by this same controller (_toolbarItems field), so a direct handler is safe here.
                new UIBarButtonItem(UIImage.FromBundle("ToolbarBack"), UIBarButtonItemStyle.Plain, (sender, e) => PreviousWeekClicked()),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace)
                {
                    Width = 20
                },
                new UIBarButtonItem(UIImage.FromBundle("ToolbarForward"), UIBarButtonItemStyle.Plain, (sender, e) => NextWeekClicked())
            };
            toolbar.Items = _toolbarItems;
            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                toolbar.RemovePinToBottom(ContentView).PinToBottom(ContentView, (int)MainScreenViewController.TAB_BAR_HEIGHT);
            });

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            UpdateLayoutMode();
        }

        private void PreviousWeekClicked()
        {
            try
            {
                ViewModel.PreviousWeek();
            }
            catch (Exception ex)
            {
                // Surface this instead of letting it vanish at the native callback boundary with no visible effect.
                System.Diagnostics.Debug.WriteLine("PreviousWeek failed: " + ex);
                ExceptionHelper.ReportHandledException(ex);
            }
        }

        private void NextWeekClicked()
        {
            try
            {
                ViewModel.NextWeek();
            }
            catch (Exception ex)
            {
                // Surface this instead of letting it vanish at the native callback boundary with no visible effect.
                System.Diagnostics.Debug.WriteLine("NextWeek failed: " + ex);
                ExceptionHelper.ReportHandledException(ex);
            }
        }

        private void UpdateToolbarLabels()
        {
            _labelDateRange.Text = $"{GetWeekText(ViewModel.DisplayStartDate)} - {GetWeekText(ViewModel.DisplayEndDate)}";

            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("en"))
            {
                _labelYearAndWeek.Text = $"{ViewModel.DisplayStartDate.ToString("yyyy")}";
                if (ViewModel.HasTwoWeekSchedule)
                {
                    _labelYearAndWeek.Text += ", " + PowerPlannerResources.GetLocalizedWeek(ViewModel.CurrentWeek);
                }
            }
            else
            {
                if (ViewModel.HasTwoWeekSchedule)
                {
                    _labelYearAndWeek.Text = PowerPlannerResources.GetLocalizedWeek(ViewModel.CurrentWeek);
                }
                else
                {
                    _labelYearAndWeek.Text = "";
                }
            }

            _labelDateRange.SizeToFit();
            _labelYearAndWeekContainer.Frame = new CoreGraphics.CGRect();
            _labelYearAndWeek.Frame = new CoreGraphics.CGRect();
            _labelYearAndWeek.SizeToFit();

            // The Years text is smaller and should be bottom-aligned
            nfloat topOffset = 0;
            if (_labelYearAndWeek.Frame.Height < _labelDateRange.Frame.Height)
            {
                topOffset = _labelDateRange.Frame.Height - _labelYearAndWeek.Frame.Height;
            }
            topOffset -= 1; // Minus 1 to make it optically aligned correctly

            _labelYearAndWeekContainer.Frame = new CoreGraphics.CGRect(
                _labelYearAndWeekContainer.Frame.X,
                _labelDateRange.Frame.Y,
                _labelYearAndWeek.Frame.Width,
                _labelDateRange.Frame.Height);

            _labelYearAndWeek.Frame = new CoreGraphics.CGRect(
                _labelYearAndWeek.Frame.X,
                _labelYearAndWeek.Frame.Y + topOffset,
                _labelYearAndWeek.Frame.Width,
                _labelYearAndWeek.Frame.Height);
        }

        private static string GetWeekText(DateTime date)
        {
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("en"))
            {
                return date.ToString("M/d");
            }
            else
            {
                return date.ToString("d");
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.LayoutMode):
                    UpdateLayoutMode();
                    break;
            }
        }

        private UIScrollView _editingView;
        private object _tabBarHeightListener;

        private UIView _welcomeView;
        private object _welcomeViewTabBarHeightListener;

        private ScheduleViewModel.LayoutModes? _prevLayoutMode;

        private void UpdateLayoutMode()
        {
            // Clean up from previous state
            if (_prevLayoutMode != null)
            {
                // If for some reason state didn't change, do nothing
                if (_prevLayoutMode.Value == ViewModel.LayoutMode)
                {
                    return;
                }

                // On iOS we make these the same, so ignore changes between them
                if ((_prevLayoutMode.Value == ScheduleViewModel.LayoutModes.FullEditing || _prevLayoutMode.Value == ScheduleViewModel.LayoutModes.SplitEditing)
                    && (ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.FullEditing || ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.SplitEditing))
                {
                    return;
                }

                switch (_prevLayoutMode.Value)
                {
                    case ScheduleViewModel.LayoutModes.SplitEditing:
                    case ScheduleViewModel.LayoutModes.FullEditing:
                        _editingView.RemoveFromSuperview();
                        break;

                    case ScheduleViewModel.LayoutModes.Welcome:
                        _welcomeView.RemoveFromSuperview();
                        break;
                }
            }

            _prevLayoutMode = ViewModel.LayoutMode;

            if (ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.Normal)
            {
                NavItem.RightBarButtonItem = _editButton;
                Title = PowerPlannerResources.GetString("MainMenuItem_Schedule");
            }
            else if (ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.SplitEditing
                || ViewModel.LayoutMode == ScheduleViewModel.LayoutModes.FullEditing)
            {
                if (_editingView == null)
                {
                    _editingView = new UIScrollView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        ShowsHorizontalScrollIndicator = false,
                        BackgroundColor = UIColorCompat.SystemBackgroundColor
                    };
                    var actualEditingView = new UIScheduleEditingView(ViewModel);
                    _editingView.Add(actualEditingView);
                    actualEditingView.ConfigureForVerticalScrolling(_editingView, top: 16, bottom: 16);

                    MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
                    {
                        _editingView.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
                    });
                }

                ContentView.Add(_editingView);
                _editingView.StretchWidthAndHeight(ContentView);

                NavItem.RightBarButtonItem = _doneButton;
                Title = PowerPlannerResources.GetString("String_EditSchedule");
            }
            else
            {
                if (_welcomeView == null)
                {
                    _welcomeView = new ScheduleWelcomeComponent()
                    {
                        ScheduleViewModel = ViewModel,
                        NookInsets = ViewModel.NookInsets
                    }.Render();
                    _welcomeView.TranslatesAutoresizingMaskIntoConstraints = false;
                }

                // Display the welcome view on top of everything, including on top of the nav bar
                ContentView.Add(_welcomeView);
                _welcomeView.StretchWidth(ContentView);

                NavItem.RightBarButtonItem = null;
                Title = PowerPlannerResources.GetString("String_EditSchedule");

                MainScreenViewController.ListenToTabBarHeightChanged(ref _welcomeViewTabBarHeightListener, delegate
                {
                    _welcomeView.StretchHeight(ContentView, top: 0, bottom: (float)MainScreenViewController.TAB_BAR_HEIGHT);
                });
            }
        }
    }
}