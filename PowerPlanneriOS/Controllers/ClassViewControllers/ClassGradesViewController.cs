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

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassGradesViewController : BareMvvmUIViewController<ClassGradesViewModel>
    {
        private UITableView _tableView;
        private object _tabBarHeightListener;
        private BindingHost _classBindingHost = new BindingHost();
        //private BareUIStackViewItemsSourceAdapter<UIWeightSummaryView> _itemsSourceWeightSummaries;
        private BareUIViewItemsSourceAdapterAsStackPanel _itemsSourceWeightSummaries;

        public ClassGradesViewController()
        {
            Title = "Grades";

            _tableView = new UITableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                SeparatorInset = UIEdgeInsets.Zero
            };
            _tableView.TableHeaderView = new UIGradesHeaderView(_classBindingHost, this, out _itemsSourceWeightSummaries);
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

        public class UIGradesHeaderView : UIView
        {
            private UILabel _labelPercent;
            private UILabel _labelGpa;
            private UILabel _labelCredits;
            private UIButton _buttonEditCredits;
            private UIView _summaryCategories;
            private ClassGradesViewController _viewController;

            public UIGradesHeaderView(BindingHost classBindingHost, ClassGradesViewController viewController, out BareUIViewItemsSourceAdapterAsStackPanel itemsSourceWeightSummaries)
            {
                _viewController = viewController;

                _labelPercent = new UILabel()
                {
                    Font = UIFont.PreferredTitle2
                };
                classBindingHost.SetLabelTextBinding<double>(_labelPercent, nameof(ViewItemClass.Grade), converter: GradeToStringConverter.Convert);
                Add(_labelPercent);

                _labelGpa = new UILabel()
                {
                    Font = UIFont.PreferredTitle3,
                    TextAlignment = UITextAlignment.Right
                };
                classBindingHost.SetLabelTextBinding(_labelGpa, nameof(ViewItemClass.GpaString));
                Add(_labelGpa);

                _labelCredits = new UILabel()
                {
                    Font = UIFont.PreferredCaption1,
                    TextAlignment = UITextAlignment.Right
                };
                classBindingHost.SetLabelTextBinding<double>(_labelCredits, nameof(ViewItemClass.Credits), converter: CreditsToStringConverter.ConvertWithCredits);
                Add(_labelCredits);

                _buttonEditCredits = new UIButton(UIButtonType.System);
                _buttonEditCredits.SetTitle("Edit", UIControlState.Normal);
                _buttonEditCredits.TouchUpInside += new WeakEventHandler(delegate { _viewController.ViewModel.ConfigureGrades(); }).Handler;
                Add(_buttonEditCredits);

                _summaryCategories = new UIView();
                itemsSourceWeightSummaries = new BareUIViewItemsSourceAdapterAsStackPanel(_summaryCategories, (o) => new UIWeightSummaryView() { DataContext = o });
                viewController.BindingHost.SetBinding(nameof(ClassGradesViewModel.ShowWeightCategoriesSummary), delegate
                {
                    SetNeedsLayout(); // Will invoke LayoutSubviews in next display cycle
                });
                Add(_summaryCategories);
            }

            public override void LayoutSubviews()
            {
                nfloat currY = 16;

                CGSize infinityHeightSize = new CGSize(base.Frame.Width, double.MaxValue);

                var percentSize = _labelPercent.SizeThatFits(infinityHeightSize);
                _labelPercent.Frame = new CGRect(16, 16, percentSize.Width, percentSize.Height);

                var gpaSize = _labelGpa.SizeThatFits(infinityHeightSize);
                _labelGpa.Frame = new CGRect(base.Frame.Width - gpaSize.Width - 16, currY, gpaSize.Width, gpaSize.Height);
                currY += gpaSize.Height;

                var creditsSize = _labelCredits.SizeThatFits(infinityHeightSize);
                _labelCredits.Frame = new CGRect(base.Frame.Width - creditsSize.Width - 16, currY, creditsSize.Width, creditsSize.Height);
                currY += creditsSize.Height;

                var editCreditsSize = _buttonEditCredits.SizeThatFits(infinityHeightSize);
                _buttonEditCredits.Frame = new CGRect(base.Frame.Width - editCreditsSize.Width - 16, currY, editCreditsSize.Width, editCreditsSize.Height);
                currY += editCreditsSize.Height;

                currY += 16;

                if (_viewController.ViewModel != null && _viewController.ViewModel.ShowWeightCategoriesSummary)
                {
                    // Show the summary
                    nfloat summaryRowHeight = UIFont.PreferredCaption1.LineHeight;
                    nfloat summaryHeight = summaryRowHeight * _summaryCategories.Subviews.Length;
                    _summaryCategories.Frame = new CGRect(16, currY, base.Frame.Width - 32, summaryHeight);
                    currY += summaryHeight;

                    currY += 16;
                }
                else
                {
                    // Hide the summary
                    _summaryCategories.Frame = new CGRect(16, currY, base.Frame.Width - 32, 0);
                }

                base.Frame = new CGRect(base.Frame.X, base.Frame.Y, base.Frame.Width, currY);
            }
        }

        private class UIWeightSummaryView : BareUIView
        {
            public UIWeightSummaryView()
            {
                var labelName = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1
                };
                BindingHost.SetLabelTextBinding(labelName, nameof(ViewItemWeightCategory.Name));
                Add(labelName);
                labelName.StretchHeight(this);

                var labelGrade = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1,
                    TextColor = UIColorCompat.SecondaryLabelColor
                };
                BindingHost.SetLabelTextBinding(labelGrade, nameof(ViewItemWeightCategory.WeightAchievedAndTotalString));
                Add(labelGrade);
                labelGrade.StretchHeight(this);

                this.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[name]->=0-[grade]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "name", labelName,
                    "grade", labelGrade));
                labelName.SetContentHuggingPriority(499, UILayoutConstraintAxis.Horizontal);
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
            _classBindingHost.BindingObject = ViewModel.Class;
            _tableView.Source = new TableViewSource(_tableView, ViewModel);
            _itemsSourceWeightSummaries.ItemsSource = ViewModel.Class.WeightCategories;

            ViewModel.Class.PropertyChanged += new WeakEventHandler(HeaderLayoutAffectingPropertyChanged).Handler;
            ViewModel.Class.WeightCategories.CollectionChanged += new WeakEventHandler(HeaderLayoutAffectingPropertyChanged).Handler;

            base.OnViewModelLoadedOverride();
        }

        private void HeaderLayoutAffectingPropertyChanged(object sender, EventArgs e)
        {
            // Need the header to recalculate its size
            _tableView.TableHeaderView?.LayoutSubviews();
        }
    }
}