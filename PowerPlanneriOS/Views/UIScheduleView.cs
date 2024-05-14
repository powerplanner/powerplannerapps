using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.App;
using CoreGraphics;
using PowerPlanneriOS.Controllers;
using System.ComponentModel;
using InterfacesiOS.Helpers;
using Vx.Extensions;

namespace PowerPlanneriOS.Views
{
    public class UIScheduleView : UIScrollView
    {
        public ScheduleViewModel ViewModel { get; private set; }

        private UIView _mainView;
        private UIView _timesColumn;
        private UIView _contentColumn;
        private UIView _daysRow;
        private UIView _allDayItemsRow;
        private UIView _scheduleRow;

        private UILabel[] _dayLabels = new UILabel[7];
        private UIView[] _scheduleColumns = new UIView[7];
        private BareUIStackViewItemsSourceAdapter[] _allDayItemSourceAdapters = new BareUIStackViewItemsSourceAdapter[7];

        private const int COL_WIDTH = 200;
        private const int COL_PADDING = 8;
        private const int INITIAL_MARGIN = 6;
        private const int HEIGHT_OF_HOUR = 80;
        private object _tabBarHeightListener;

        public UIScheduleView(ScheduleViewModel viewModel)
        {
            this.MaximumZoomScale = 1f;
            this.MinimumZoomScale = 0.3f;
            this.ViewForZoomingInScrollView += delegate { return _mainView; };
            this.BackgroundColor = UIColorCompat.SystemBackgroundColor;

            ViewModel = viewModel;
            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            _mainView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            this.Add(_mainView);
            _mainView.ConfigureForMultiDirectionScrolling(this);

            _timesColumn = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColorCompat.SystemGray4Color
            };
            _mainView.Add(_timesColumn);
            _timesColumn.StretchHeight(_mainView);

            _contentColumn = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _mainView.Add(_contentColumn);
            _contentColumn.StretchHeight(_mainView);
            _mainView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|[timesColumn][contentColumn]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                "timesColumn", _timesColumn,
                "contentColumn", _contentColumn)));

            // Days row
            _daysRow = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColorCompat.SystemGray4Color
            };
            _contentColumn.Add(_daysRow);
            _daysRow.StretchWidth(_contentColumn);

            // All day items row
            _allDayItemsRow = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _contentColumn.Add(_allDayItemsRow);
            _allDayItemsRow.StretchWidth(_contentColumn);

            // Schedule row
            _scheduleRow = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _contentColumn.Add(_scheduleRow);
            _scheduleRow.StretchWidth(_contentColumn);

            //_daysRow.PinToTop(_contentColumn);
            //_scheduleRow.StretchHeight(_contentColumn);
            _contentColumn.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|[daysRow][allDayItemsRow][scheduleRow]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                "daysRow", _daysRow,
                "allDayItemsRow", _allDayItemsRow,
                "scheduleRow", _scheduleRow)));

            // Make scheduleRow be the big one, allDayItemsRow shrink
            _scheduleRow.SetContentCompressionResistancePriority(501, UILayoutConstraintAxis.Vertical);
            _allDayItemsRow.SetContentCompressionResistancePriority(499, UILayoutConstraintAxis.Vertical);

            // Populate days columns
            UIView prevDayLabel = null;
            UIView prevAllDayItems = null;
            UIView prevScheduleColumn = null;
            for (int i = 0; i < 7; i++)
            {
                // Day header
                var dayLabel = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredSubheadline,
                    Lines = 1
                };
                _daysRow.Add(dayLabel);
                if (prevDayLabel == null)
                {
                    dayLabel.PinToLeft(_daysRow, COL_PADDING);
                }
                else
                {
                    dayLabel.SetToRightOf(prevDayLabel, _daysRow, COL_PADDING);
                }
                dayLabel.SetWidth(COL_WIDTH - COL_PADDING);
                dayLabel.StretchHeight(_daysRow, top: 3, bottom: 3);
                _dayLabels[i] = dayLabel;
                prevDayLabel = dayLabel;

                // All day items
                var allDayItemsStackView = new UIStackView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Axis = UILayoutConstraintAxis.Vertical,
                    Spacing = 1
                };
                {
                    var itemsSource = new BareUIStackViewItemsSourceAdapter(allDayItemsStackView, CreateAllDayItemView);
                    _allDayItemSourceAdapters[i] = itemsSource;
                }
                UIView allDayItemsView = allDayItemsStackView;
                // Have to place the stack view in a view since background doesn't render on stack view itself
                allDayItemsView = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = UIColorCompat.SecondarySystemBackgroundColor
                };
                allDayItemsView.Add(allDayItemsStackView);
                allDayItemsStackView.StretchWidthAndHeight(allDayItemsView, right: 1);
                if (i % 2 != 0)
                {
                    // Alternate light and dark
                    allDayItemsView.BackgroundColor = UIColorCompat.SecondarySystemBackgroundColor;
                }
                else
                {
                    allDayItemsView.BackgroundColor = UIColorCompat.SystemBackgroundColor;
                }
                _allDayItemsRow.Add(allDayItemsView);
                if (prevAllDayItems == null)
                {
                    allDayItemsView.PinToLeft(_allDayItemsRow);
                }
                else
                {
                    allDayItemsView.SetToRightOf(prevAllDayItems, _allDayItemsRow);
                }
                allDayItemsView.SetWidth(COL_WIDTH);
                allDayItemsView.StretchHeight(_allDayItemsRow);
                prevAllDayItems = allDayItemsView;

                // Schedule column
                var scheduleColumn = new UIControl()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                if (i % 2 != 0)
                {
                    // Alternate light and dark
                    scheduleColumn.BackgroundColor = UIColorCompat.SecondarySystemBackgroundColor;
                }
                _scheduleRow.Add(scheduleColumn);
                if (prevScheduleColumn == null)
                {
                    scheduleColumn.PinToLeft(_scheduleRow);
                }
                else
                {
                    scheduleColumn.SetToRightOf(prevScheduleColumn, _scheduleRow);
                }
                scheduleColumn.SetWidth(COL_WIDTH);
                scheduleColumn.StretchHeight(_scheduleRow);
                _scheduleColumns[i] = scheduleColumn;
                prevScheduleColumn = scheduleColumn;
            }

            ViewModel.InitializeArrangers(HEIGHT_OF_HOUR, UIScheduleViewEventItemCollapsed.SPACING_WITH_NO_ADDITIONAL, UIScheduleViewEventItemCollapsed.SPACING_WITH_ADDITIONAL, UIScheduleViewEventItemCollapsed.WIDTH_OF_COLLAPSED_ITEM);
            ViewModel.OnFullReset += new WeakEventHandler<EventArgs>(ViewModel_OnFullReset).Handler;
            ViewModel.OnItemsForDateChanged += new WeakEventHandler<DateTime>(ViewModel_OnItemsForDateChanged).Handler;

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                // Plus 44 for the toolbar that also appears
                this.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT + 44, 0);
            });

            UpdateWidth();

            Render();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CountOfDaysToDisplay):
                    UpdateWidth();
                    break;
            }
        }

        private UIView CreateAllDayItemView(object obj)
        {
            // Last item is a blank view that'll act as the "fill remaining space" so the previous item doesn't stretch its height
            if (obj is string)
            {
                return new UIView();
            }

            var view = new UIMainCalendarItemView()
            {
                DataContext = obj
            };

            return view;
        }

        private void ViewModel_OnItemsForDateChanged(object sender, DateTime e)
        {
            RenderDate(e);
        }

        private void ViewModel_OnFullReset(object sender, EventArgs e)
        {
            Render();
        }

        private void Render()
        {
            // Populate days
            DayOfWeek dayOfWeek = ViewModel.FirstDayOfWeek;
            for (int i = 0; i < 7; i++, dayOfWeek++)
            {
                _dayLabels[i].Text = DateTools.ToLocalizedString(dayOfWeek);
            }

            // Get earliest start and end date
            DateTime today = DateTime.Today;
            DateTime classStartTime = today.Add(ViewModel.StartTime);
            DateTime classEndTime = today.Add(ViewModel.EndTime);

            // Remove existing times
            foreach (var time in _timesColumn.Subviews)
            {
                time.RemoveFromSuperview();
            }

            // Fill in the times on the left column
            for (DateTime tempClassStartTime = classStartTime; classEndTime >= tempClassStartTime; tempClassStartTime = tempClassStartTime.AddHours(1))
            {
                var time = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredSubheadline,
                    Lines = 1,
                    Text = DateTimeFormatterExtension.Current.FormatAsShortTimeWithoutAmPm(tempClassStartTime)
                };
                _timesColumn.Add(time);
                time.StretchWidth(_timesColumn, left: 5, right: 5);
                PinTopRelativeToStart(time, GetTopMarginAsPx(tempClassStartTime.TimeOfDay, classStartTime.TimeOfDay));
            }

            _scheduleRow.SetHeight(INITIAL_MARGIN + ((classEndTime - classStartTime).Hours + 1) * HEIGHT_OF_HOUR + 55);

            RenderAllDates();
        }

        private void UpdateWidth()
        {
            _contentColumn.SetWidth(COL_WIDTH * ViewModel.CountOfDaysToDisplay);
        }

        private void RenderAllDates()
        {
            for (int i = 0; i < 7; i++)
            {
                RenderDate(ViewModel.StartDate.AddDays(i));
            }
        }

        private void RenderDate(DateTime date)
        {
            var col = _scheduleColumns[GetColumn(date.DayOfWeek)];

            // Clear all items
            foreach (var item in col.Subviews)
            {
                item.RemoveFromSuperview();
            }

            // Get arranger for the date
            var arranger = ViewModel.Items[date.DayOfWeek];
            int colIndex = GetColumn(date.DayOfWeek);

            // Assign all day items
            List<object> holidayAndAllDayItemsWithBlankBottomView = new List<object>(arranger.HolidayAndAllDayItems);
            holidayAndAllDayItemsWithBlankBottomView.Add("BottomView");
            _allDayItemSourceAdapters[colIndex].ItemsSource = holidayAndAllDayItemsWithBlankBottomView;

            foreach (var s in arranger.ScheduleItems)
            {
                UIControl scheduleItem = new UIScheduleItemView(s.Item, arranger.Date);
                scheduleItem.TouchUpInside += new WeakEventHandler(ScheduleItem_TouchUpInside).Handler;

                AddVisualItem(scheduleItem, s, date.DayOfWeek);
            }

            // If Saturday or Sunday and no items on day, hide it
            if (!arranger.HolidayAndAllDayItems.Any() && !arranger.EventItems.Any() && !arranger.ScheduleItems.Any() && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
            {
                col.SetWidth(0);
                _allDayItemsRow.Subviews[colIndex].SetWidth(0);
                var dayLabel = _dayLabels[colIndex];
                dayLabel.SetWidth(0);
                if (colIndex <= _dayLabels.Length - 2)
                {
                    // Remove padding from the next item to compensate for the padding that we are keeping around
                    _dayLabels[colIndex + 1].SetToRightOf(dayLabel, _daysRow, 0);
                }
                return;
            }
            else
            {
                col.SetWidth(COL_WIDTH);
                _allDayItemsRow.Subviews[colIndex].SetWidth(COL_WIDTH);
                var dayLabel = _dayLabels[colIndex];
                dayLabel.SetWidth(COL_WIDTH - COL_PADDING);
                if (colIndex <= _dayLabels.Length - 2)
                {
                    // Re-add padding
                    _dayLabels[colIndex + 1].SetToRightOf(dayLabel, _daysRow, COL_PADDING);
                }
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in arranger.EventItems.Reverse())
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

                AddVisualItem(visual, e, arranger.Date.DayOfWeek);
            }

            if (arranger.HasHolidays)
            {
                var overlay = new UIView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = UIColor.FromRGBA(228 / 255f, 0, 137 / 225f, 0.3f)
                };
                col.Add(overlay);
                overlay.StretchWidth(col, right: 1);
                overlay.StretchHeight(col);
            }

            if (arranger.IsDifferentSemester)
            {
                var diffSemesterOverlay = new DifferentSemesterOverlayControl(
                    topPadding: 40,
                    leftPadding: COL_PADDING,
                    rightPadding: COL_PADDING)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                col.Add(diffSemesterOverlay);
                diffSemesterOverlay.StretchWidth(col);
                diffSemesterOverlay.StretchHeight(col);
            }

            //_mainView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|[timesColumn][contentColumn({COL_WIDTH * 7})]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
            //    "timesColumn", _timesColumn,
            //    "contentColumn", _contentColumn)));
        }

        private void ScheduleItem_TouchUpInside(object sender, EventArgs e)
        {
            var scheduleItem = sender as UIScheduleItemView;

            if (ViewModel.LayoutMode != ScheduleViewModel.LayoutModes.Normal)
            {
                ViewModel.EditTimes(scheduleItem.Item);
            }

            else
            {
                ViewModel.ViewClass(scheduleItem.Item.Class);
            }
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            UIView hitView = base.HitTest(point, uievent);

            foreach (var e in FindAllEventVisuals())
            {
                if (!e.Descendants().Contains(hitView))
                {
                    e.HideFull();
                }
            }

            return hitView;
        }

        private IEnumerable<UIScheduleViewEventItem> FindAllEventVisuals()
        {
            return this.Descendants().OfType<UIScheduleViewEventItem>();
        }

        private void AddVisualItem(UIView visual, DayScheduleItemsArranger.BaseScheduleItem item, DayOfWeek day)
        {
            var col = _scheduleColumns[GetColumn(day)];

            AddVisualItem(visual, item, col, COL_PADDING);

            PinTopRelativeToStart(visual, item.TopOffset + INITIAL_MARGIN);
        }

        public static void AddVisualItem(UIView visual, DayScheduleItemsArranger.BaseScheduleItem item, UIView addTo, int colPadding)
        {
            visual.TranslatesAutoresizingMaskIntoConstraints = false;

            addTo.Add(visual);

            int leftStart = (int)item.LeftOffset + colPadding;

            if (item.NumOfColumns > 1)
            {
                const float spacingBetweenConflicting = 4;
                float spacingOffset = spacingBetweenConflicting / item.NumOfColumns;

                try
                {
                    if (item.Column == 0)
                    {
                        visual.PinToLeft(addTo, left: leftStart);
                    }
                    else
                    {
                        addTo.AddConstraint(NSLayoutConstraint.Create(
                            visual,
                            NSLayoutAttribute.Leading,
                            NSLayoutRelation.Equal,
                            addTo,
                            NSLayoutAttribute.Trailing, // Trailing == width of column
                            multiplier: (float)item.Column / item.NumOfColumns,
                            constant: spacingBetweenConflicting * (item.Column - 1) + spacingOffset));
                    }

                    if (item.Column == item.NumOfColumns - 1)
                    {
                        visual.PinToRight(addTo, right: colPadding);
                    }
                    else
                    {
                        addTo.AddConstraint(NSLayoutConstraint.Create(
                            visual,
                            NSLayoutAttribute.Trailing,
                            NSLayoutRelation.Equal,
                            addTo,
                            NSLayoutAttribute.Trailing,
                            multiplier: (float)(item.Column + 1) / item.NumOfColumns,
                            constant: spacingBetweenConflicting * -1 * (item.NumOfColumns - item.Column - 2) - spacingOffset));
                    }

                    // Note that this results in middle items being wider than left/right, but the case where there's three items is a niche case anyways
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            else
            {
                visual.StretchWidth(addTo, left: leftStart, right: colPadding);
            }

            if (item is DayScheduleItemsArranger.ScheduleItem)
            {
                visual.SetHeight((int)item.Height);
            }
        }

        private void PinTopRelativeToStart(UIView view, double topOffset)
        {
            _mainView.AddConstraint(NSLayoutConstraint.Create(
                view,
                NSLayoutAttribute.Top,
                NSLayoutRelation.Equal,
                _allDayItemsRow,
                NSLayoutAttribute.Bottom,
                1,
                (float)topOffset));
        }

        private bool HasItemsOnDay(DayOfWeek dayOfWeek)
        {
            return _scheduleColumns[GetColumn(dayOfWeek)].Subviews.Any();
        }

        private int GetColumn(DayOfWeek dayOfWeek)
        {
            int answer = (int)dayOfWeek - (int)ViewModel.FirstDayOfWeek;
            if (answer < 0)
            {
                answer = answer + 7;
            }
            return answer;
        }

        private double GetTopMarginAsPx(TimeSpan itemTime, TimeSpan baseTime)
        {
            return INITIAL_MARGIN + Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0);
        }
    }
}