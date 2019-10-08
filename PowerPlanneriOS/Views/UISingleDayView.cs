using System;
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

namespace PowerPlanneriOS.Views
{
    public class UISingleDayView : UIView
    {
        public event EventHandler<ViewItemClass> OnRequestViewClass;

        private UILabel _title;
        private CAShapeLayer _line;
        private UITableView _items;
        private object _tabBarHeightListener;
        private DateTime _today = DateTime.Today;
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

        private void _scheduleSnapshot_OnRequestViewClass(object sender, ViewItemClass e)
        {
            OnRequestViewClass?.Invoke(this, e);
        }

        private ViewItemSemester _semester;
        /// <summary>
        /// Set this once after constructing
        /// </summary>
        public MainScreenViewModel MainScreenViewModel
        {
            set
            {
                _tableViewSource = new TableViewSource(_items, value);
                _items.Source = _tableViewSource;
                _semester = value.CurrentSemester;
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

            _tableViewSource.Items = HomeworksOnDay.Get(SemesterItems.Items, Date);

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

        private string GetHeaderText(DateTime date)
        {
            if (date.Date == _today)
                return PowerPlannerResources.GetRelativeDateToday();

            else if (date.Date == _today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow();

            else if (date.Date == _today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday();

            return date.ToString("dddd, MMM d");
        }

        public override void LayoutSubviews()
        {
            var titleSize = _title.SizeThatFits(this.Frame.Size);

            nfloat y = 16;

            _title.Frame = new CGRect(
                x: 16,
                y: y,
                width: this.Frame.Width,
                height: titleSize.Height);

            y += titleSize.Height + 12;

            _line.Path = CGPath.FromRect(new CGRect(0, y - 0.5, this.Frame.Width, 0.5f));

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
            private MainScreenViewModel _viewModel;

            private const string CELL_ID_ITEM = "Item";

            public TableViewSource(UITableView tableView, MainScreenViewModel viewModel)
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

                        _tableView.ReloadData();
                    }
                }
            }

            public void ScrollToItem(BaseViewItemHomeworkExam item)
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

                if (item is BaseViewItemHomeworkExam)
                {
                    _viewModel.ShowItem(item as BaseViewItemHomeworkExam);

                    // Immediately unselect it
                    _tableView.SelectRow(null, true, UITableViewScrollPosition.None);
                }
            }
        }
    }
}