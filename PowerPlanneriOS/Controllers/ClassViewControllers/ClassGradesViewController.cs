using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using InterfacesiOS.Binding;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewItems;
using System.Collections.Specialized;
using ToolsPortable;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using CoreGraphics;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesiOS.Helpers;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassGradesViewController : BareMvvmUIViewController<ClassGradesViewModel>
    {
        private UITableView _tableView;
        private object _tabBarHeightListener;

        public ClassGradesViewController()
        {
            Title = "Grades";
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            _tableView = new UITableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                SeparatorInset = UIEdgeInsets.Zero
            };
            _tableView.TableHeaderView = ViewModel.SummaryComponent.Render();
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

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (_tableView != null && _tableView.TableHeaderView != null)
            {
                // Support dynamic height table header: https://stackoverflow.com/questions/34661793/setting-tableheaderview-height-dynamically
                var height = _tableView.TableHeaderView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height;
                var frame = _tableView.TableHeaderView.Frame;

                // Avoid infinite loop
                if (height != frame.Size.Height)
                {
                    _tableView.TableHeaderView.Frame = new CGRect(frame.Location, new CGSize(frame.Width, height));
                    _tableView.SetNeedsLayout();
                }
            }
        }

        private class TableViewSource : UITableViewSource
        {
            private UITableView _tableView;
            private ViewItemClass _class;
            private ClassGradesViewModel _viewModel;

            public TableViewSource(UITableView tableView, ClassGradesViewModel viewModel)
            {
                _tableView = tableView;
                _viewModel = viewModel;
                _class = viewModel.Class;
                _gradesCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Grades_CollectionChanged).Handler;
                _class.WeightCategories.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(WeightCategories_CollectionChanged).Handler;
                _viewModel.ClassViewModel.ViewItemsGroupClass.UnassignedItems.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(UnassignedItems_CollectionChanged).Handler;
                UpdateGradeCollectionChangedHandlers();
            }

            private void UnassignedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                _tableView.ReloadData();
            }

            private NotifyCollectionChangedEventHandler _gradesCollectionChangedHandler;
            private List<ViewItemWeightCategory> _trackedWeightCategories = new List<ViewItemWeightCategory>();
            private void WeightCategories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                try
                {
                    UpdateGradeCollectionChangedHandlers();
                    _tableView.ReloadData();
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            private void UpdateGradeCollectionChangedHandlers()
            {
                foreach (var w in _class.WeightCategories)
                {
                    if (!_trackedWeightCategories.Contains(w))
                    {
                        w.Grades.CollectionChanged += _gradesCollectionChangedHandler;
                        _trackedWeightCategories.Add(w);
                    }
                }

                for (int i = 0; i < _trackedWeightCategories.Count; i++)
                {
                    var w = _trackedWeightCategories[i];
                    if (!_class.WeightCategories.Contains(w))
                    {
                        w.Grades.CollectionChanged -= _gradesCollectionChangedHandler;
                        _trackedWeightCategories.RemoveAt(i);
                        i--;
                    }
                }
            }

            private void Grades_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                _tableView.ReloadData();
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return _class.WeightCategories.Count + (_viewModel.ClassViewModel.ViewItemsGroupClass.HasUnassignedItems ? 1 : 0);
            }

            public override UIView GetViewForHeader(UITableView tableView, nint section)
            {
                UIWeightHeaderCell cell = tableView.DequeueReusableHeaderFooterView("Header") as UIWeightHeaderCell;
                if (cell == null)
                {
                    cell = new UIWeightHeaderCell(new NSString("Header"));
                }

                if (section < _class.WeightCategories.Count)
                {
                    cell.WeightCategory = _class.WeightCategories[(int)section];
                }
                else
                {
                    cell.WeightCategory = ViewItemWeightCategory.UNASSIGNED;
                }

                return cell;
            }

            private object GetItem(NSIndexPath indexPath)
            {
                if (indexPath.Section < _class.WeightCategories.Count)
                {
                    return _class.WeightCategories[indexPath.Section].Grades[indexPath.Row];
                }

                // Otherwise return unassigned
                return _viewModel.ClassViewModel.ViewItemsGroupClass.UnassignedItems[indexPath.Row];
            }

            private const string CELL_ID_GRADE = "Grade";
            private const string CELL_ID_UNASSIGNED = "Unassigned";
            private string GetCellId(NSIndexPath indexPath)
            {
                if (indexPath.Section < _class.WeightCategories.Count)
                {
                    return CELL_ID_GRADE;
                }

                return CELL_ID_UNASSIGNED;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var item = GetItem(indexPath);
                string cellId = GetCellId(indexPath);

                BareUITableViewCell cell = tableView.DequeueReusableCell(cellId) as BareUITableViewCell;

                // If no cells to reuse, create a new one
                if (cell == null)
                {
                    switch (cellId)
                    {
                        case CELL_ID_GRADE:
                            cell = new UIGradeCell(CELL_ID_GRADE);
                            break;

                        case CELL_ID_UNASSIGNED:
                            cell = new UITaskCell(CELL_ID_UNASSIGNED);
                            break;
                    }
                }

                cell.DataContext = item;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section < _class.WeightCategories.Count)
                {
                    return _class.WeightCategories[(int)section].Grades.Count;
                }

                return _viewModel.ClassViewModel.ViewItemsGroupClass.UnassignedItems.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                object item = GetItem(indexPath);

                if (item is BaseViewItemMegaItem)
                {
                    if (indexPath.Section < _class.WeightCategories.Count)
                    {
                        _viewModel.ShowItem(item as BaseViewItemMegaItem);
                    }
                    else if (item is ViewItemTaskOrEvent taskOrEvent)
                    {
                        _viewModel.ShowUnassignedItem(taskOrEvent);
                    }

                    // Immediately unselect it
                    _tableView.SelectRow(null, true, UITableViewScrollPosition.None);
                }
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            _tableView.Source = new TableViewSource(_tableView, ViewModel);

            base.OnViewModelLoadedOverride();
        }
    }
}