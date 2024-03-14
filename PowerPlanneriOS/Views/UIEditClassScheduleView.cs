using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using CoreAnimation;
using CoreGraphics;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesiOS.Helpers;
using Vx.Extensions;

namespace PowerPlanneriOS.Views
{
    public class UIEditClassScheduleView : BareUIView
    {
        public event EventHandler<ViewItemClass> OnRequestAddTime;
        public event EventHandler<ViewItemSchedule[]> OnRequestEditSchedules;
        public event EventHandler<ViewItemClass> OnRequestEditClass;

        private CAShapeLayer _circle;
        private UILabel _labelName;
        private BareUIStackViewItemsSourceAdapter<UIEditClassScheduleTimeView> _timesAdapter;

        public UIEditClassScheduleView()
        {
            base.BackgroundColor = UIColorCompat.SecondarySystemBackgroundColor;

            var headerView = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                const int CIRCLE_HEIGHT = 18;

                _circle = new CAShapeLayer();
                _circle.Path = CGPath.EllipseFromRect(new CGRect(8, 3, CIRCLE_HEIGHT, CIRCLE_HEIGHT));
                BindingHost.SetColorBinding(_circle, nameof(ViewItemClass.Color));
                headerView.Layer.AddSublayer(_circle);

                _labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredTitle3,
                    Lines = 1
                };
                BindingHost.SetLabelTextBinding(_labelName, nameof(ViewItemClass.Name));
                headerView.Add(_labelName);
                _labelName.StretchWidthAndHeight(headerView, left:  CIRCLE_HEIGHT + 8 + 8, right: 8);

                headerView.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { OnRequestEditClass(this, DataContext as ViewItemClass); }).Handler;
            }
            this.Add(headerView);
            headerView.StretchWidth(this);

            // Times
            var stackViewTimes = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Spacing = 8
            };
            {
                _timesAdapter = new BareUIStackViewItemsSourceAdapter<UIEditClassScheduleTimeView>(stackViewTimes);
                _timesAdapter.OnViewCreated += _timesAdapter_OnViewCreated;
            }
            this.Add(stackViewTimes);
            stackViewTimes.StretchWidth(this);

            // Button
            var buttonAdd = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            buttonAdd.SetTitle(PowerPlannerResources.GetString("String_AddTime"), UIControlState.Normal);
            buttonAdd.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { OnRequestAddTime?.Invoke(this, DataContext as ViewItemClass); }).Handler;
            this.Add(buttonAdd);
            buttonAdd.StretchWidth(this, left: 8, right: 8);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-8-[headerView]-8-[stackViewTimes]-8-[buttonAdd]-8-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                "headerView", headerView,
                "stackViewTimes", stackViewTimes,
                "buttonAdd", buttonAdd)));
        }

        private void _timesAdapter_OnViewCreated(object sender, UIEditClassScheduleTimeView e)
        {
            e.OnRequestEditTimes += new WeakEventHandler<ViewItemSchedule[]>(E_OnRequestEditTimes).Handler;
        }

        private void E_OnRequestEditTimes(object sender, ViewItemSchedule[] e)
        {
            OnRequestEditSchedules?.Invoke(this, e);
        }

        private NotifyCollectionChangedEventHandler _schedulesChangedHandler;
        private ViewItemClass _currClass;
        protected override void OnDataContextChanged()
        {
            base.OnDataContextChanged();

            if (_currClass == DataContext)
            {
                return;
            }

            // Unregister old
            if (_currClass != null)
            {
                _currClass.Schedules.CollectionChanged -= _schedulesChangedHandler;
            }

            // Register new
            _currClass = DataContext as ViewItemClass;
            if (_currClass != null)
            {
                if (_schedulesChangedHandler == null)
                {
                    _schedulesChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Schedules_CollectionChanged).Handler;
                }
                _currClass.Schedules.CollectionChanged += _schedulesChangedHandler;
            }

            UpdateGroupedSchedules();
        }

        private void Schedules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender != (DataContext as ViewItemClass)?.Schedules)
            {
                return;
            }
            UpdateGroupedSchedules();
        }

        public void UpdateGroupedSchedules()
        {
            var currClass = DataContext as ViewItemClass;
            if (currClass == null)
            {
                _timesAdapter.ItemsSource = null;
                return;
            }

            _timesAdapter.ItemsSource = currClass.GetSchedulesGroupedBySharedEditingValues();
        }
    }

    public class UIEditClassScheduleTimeView : BareUIView
    {
        public event EventHandler<ViewItemSchedule[]> OnRequestEditTimes;

        private UILabel _labelDayOfWeeks;
        private UILabel _labelTime;
        private UILabel _labelRoom;
        private UILabel _labelWeek;
        private BareUIVisibilityContainer _visibilityRoom;
        private BareUIVisibilityContainer _visibilityWeek;

        public UIEditClassScheduleTimeView()
        {
            _labelDayOfWeeks = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredHeadline
            };
            this.Add(_labelDayOfWeeks);
            _labelDayOfWeeks.StretchWidth(this, left: 8, right: 8);

            _labelTime = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1
            };
            this.Add(_labelTime);
            _labelTime.StretchWidth(this, left: 8, right: 8);

            _labelRoom = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1,
                TextColor = UIColorCompat.SecondaryLabelColor
            };
            _visibilityRoom = new BareUIVisibilityContainer()
            {
                Child = _labelRoom
            };
            this.Add(_visibilityRoom);
            _visibilityRoom.StretchWidth(this, left: 8, right: 8);

            _labelWeek = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1,
                TextColor = UIColorCompat.SecondaryLabelColor
            };
            _visibilityWeek = new BareUIVisibilityContainer()
            {
                Child = _labelWeek
            };
            this.Add(_visibilityWeek);
            _visibilityWeek.StretchWidth(this, left: 8, right: 8);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|[labelDayOfWeeks][labelTime][visibilityRoom][visibilityWeek]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                "labelDayOfWeeks", _labelDayOfWeeks,
                "labelTime", _labelTime,
                "visibilityRoom", _visibilityRoom,
                "visibilityWeek", _visibilityWeek)));

            var touchContainer = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            this.Add(touchContainer);
            touchContainer.StretchWidth(this);
            touchContainer.StretchHeight(this);
            touchContainer.TouchUpInside += new WeakEventHandler<EventArgs>(TouchContainer_TouchUpInside).Handler;
        }

        private void TouchContainer_TouchUpInside(object sender, EventArgs e)
        {
            if (OnRequestEditTimes != null && DataContext is IEnumerable<ViewItemSchedule>)
            {
                OnRequestEditTimes(this, (DataContext as IEnumerable<ViewItemSchedule>).ToArray());
            }
        }

        protected override void OnDataContextChanged()
        {
            IEnumerable<ViewItemSchedule> schedules = DataContext as IEnumerable<ViewItemSchedule>;
            if (schedules == null || !schedules.Any())
                return;

            ViewItemSchedule first = schedules.First();

            _labelDayOfWeeks.Text = string.Join(", ", schedules.Select(i => i.DayOfWeek).Distinct().OrderBy(i => i).Select(i => DateTools.ToLocalizedString(i)));
            // Editing view, so we use School Time
            _labelTime.Text = PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(first.StartTimeInSchoolTime), DateTimeFormatterExtension.Current.FormatAsShortTime(first.EndTimeInSchoolTime));
            if (string.IsNullOrWhiteSpace(first.Room))
            {
                _visibilityRoom.IsVisible = false;
            }
            else
            {
                _labelRoom.Text = first.Room;
                _visibilityRoom.IsVisible = true;
            }

            if (first.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks)
            {
                _visibilityWeek.IsVisible = false;
            }

            else
            {
                _labelWeek.Text = PowerPlannerResources.GetLocalizedWeek(first.ScheduleWeek);
                _visibilityWeek.IsVisible = false;
            }

            base.OnDataContextChanged();
        }
    }
}