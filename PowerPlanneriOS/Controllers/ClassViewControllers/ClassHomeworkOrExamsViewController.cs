using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using System.Collections.Specialized;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlanneriOS.Views;
using System.ComponentModel;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassHomeworkOrExamsViewController : BareMvvmUIViewController<ClassHomeworkOrExamsViewModel>
    {
        private UITableView _tableView;
        private object _tabBarHeightListener;

        public ClassHomeworkOrExamsViewController()
        {
            _tableView = new UITableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                SeparatorInset = UIEdgeInsets.Zero
            };
            _tableView.TableFooterView = new UIView(); // Eliminate extra separators on bottom of view
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                // Stretch to full width even on iPad
                _tableView.CellLayoutMarginsFollowReadableWidth = false;
            }
            View.Add(_tableView);
            _tableView.StretchWidthAndHeight(View);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _tableView.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });
        }

        public override void OnViewModelLoadedOverride()
        {
            _tableView.Source = new TableViewSource(_tableView, ViewModel);

            base.OnViewModelLoadedOverride();
        }

        private class TableViewSource : UITableViewSource
        {
            private UITableView _tableView;
            private ClassHomeworkOrExamsViewModel _viewModel;

            private const string CELL_ID_ITEM = "Item";
            private const string CELL_ID_HEADER = "Header";
            private const string CELL_ID_BUTTON = "Button";

            public TableViewSource(UITableView tableView, ClassHomeworkOrExamsViewModel viewModel)
            {
                _tableView = tableView;
                _viewModel = viewModel;

                viewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
                (viewModel.ItemsWithHeaders as INotifyCollectionChanged).CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(TableViewSource_CollectionChanged).Handler;

                tableView.ReloadData();
            }

            public void ScrollToItem(BaseViewItemHomeworkExam item)
            {
                for (int i = 0; i < _viewModel.ItemsWithHeaders.Count; i++)
                {
                    if (_viewModel.ItemsWithHeaders[i] == item)
                    {
                        _tableView.ScrollToRow(NSIndexPath.FromRowSection(i, 0), UITableViewScrollPosition.None, true);
                        return;
                    }
                }
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(_viewModel.HasPastCompletedItems))
                {
                    ReloadData();
                }
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
                if (row < _viewModel.ItemsWithHeaders.Count)
                {
                    return _viewModel.ItemsWithHeaders[row] is DateTime ? CELL_ID_HEADER : CELL_ID_ITEM;
                }

                if (row == _viewModel.ItemsWithHeaders.Count)
                {
                    return CELL_ID_BUTTON;
                }

                // I need to actually load these first
                throw new NotImplementedException();
                //row = row - _viewModel.ItemsWithHeaders.Count - 1;

                //return _viewModel.PastCompletedItemsWithHeaders[row] is DateTime ? CELL_ID_HEADER : CELL_ID_ITEM;
            }

            private object GetItem(int row)
            {
                if (row < _viewModel.ItemsWithHeaders.Count)
                {
                    return _viewModel.ItemsWithHeaders[row];
                }

                if (row == _viewModel.ItemsWithHeaders.Count)
                {
                    return CELL_ID_BUTTON;
                }

                throw new NotImplementedException();
                //row = row - _viewModel.ItemsWithHeaders.Count - 1;

                //return _viewModel.PastCompletedItemsWithHeaders[row];
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

                        case CELL_ID_HEADER:
                            cell = new UIFriendlyDateHeaderCell(CELL_ID_HEADER);
                            break;

                        case CELL_ID_BUTTON:
                            cell = new BareUITableViewCell(CELL_ID_BUTTON);
                            break;
                    }
                }

                cell.DataContext = GetItem(indexPath.Row);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                nint count = _viewModel.ItemsWithHeaders.Count;

                // In future when I add support for that, I'll have to uncomment this code again
                //if (_viewModel.HasPastCompletedItems)
                //{
                //    count++;

                //    if (_viewModel.IsPastCompletedItemsDisplayed)
                //    {
                //        count += _viewModel.PastCompletedItemsWithHeaders.Count;
                //    }
                //}

                return count;
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

        public void ScrollToItem(BaseViewItemHomeworkExam changedItem)
        {
            (_tableView.Source as TableViewSource)?.ScrollToItem(changedItem);
        }
    }
}