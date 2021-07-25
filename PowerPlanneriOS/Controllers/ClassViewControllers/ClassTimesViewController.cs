using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using Vx.Extensions;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassTimesViewController : BareMvvmUIViewControllerWithScrolling<ClassTimesViewModel>
    {
        public ClassTimesViewController()
        {
            Title = "Times";

            StackView.Spacing = 16;
        }

        protected override int TopPadding => 16;
        protected override int LeftPadding => 16;
        protected override int RightPadding => 16;

        private BareUIStackViewItemsSourceAdapter<UIGroupedTimeView> _itemsSourceAdapter;
        public override void OnViewModelLoadedOverride()
        {
            _itemsSourceAdapter = new BareUIStackViewItemsSourceAdapter<UIGroupedTimeView>(StackView)
            {
                ItemsSource = ViewModel.TimesGroupedByDay
            };

            base.OnViewModelLoadedOverride();
        }

        private class UIGroupedTimeView : BareUIView
        {
            public ClassTimesViewModel.GroupedDay Group
            {
                get { return DataContext as ClassTimesViewModel.GroupedDay; }
            }

            private BareUIStackViewItemsSourceAdapter<UITimeView> _itemsSourceAdapter;
            public UIGroupedTimeView()
            {
                base.TranslatesAutoresizingMaskIntoConstraints = false;

                BindingHost.SetVisibilityBinding(this, nameof(Group.IsVisible));

                var stackView = new UIStackView()
                {
                    Axis = UILayoutConstraintAxis.Vertical,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                var labelDayOfWeek = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredSubheadline
                };
                BindingHost.SetLabelTextBinding(labelDayOfWeek, nameof(Group.DayOfWeek));
                stackView.AddArrangedSubview(labelDayOfWeek);
                labelDayOfWeek.StretchWidth(stackView);

                var stackViewTimes = new UIStackView()
                {
                    Axis = UILayoutConstraintAxis.Vertical,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Spacing = 8
                };
                _itemsSourceAdapter = new BareUIStackViewItemsSourceAdapter<UITimeView>(stackViewTimes);
                stackView.AddArrangedSubview(stackViewTimes);
                stackViewTimes.StretchWidth(stackView);

                base.AddSubview(stackView);
                stackView.StretchWidthAndHeight(this);
            }

            protected override void OnDataContextChanged()
            {
                _itemsSourceAdapter.ItemsSource = Group?.Times;

                base.OnDataContextChanged();
            }

            private class UITimeView : BareUIView
            {
                public ViewItemSchedule Schedule => DataContext as ViewItemSchedule;

                private UIStackView _stackView;
                private UILabel _labelTime;
                private UITextView _textViewRoom;

                public UITimeView()
                {
                    base.TranslatesAutoresizingMaskIntoConstraints = false;

                    _stackView = new UIStackView()
                    {
                        Axis = UILayoutConstraintAxis.Vertical,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };

                    _labelTime = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = UIFont.PreferredCaption1
                    };
                    _stackView.AddArrangedSubview(_labelTime);
                    _labelTime.StretchWidth(_stackView);

                    _textViewRoom = new UITextView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        BackgroundColor = UIColor.Clear,
                        Font = UIFont.PreferredCaption1,
                        Editable = false,
                        ScrollEnabled = false,

                        // Link detection: http://iosdevelopertips.com/user-interface/creating-clickable-hyperlinks-from-a-url-phone-number-or-address.html
                        DataDetectorTypes = UIDataDetectorType.All
                    };

                    // Lose the padding: https://stackoverflow.com/questions/746670/how-to-lose-margin-padding-in-uitextview
                    _textViewRoom.TextContainerInset = UIEdgeInsets.Zero;
                    _textViewRoom.TextContainer.LineFragmentPadding = 0;

                    _stackView.AddArrangedSubview(_textViewRoom);
                    _textViewRoom.StretchWidth(_stackView);

                    base.AddSubview(_stackView);
                    _stackView.StretchWidthAndHeight(this);
                }

                protected override void OnDataContextChanged()
                {
                    if (Schedule != null)
                    {
                        _labelTime.Text = PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(Schedule.StartTime), DateTimeFormatterExtension.Current.FormatAsShortTime(Schedule.EndTime));

                        if (string.IsNullOrWhiteSpace(Schedule.Room))
                        {
                            if (_stackView.ArrangedSubviews.Contains(_textViewRoom))
                            {
                                _stackView.ActuallyRemoveArrangedSubview(_textViewRoom);
                            }
                        }
                        else
                        {
                            if (!_stackView.ArrangedSubviews.Contains(_textViewRoom))
                            {
                                _stackView.AddArrangedSubview(_textViewRoom);
                            }

                            _textViewRoom.Text = Schedule.Room.Trim();
                        }
                    }

                    base.OnDataContextChanged();
                }
            }
        }
    }
}