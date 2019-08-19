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

namespace PowerPlanneriOS.Controllers
{
    public class ScheduleViewController : PopupViewController<ScheduleViewModel>
    {
        private UIBarButtonItem _editButton;
        private UIBarButtonItem _doneButton;

        public ScheduleViewController()
        {
            Title = "Schedule";
            HideBackButton();

            _editButton = new UIBarButtonItem(UIBarButtonSystemItem.Edit)
            {
                Title = "Edit schedule"
            };
            _editButton.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.EnterEditMode(); }).Handler;
            NavItem.RightBarButtonItem = _editButton;

            _doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)
            {
                Title = "Done"
            };
            _doneButton.Clicked += new WeakEventHandler<EventArgs>(delegate { ViewModel.ExitEditMode(); }).Handler;

            // Add a dummy scroll view that will get the auto content inset behavior
            ContentView.Add(new UIScrollView());
        }

        private UILabel _labelDateRange;
        private UIView _labelYearAndWeekContainer;
        private UILabel _labelYearAndWeek;
        public override void OnViewModelAndViewLoadedOverride()
        {
            base.OnViewModelAndViewLoadedOverride();

            var scheduleView = new UIScheduleView(ViewModel)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(scheduleView);
            scheduleView.StretchWidthAndHeight(ContentView);

            var toolbar = new UIToolbar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
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
            BindingHost.SetBindings(new string[]
            {
                nameof(ViewModel.DisplayStartDate),
                nameof(ViewModel.DisplayEndDate),
                nameof(ViewModel.CurrentWeek),
                nameof(ViewModel.HasTwoWeekSchedule)
            }, UpdateToolbarLabels);
            toolbar.Items = new UIBarButtonItem[]
            {
                new UIBarButtonItem(_labelDateRange),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace)
                {
                    Width = 6
                },
                new UIBarButtonItem(_labelYearAndWeekContainer),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIImage.FromBundle("ToolbarBack"), UIBarButtonItemStyle.Plain, new WeakEventHandler(delegate { ViewModel.PreviousWeek(); }).Handler),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace)
                {
                    Width = 20
                },
                new UIBarButtonItem(UIImage.FromBundle("ToolbarForward"), UIBarButtonItemStyle.Plain, new WeakEventHandler(delegate { ViewModel.NextWeek(); }).Handler)
            };
            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                toolbar.RemovePinToBottom(ContentView).PinToBottom(ContentView, (int)MainScreenViewController.TAB_BAR_HEIGHT);
            });

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            UpdateLayoutMode();
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
                Title = "Schedule";
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
                        BackgroundColor = UIColor.White
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
                Title = "Edit Schedule";
            }
            else
            {
                if (_welcomeView == null)
                {
                    _welcomeView = new UIView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        BackgroundColor = UIColor.White
                    };
                    {
                        // New user
                        var topSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                        _welcomeView.Add(topSpacer);
                        topSpacer.StretchWidth(_welcomeView);

                        var bottomSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                        _welcomeView.Add(bottomSpacer);
                        bottomSpacer.StretchWidth(_welcomeView);

                        var title = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Text = "Welcome to Power Planner!",
                            AdjustsFontSizeToFitWidth = true,
                            Font = UIFont.PreferredTitle3,
                            TextAlignment = UITextAlignment.Center
                        };
                        _welcomeView.Add(title);
                        title.StretchWidth(_welcomeView, left: 16, right: 16);

                        var subtitle = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Text = "To get started, add your classes and schedule!",
                            Lines = 0,
                            Font = UIFont.PreferredCaption1,
                            TextAlignment = UITextAlignment.Center
                        };
                        _welcomeView.Add(subtitle);
                        subtitle.StretchWidth(_welcomeView, left: 16, right: 16);

                        var buttonAddClass = new UIButton(UIButtonType.System)
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false
                        };
                        buttonAddClass.SetTitle("Add Class", UIControlState.Normal);
                        buttonAddClass.SetTitleColor(new UIColor(1, 1), UIControlState.Normal);
                        buttonAddClass.BackgroundColor = ColorResources.PowerPlannerAccentBlue;
                        buttonAddClass.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.AddClass(); }).Handler;
                        _welcomeView.Add(buttonAddClass);
                        buttonAddClass.StretchWidth(_welcomeView, left: 16, right: 16);
                        buttonAddClass.SetMaxWidth(250);

                        _welcomeView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[topSpacer(==bottomSpacer)][title]-8-[subtitle]-16-[buttonAddClass][bottomSpacer]-32-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                            "topSpacer", topSpacer,
                            "title", title,
                            "subtitle", subtitle,
                            "buttonAddClass", buttonAddClass,
                            "bottomSpacer", bottomSpacer));

                        // Returning user (we add on top so it remains clickable
                        if (ViewModel.IsReturningUserVisible)
                        {
                            var returningUser = new UILabel()
                            {
                                TranslatesAutoresizingMaskIntoConstraints = false,
                                Text = PowerPlannerResources.GetString("SchedulePage_TextBlockReturningUser.Text"),
                                Font = UIFont.PreferredCaption1
                            };
                            _welcomeView.Add(returningUser);
                            returningUser.StretchWidth(_welcomeView, left: 16, right: 16);

                            var buttonLogIn = PowerPlannerUIHelper.CreatePowerPlannerBlueButton("Log In");
                            buttonLogIn.TranslatesAutoresizingMaskIntoConstraints = false;
                            buttonLogIn.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.LogIn(); }).Handler;
                            _welcomeView.Add(buttonLogIn);
                            buttonLogIn.PinToLeft(_welcomeView, left: 16);

                            _welcomeView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-16-[returningUser]-8-[buttonLogIn]", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                                "returningUser", returningUser,
                                "buttonLogIn", buttonLogIn));
                        }
                    }
                }

                // Display the welcome view on top of everything, including on top of the nav bar
                ContentView.Add(_welcomeView);
                _welcomeView.StretchWidth(ContentView);

                NavItem.RightBarButtonItem = null;
                Title = "Edit Schedule";

                MainScreenViewController.ListenToTabBarHeightChanged(ref _welcomeViewTabBarHeightListener, delegate
                {
                    _welcomeView.StretchHeight(ContentView, top: 0, bottom: (float)MainScreenViewController.TAB_BAR_HEIGHT);
                });
            }
        }
    }
}