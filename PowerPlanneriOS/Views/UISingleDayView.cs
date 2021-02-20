﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;
using CoreAnimation;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlanneriOS.Controllers;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using System.ComponentModel;

namespace PowerPlanneriOS.Views
{
    public class UISingleDayView : UIView
    {
        public event EventHandler<ViewItemClass> OnRequestViewClass;
        public event EventHandler OnRequestExpand;

        private UILabel _title;
        private UIButton _buttonExpand;
        private UILabel _nothingDueText;
        private CAShapeLayer _line;
        private UITableView _items;
        private object _tabBarHeightListener;
        public SemesterItemsViewGroup SemesterItems { get; set; }
        private TableViewSource _tableViewSource;
        private UIDayScheduleSnapshot _scheduleSnapshot;

        public UISingleDayView()
        {
            _title = new UILabel()
            {
                Font = UIFont.PreferredBody
            };
            this.Add(_title);

            _buttonExpand = new UIButton(UIButtonType.System);
            _buttonExpand.SetImage(UIImage.FromBundle("ToolbarDown").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            nfloat radians180 = unchecked((nfloat)Math.PI); // 180 degrees is exactly PI in radians
            _buttonExpand.Transform = CGAffineTransform.MakeRotation(radians180);
            _buttonExpand.TouchUpInside += _buttonExpand_TouchUpInside;
            this.Add(_buttonExpand);

            _nothingDueText = new UILabel()
            {
                Font = UIFont.PreferredBody,
                Text = PowerPlannerResources.GetString("String_NothingDue"),
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.LightGray
            };
            this.Add(_nothingDueText);

            _line = new CAShapeLayer()
            {
                FillColor = UIColor.LightGray.CGColor
            };
            this.Layer.AddSublayer(_line);

            _items = new UITableView()
            {
                SeparatorInset = UIEdgeInsets.Zero
            };
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                // Stretch to full width even on iPad
                _items.CellLayoutMarginsFollowReadableWidth = false;
            }
            _scheduleSnapshot = new UIDayScheduleSnapshot();
            _scheduleSnapshot.OnRequestViewClass += new WeakEventHandler<ViewItemClass>(_scheduleSnapshot_OnRequestViewClass).Handler;
            _items.TableFooterView = _scheduleSnapshot;
            this.Add(_items);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _items.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });
        }

        private void _buttonExpand_TouchUpInside(object sender, EventArgs e)
        {
            OnRequestExpand?.Invoke(this, new EventArgs());
        }

        private void _scheduleSnapshot_OnRequestViewClass(object sender, ViewItemClass e)
        {
            OnRequestViewClass?.Invoke(this, e);
        }

        private CalendarViewModel _viewModel;
        private ViewItemSemester _semester;
        /// <summary>
        /// Set this once after constructing
        /// </summary>
        public CalendarViewModel CalendarViewModel
        {
            set
            {
                _viewModel = value;
                _tableViewSource = new TableViewSource(_items, value);
                _items.Source = _tableViewSource;
                _semester = value.MainScreenViewModel.CurrentSemester;

                value.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
                _tableViewSource.HasItemsChanged += _tableViewSource_HasItemsChanged;

                UpdateShowHeader();
            }
        }

        private void _tableViewSource_HasItemsChanged(object sender, bool e)
        {
            SetNeedsLayout();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_viewModel.DisplayState):
                    UpdateShowHeader();
                    break;
            }
        }

        private void UpdateShowHeader()
        {
            ShowHeader = _viewModel.DisplayState != CalendarViewModel.DisplayStates.Day;
        }

        private bool _showHeader = true;
        private bool ShowHeader
        {
            get => _showHeader;
            set
            {
                if (_showHeader != value)
                {
                    _showHeader = value;
                    SetNeedsLayout();
                }
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                value = value.Date;

                if (_date == value)
                {
                    return;
                }

                _date = value;
                OnDateChanged();
            }
        }

        private void OnDateChanged()
        {
            _title.Text = GetHeaderText(Date);

            _tableViewSource.Items = TasksOrEventsOnDay.Get(_viewModel.MainScreenViewModel.CurrentAccount, SemesterItems.Items, Date);

            _scheduleSnapshot.Initialize(SemesterItems, Date);

            // Make sure we always start at the top, otherwise past recycled views might be scrolled down further
            _items.SetContentOffset(new CGPoint(), animated: false);

            UpdateIsDifferentSemester();
        }

        private UIView _differentSemesterView;
        private void UpdateIsDifferentSemester()
        {
            if (_semester != null && !_semester.IsDateDuringThisSemester(Date))
            {
                if (_differentSemesterView == null)
                {
                    _differentSemesterView = new DifferentSemesterOverlayControl(int.MaxValue, 16, 16);
                    this.Add(_differentSemesterView);
                }
            }
            else
            {
                if (_differentSemesterView != null)
                {
                    _differentSemesterView.RemoveFromSuperview();
                    _differentSemesterView = null;
                }
            }
        }

        public static string GetHeaderText(DateTime date)
        {
            var today = DateTime.Today;

            if (date.Date == today)
                return PowerPlannerResources.GetRelativeDateToday();

            else if (date.Date == today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow();

            else if (date.Date == today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday();

            return PowerPlannerAppDataLibrary.Helpers.DateHelpers.ToMediumDateString(date).ToUpper();
        }

        public override void LayoutSubviews()
        {
            nfloat y = 0;

            if (ShowHeader)
            {
                y += 16;

                nfloat hPadding = 16;

                var buttonSize = _buttonExpand.SizeThatFits(this.Frame.Size);
                var titleSize = _title.SizeThatFits(this.Frame.Size);

                _title.Frame = new CGRect(
                    x: hPadding,
                    y: y,
                    width: -hPadding + this.Frame.Width - hPadding - buttonSize.Width - hPadding,
                    height: titleSize.Height);

                _buttonExpand.Frame = new CGRect(
                    x: this.Frame.Width - hPadding - buttonSize.Width,
                    y: y + (titleSize.Height - buttonSize.Height) / 2, // We want this vertically centered with the text
                    width: buttonSize.Width,
                    height: buttonSize.Height);

                _title.Hidden = false;
                _buttonExpand.Hidden = false;
                _line.Hidden = false;

                y += titleSize.Height + 12;

                _line.Path = CGPath.FromRect(new CGRect(0, y - 0.5, this.Frame.Width, 0.5f));
            }
            else
            {
                _title.Hidden = true;
                _buttonExpand.Hidden = true;
                _line.Hidden = true;
            }

            if (_tableViewSource != null && !_tableViewSource.HasItems)
            {
                y += 16;

                // Show nothing due text
                _nothingDueText.Hidden = false;

                var size = _nothingDueText.SizeThatFits(this.Frame.Size);

                _nothingDueText.Frame = new CGRect(
                    x: 0,
                    y: y,
                    width: this.Frame.Width,
                    height: size.Height);

                y += size.Height;
            }
            else
            {
                _nothingDueText.Hidden = true;
            }

            nfloat remainingHeight = this.Frame.Height - y;
            if (remainingHeight < 0)
            {
                remainingHeight = 0;
            }

            _items.Frame = new CGRect(
                x: 0,
                y: y,
                width: this.Frame.Width,
                height: remainingHeight);

            if (_differentSemesterView != null)
            {
                _differentSemesterView.Frame = new CGRect(
                    x: 0,
                    y: 0,
                    width: this.Frame.Width,
                    height: this.Frame.Height);
            }
        }

        private class TableViewSource : UITableViewSource
        {
            private UITableView _tableView;
            private CalendarViewModel _viewModel;

            public event EventHandler<bool> HasItemsChanged;

            private const string CELL_ID_ITEM = "Item";

            public TableViewSource(UITableView tableView, CalendarViewModel viewModel)
            {
                _tableView = tableView;
                _viewModel = viewModel;
            }

            private NotifyCollectionChangedEventHandler _itemsChangedHandler;
            private MyObservableList<BaseViewItemMegaItem> _items;
            public MyObservableList<BaseViewItemMegaItem> Items
            {
                set
                {
                    if (_items == value)
                    {
                        return;
                    }

                    if (_items != null && _itemsChangedHandler != null)
                    {
                        _items.CollectionChanged -= _itemsChangedHandler;
                    }

                    _items = value;

                    if (_items != null)
                    {
                        if (_itemsChangedHandler == null)
                        {
                            _itemsChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(TableViewSource_CollectionChanged).Handler;
                        }
                        _items.CollectionChanged += _itemsChangedHandler;

                        HasItems = value.Any();

                        _tableView.ReloadData();
                    }
                }
            }

            private bool _hasItems;
            public bool HasItems
            {
                get => _hasItems;
                set
                {
                    if (_hasItems != value)
                    {
                        _hasItems = value;
                        HasItemsChanged?.Invoke(this, value);
                    }
                }
            }

            public void ScrollToItem(ViewItemTaskOrEvent item)
            {
                //for (int i = 0; i < _viewModel.ItemsWithHeaders.Count; i++)
                //{
                //    if (_viewModel.ItemsWithHeaders[i] == item)
                //    {
                //        _tableView.ScrollToRow(NSIndexPath.FromRowSection(i, 0), UITableViewScrollPosition.None, true);
                //        return;
                //    }
                //}
            }

            private void TableViewSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                HasItems = _items != null && _items.Any();
                ReloadData();
            }

            private void ReloadData()
            {
                _tableView.ReloadData();
            }

            private string GetCellId(int row)
            {
                return CELL_ID_ITEM;
            }

            private object GetItem(int row)
            {
                return _items[row];
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                string cellId = GetCellId(indexPath.Row);

                BareUITableViewCell cell = tableView.DequeueReusableCell(cellId) as BareUITableViewCell;

                // If no cells to reuse, create a new one
                if (cell == null)
                {
                    switch (cellId)
                    {
                        case CELL_ID_ITEM:
                            cell = new UITaskCell(CELL_ID_ITEM);
                            break;
                    }
                }

                cell.DataContext = GetItem(indexPath.Row);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (_items == null)
                {
                    return 0;
                }

                return _items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                object item = GetItem(indexPath.Row);

                if (item is ViewItemTaskOrEvent taskOrEvent)
                {
                    _viewModel.ShowItem(taskOrEvent);

                    // Immediately unselect it
                    _tableView.SelectRow(null, true, UITableViewScrollPosition.None);
                }
            }
        }
    }
}