using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.Extensions;
using ToolsPortable;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlanneriOS.Views
{
    public class UIDayScheduleSnapshot : UIView
    {
        private static readonly float TIME_INDICATOR_SIZE = 60;
        private static readonly float GAP_SIZE = 2;
        private static readonly float HEIGHT_OF_HOUR = TIME_INDICATOR_SIZE + GAP_SIZE;

        public event EventHandler<ViewItemClass> OnRequestViewClass;

        private UIStackView _stackViewHolidays;
        private BareUIStackViewItemsSourceAdapter<UIMainCalendarItemView> _holidaysItemsSourceAdapter;
        private UIView _timetable;
        private UIView _scheduleTimesColumn;
        private UIView _scheduleItemsColumn;
        private UIView _scheduleGapLines;

        public UIDayScheduleSnapshot()
        {
            var background = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.FromWhiteAlpha(239 / 255f, 1)
            };
            {
                var paddingContainer = new UIControl()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                {
                    _stackViewHolidays = new UIStackView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Axis = UILayoutConstraintAxis.Vertical,
                        Spacing = 1
                    };
                    paddingContainer.Add(_stackViewHolidays);
                    _stackViewHolidays.StretchWidth(paddingContainer);

                    _timetable = new UIView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    {
                        _scheduleGapLines = new UIView()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false
                        };
                        _timetable.Add(_scheduleGapLines);
                        _scheduleGapLines.StretchWidthAndHeight(_timetable);

                        _scheduleTimesColumn = new UIView()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false
                        };
                        _timetable.Add(_scheduleTimesColumn);
                        _scheduleTimesColumn.StretchHeight(_timetable);
                        _scheduleTimesColumn.SetWidth(TIME_INDICATOR_SIZE);
                        _scheduleTimesColumn.PinToLeft(_timetable);

                        var verticalDivider = new UIView()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            BackgroundColor = UIColor.White
                        };
                        _timetable.Add(verticalDivider);
                        verticalDivider.StretchHeight(_timetable);
                        verticalDivider.SetWidth(GAP_SIZE);
                        verticalDivider.PinToLeft(_timetable, left: (int)TIME_INDICATOR_SIZE);

                        _scheduleItemsColumn = new UIView()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false
                        };
                        _timetable.Add(_scheduleItemsColumn);
                        _scheduleItemsColumn.StretchHeight(_timetable);
                        _scheduleItemsColumn.StretchWidth(_timetable, left: TIME_INDICATOR_SIZE + GAP_SIZE + 8, right: 8);

                        // Normally we would have used constraints to lay these out horizontally, but for some reason constraints
                        // are acting up and the horizontal constraints weren't working correctly, so just pinning things to the left
                        // and applying correct padding
                        // Maybe it was because I originally forgot TranslatesAutoresizing on the verticalDivider... that would explain it
                    }
                    paddingContainer.Add(_timetable);
                    _timetable.StretchWidth(paddingContainer);

                    _holidaysItemsSourceAdapter = new BareUIStackViewItemsSourceAdapter<UIMainCalendarItemView>(_stackViewHolidays);

                    paddingContainer.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[holidays][timetable]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                        "holidays", _stackViewHolidays,
                        "timetable", _timetable));
                }
                background.Add(paddingContainer);
                paddingContainer.StretchWidthAndHeight(background);
            }
            this.Add(background);
            background.StretchWidthAndHeight(this, top: 16, bottom: 16);
        }

        public SemesterItemsViewGroup SemesterItems { get; private set; }
        public DateTime Date { get; private set; }

        private HolidaysOnDay _currHolidays;
        private DayScheduleItemsArranger _arrangedItems;
        private EventHandler _arrangedItemsOnItemsChangedHandler;
        private NotifyCollectionChangedEventHandler _currHolidaysChangedHandler;
        private int _request = 0;
        public async void Initialize(SemesterItemsViewGroup semesterItems, DateTime date)
        {
            SemesterItems = semesterItems;
            Date = date;

            try
            {
                _request++;
                var currRequest = _request;
                await semesterItems.LoadingTask;
                if (currRequest != _request)
                {
                    // Another initialize happened while loading, so stop here on this old request
                    // (No concern about int overflow since it wraps by default)
                    return;
                }

                if (_currHolidays != null && _currHolidaysChangedHandler != null)
                {
                    _currHolidays.CollectionChanged -= _currHolidaysChangedHandler;
                    _currHolidays = null;
                }

                _currHolidays = HolidaysOnDay.Create(semesterItems.Items, Date);
                _currHolidaysChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(_currHolidays_CollectionChanged).Handler;
                _currHolidays.CollectionChanged += _currHolidaysChangedHandler;

                _holidaysItemsSourceAdapter.ItemsSource = _currHolidays;
                UpdateHolidaysBehavior();

                if (_arrangedItemsOnItemsChangedHandler == null)
                {
                    _arrangedItemsOnItemsChangedHandler = new WeakEventHandler<EventArgs>(_arrangedItems_OnItemsChanged).Handler;
                }
                else if (_arrangedItems != null)
                {
                    _arrangedItems.OnItemsChanged -= _arrangedItemsOnItemsChangedHandler;
                }

                _arrangedItems = DayScheduleItemsArranger.Create(PowerPlannerApp.Current.GetCurrentAccount(), semesterItems, PowerPlannerApp.Current.GetMainScreenViewModel().ScheduleViewItemsGroup, Date, HEIGHT_OF_HOUR, UIScheduleViewEventItemCollapsed.SPACING_WITH_NO_ADDITIONAL, UIScheduleViewEventItemCollapsed.SPACING_WITH_ADDITIONAL, UIScheduleViewEventItemCollapsed.WIDTH_OF_COLLAPSED_ITEM, includeTasksAndEventsAndHolidays: true);
                _arrangedItems.OnItemsChanged += _arrangedItemsOnItemsChangedHandler;

                render();
            }
            catch (Exception ex)
            {
                // There might have been a data error loading the main data, don't want to crash because of that
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private float _timetableHeight = 0;
        private void render()
        {
            _scheduleGapLines.ClearAllSubviews();
            _scheduleItemsColumn.ClearAllSubviews();
            _scheduleTimesColumn.ClearAllSubviews();
            _timetable.SetHeight(0);
            _timetableHeight = 0;

            if (Date == DateTime.MinValue || !SemesterItems.Semester.IsDateDuringThisSemester(Date))
            {
                UpdateVisibility();
                UpdateTotalHeight();
                return;
            }

            if (!_arrangedItems.IsValid())
            {
                UpdateVisibility();
                UpdateTotalHeight();
                return;
            }

            base.Hidden = false;

            float totalHeight = ((int)(_arrangedItems.EndTime - _arrangedItems.StartTime).TotalHours + 1) * HEIGHT_OF_HOUR;
            _timetable.SetHeight(totalHeight);
            _timetableHeight = totalHeight;

            UpdateTotalHeight();

            for (TimeSpan time = _arrangedItems.StartTime; time <= _arrangedItems.EndTime; time = time.Add(TimeSpan.FromHours(1)))
            {
                string text = DateTime.Today.Add(time).ToString("h:").TrimEnd(':');

                var label = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredHeadline,
                    Lines = 1,
                    Text = text,
                    TextAlignment = UITextAlignment.Center
                };
                _scheduleTimesColumn.Add(label);
                label.StretchWidth(_scheduleTimesColumn);
                label.SetHeight(HEIGHT_OF_HOUR);
                label.PinToTop(_scheduleTimesColumn, GetTopMarginAsPx(time, _arrangedItems.StartTime));

                // Add the divider if not the first
                if (time != _arrangedItems.StartTime)
                {
                    var gap = new UIView()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        BackgroundColor = UIColor.White
                    };
                    _scheduleGapLines.Add(gap);
                    gap.StretchWidth(_scheduleGapLines);
                    gap.SetHeight(GAP_SIZE);
                    gap.PinToTop(_scheduleGapLines, GetTopMarginAsPx(time, _arrangedItems.StartTime));
                }
            }

            foreach (var s in _arrangedItems.ScheduleItems)
            {
                UIScheduleItemView visual = new UIScheduleItemView(s.Item);
                visual.TouchUpInside += new WeakEventHandler(ScheduleItem_TouchUpInside).Handler;

                AddVisualItem(visual, s);
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in _arrangedItems.EventItems.Reverse())
            {
                UIView visual;
                if (e.IsCollapsedMode)
                {
                    visual = new UIScheduleViewEventItemCollapsed()
                    {
                        Item = e
                    };
                }
                else
                {
                    visual = new UIScheduleViewEventItemFull()
                    {
                        Item = e
                    };
                }

                AddVisualItem(visual, e);
            }
        }

        private void ScheduleItem_TouchUpInside(object sender, EventArgs e)
        {
            var scheduleItem = sender as UIScheduleItemView;

            OnRequestViewClass?.Invoke(this, scheduleItem.Item.Class);
        }

        private float GetTopMarginAsPx(TimeSpan itemTime, TimeSpan baseTime)
        {
            return (float)Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0);
        }

        private void AddVisualItem(UIView visual, DayScheduleItemsArranger.BaseScheduleItem item)
        {
            UIScheduleView.AddVisualItem(visual, item, _scheduleItemsColumn, colPadding: 0);

            visual.PinToTop(_scheduleItemsColumn, (int)item.TopOffset);
        }

        private void _arrangedItems_OnItemsChanged(object sender, EventArgs e)
        {
            try
            {
                render();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdateHolidaysBehavior()
        {
            if (_currHolidays != null && _currHolidays.Count > 0)
            {
                _timetable.Alpha = 0.5f;
            }
            else
            {
                _timetable.Alpha = 1;
            }
        }

        private void _currHolidays_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateHolidaysBehavior();
                UpdateVisibility();
                UpdateTotalHeight();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdateTotalHeight()
        {
            nfloat totalHeight;

            if (_stackViewHolidays.ArrangedSubviews.Length > 0)
            {
                int count = _stackViewHolidays.ArrangedSubviews.Length;
                totalHeight = _timetableHeight + UIMainCalendarItemView.GetHeight() * count + count - 1; // plus count - 1 for the spacing between items
            }
            else
            {
                totalHeight = _timetableHeight;
            }

            if (totalHeight > 0)
            {
                totalHeight += 32; // Compensate for the padding we add outside
            }

            this.Frame = new CoreGraphics.CGRect(
                x: this.Frame.X,
                y: this.Frame.Y,
                width: this.Frame.Width,
                height: totalHeight);
            if (this.Superview is UITableView)
            {
                // To make the table view actually refresh, we have to set this
                (this.Superview as UITableView).TableFooterView = this;
            }
        }

        private void UpdateVisibility()
        {
            if (_scheduleTimesColumn.Subviews.Length == 0 && (_currHolidays == null || _currHolidays.Count == 0))
            {
                this.Hidden = false;
            }
            else
            {
                this.Hidden = true;
            }
        }
    }
}