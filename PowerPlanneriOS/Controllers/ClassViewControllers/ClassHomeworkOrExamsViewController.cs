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
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlanneriOS.Helpers;

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
            private const string CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON = "ShowHideButton";
            private const string CELL_ID_OLD_ITEMS_HEADER = "OldItemsHeader";

            public TableViewSource(UITableView tableView, ClassHomeworkOrExamsViewModel viewModel)
            {
                _tableView = tableView;
                _viewModel = viewModel;

                viewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
                viewModel.ClassViewModel.ViewItemsGroupClass.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewItemsGroupClass_PropertyChanged).Handler;
                (viewModel.ItemsWithHeaders as INotifyCollectionChanged).CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(TableViewSource_CollectionChanged).Handler;

                tableView.ReloadData();
            }

            private void ViewItemsGroupClass_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (_viewModel.Type == ClassHomeworkOrExamsViewModel.ItemType.Homework)
                {
                    if (e.PropertyName == nameof(ClassViewItemsGroup.PastCompletedHomework))
                    {
                        ReloadData();
                    }
                }
                else
                {
                    if (e.PropertyName == nameof(ClassViewItemsGroup.PastCompletedExams))
                    {
                        ReloadData();
                    }
                }
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
                switch (e.PropertyName)
                {
                    case nameof(_viewModel.HasPastCompletedItems):
                    case nameof(_viewModel.IsPastCompletedItemsDisplayed):
                        ReloadData();
                        break;

                    case nameof(_viewModel.PastCompletedItemsWithHeaders):
                        if (_viewModel.PastCompletedItemsWithHeaders is INotifyCollectionChanged pastCompletedItemsCollection)
                        {
                            pastCompletedItemsCollection.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(PastCompletedItemsCollection_CollectionChanged).Handler;
                        }
                        ReloadData();
                        break;
                }
            }

            private void PastCompletedItemsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                ReloadData();
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
                    return CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON;
                }

                // I need to actually load these first
                row = row - _viewModel.ItemsWithHeaders.Count - 1;

                return _viewModel.PastCompletedItemsWithHeaders[row] is DateTime ? CELL_ID_HEADER : CELL_ID_ITEM;
            }

            private object GetItem(int row)
            {
                if (row < _viewModel.ItemsWithHeaders.Count)
                {
                    return _viewModel.ItemsWithHeaders[row];
                }

                if (row == _viewModel.ItemsWithHeaders.Count)
                {
                    return CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON;
                }

                row = row - _viewModel.ItemsWithHeaders.Count - 1;

                return _viewModel.PastCompletedItemsWithHeaders[row];
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

                        case CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON:
                            cell = new UIShowHideOldItemsCell(CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON, _viewModel.ClassViewModel.ViewItemsGroupClass, _viewModel.Type);
                            break;
                    }
                }

                // We don't set data context on the Show Hide old items button, since it already set the data context in its construtor
                if (cellId != CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON)
                {
                    cell.DataContext = GetItem(indexPath.Row);
                }

                return cell;
            }

            private class UIShowHideOldItemsCell : BareUITableViewCell
            {
                private UILabel _labelText;

                public UIShowHideOldItemsCell(string cellId, ClassViewItemsGroup classItemsGroup, ClassHomeworkOrExamsViewModel.ItemType type) : base(cellId)
                {
                    ContentView.BackgroundColor = new UIColor(0.95f, 1);

                    _labelText = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = UIFont.PreferredSubheadline,
                        TextColor = ColorResources.PowerPlannerAccentBlue,
                        TextAlignment = UITextAlignment.Center
                    };

                    DataContext = classItemsGroup;

                    if (type == ClassHomeworkOrExamsViewModel.ItemType.Homework)
                    {
                        BindingHost.SetLabelTextBinding(_labelText, nameof(classItemsGroup.IsPastCompletedHomeworkDisplayed), (isDisplayed) =>
                        {
                            return ((bool)isDisplayed) ? "Hide old tasks" : "Show old tasks";
                        });
                    }
                    else
                    {
                        BindingHost.SetLabelTextBinding(_labelText, nameof(classItemsGroup.IsPastCompletedExamsDisplayed), (isDisplayed) =>
                        {
                            return ((bool)isDisplayed) ? "Hide old events" : "Show old events";
                        });
                    }

                    ContentView.AddSubview(_labelText);
                    _labelText.StretchWidthAndHeight(ContentView, left: 16, top: 8, bottom: 8);

                    ContentView.SetHeight(44);
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                nint count = _viewModel.ItemsWithHeaders.Count;

                if (_viewModel.HasPastCompletedItems)
                {
                    count++;

                    if (_viewModel.IsPastCompletedItemsDisplayed && _viewModel.PastCompletedItemsWithHeaders != null)
                    {
                        count += _viewModel.PastCompletedItemsWithHeaders.Count;
                    }
                }

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

                else if (item is string s && s == CELL_ID_SHOW_HIDE_OLD_ITEMS_BUTTON)
                {
                    _viewModel.TogglePastCompletedItems();
                }
            }
        }

        public void ScrollToItem(BaseViewItemHomeworkExam changedItem)
        {
            (_tableView.Source as TableViewSource)?.ScrollToItem(changedItem);
        }
    }
}