using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using InterfacesiOS.Controllers;
using ToolsPortable;
using System.ComponentModel;
using System.Collections.Specialized;
using InterfacesiOS.Views;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Helpers;

namespace PowerPlanneriOS.Controllers
{
    public class AgendaViewController : PopupViewController<AgendaViewModel>
    {
        private UITableView _tableView;
        private object _tabBarHeightListener;

        public AgendaViewController()
        {
            Title = GetTitle();
            HideBackButton();

            NavItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add)
            {
                Title = "Add item"
            };
            NavItem.RightBarButtonItem.Clicked += new WeakEventHandler<EventArgs>(ButtonAddItem_Clicked).Handler;

            _tableView = new UITableView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                SeparatorInset = UIEdgeInsets.Zero
            };
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                // Stretch to full width even on iPad
                _tableView.CellLayoutMarginsFollowReadableWidth = false;
            }
            _tableView.TableFooterView = new UIView(); // Eliminate extra separators on bottom of view
            ContentView.Add(_tableView);
            _tableView.StretchWidthAndHeight(ContentView);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _tableView.ContentInset = new UIEdgeInsets(0, 0, MainScreenViewController.TAB_BAR_HEIGHT, 0);
            });
        }

        private void ButtonAddItem_Clicked(object sender, EventArgs e)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            actionSheetAlert.AddAction(UIAlertAction.Create("Add Task", UIAlertActionStyle.Default, delegate { ViewModel.AddTask(); }));
            actionSheetAlert.AddAction(UIAlertAction.Create("Add Event", UIAlertActionStyle.Default, delegate { ViewModel.AddEvent(); }));

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = NavItem.RightBarButtonItem;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            this.PresentViewController(actionSheetAlert, true, null);
        }

        protected string GetTitle()
        {
            return "Agenda";
        }

        public override void OnViewModelLoadedOverride()
        {
            _tableView.Source = new TableViewSource(_tableView, ViewModel);

            var labelNothingHere = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
                Font = UIFont.PreferredCallout,
                Text = "No tasks! Tap the \"+\" button in the top right to add tasks and events.",
                TextColor = UIColorCompat.SecondaryLabelColor,
                TextAlignment = UITextAlignment.Center,
                Hidden = true
            };
            View.Add(labelNothingHere);
            labelNothingHere.StretchWidth(View, left: 32, right: 32);
            labelNothingHere.StretchHeight(View, top: 16, bottom: 16);
            BindingHost.SetBinding(nameof(ViewModel.HasNoItems), delegate
            {
                labelNothingHere.Hidden = !ViewModel.HasNoItems;
            });

            base.OnViewModelLoadedOverride();
        }

        protected override void OnViewReturnedTo()
        {
            try
            {
                if (IsViewModelLoaded)
                {
                    var changedItem = ViewModel.GetLastChangedItem();
                    if (changedItem != null)
                    {
                        (_tableView.Source as TableViewSource)?.ScrollToItem(changedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            base.OnViewReturnedTo();
        }

        private class TableViewSource : UITableViewSource
        {
            private UITableView _tableView;
            private AgendaViewModel _viewModel;

            private const string CELL_ID_ITEM = "Item";
            private const string CELL_ID_HEADER = "Header";
            private const string CELL_ID_BUTTON = "Button";

            public TableViewSource(UITableView tableView, AgendaViewModel viewModel)
            {
                _tableView = tableView;
                _viewModel = viewModel;

                (viewModel.ItemsWithHeaders as INotifyCollectionChanged).CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(TableViewSource_CollectionChanged).Handler;

                tableView.ReloadData();
            }

            public void ScrollToItem(ViewItemTaskOrEvent item)
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
                return _viewModel.ItemsWithHeaders[row] is AgendaViewModel.ItemsGroup ? CELL_ID_HEADER : CELL_ID_ITEM;
            }

            private object GetItem(int row)
            {
                return _viewModel.ItemsWithHeaders[row];
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
                            cell = new UIHeaderCell(CELL_ID_HEADER);
                            break;
                    }
                }

                cell.DataContext = GetItem(indexPath.Row);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _viewModel.ItemsWithHeaders.Count;
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

            private class UIHeaderCell : BareUITableViewCell<AgendaViewModel.ItemsGroup>
            {
                private UILabel _labelText;

                public UIHeaderCell(string cellId) : base(cellId)
                {
                    // Don't allow clicking on this header cell
                    UserInteractionEnabled = false;

                    ContentView.BackgroundColor = UIColorCompat.SecondarySystemBackgroundColor;

                    _labelText = new UILabel()
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        Font = UIFont.PreferredSubheadline,
                        TextColor = UIColorCompat.SecondaryLabelColor
                    };
                    ContentView.AddSubview(_labelText);
                    _labelText.StretchWidthAndHeight(ContentView, left: 16, top: 8, bottom: 8);

                    ContentView.SetHeight(44);
                }

                protected override void OnDataContextChanged()
                {
                    _labelText.Text = GetHeaderText(DataContext);

                    base.OnDataContextChanged();
                }

                private static string GetHeaderText(AgendaViewModel.ItemsGroup itemsGroup)
                {
                    switch (itemsGroup)
                    {
                        case AgendaViewModel.ItemsGroup.Overdue:
                            return PowerPlannerResources.GetRelativeDateInThePast();

                        case AgendaViewModel.ItemsGroup.Today:
                            return PowerPlannerResources.GetRelativeDateToday();

                        case AgendaViewModel.ItemsGroup.Tomorrow:
                            return PowerPlannerResources.GetRelativeDateTomorrow();

                        case AgendaViewModel.ItemsGroup.InTwoDays:
                            return PowerPlannerResources.GetRelativeDateInXDays(2);

                        case AgendaViewModel.ItemsGroup.WithinSevenDays:
                            return PowerPlannerResources.GetRelativeDateWithinXDays(7);

                        case AgendaViewModel.ItemsGroup.WithinFourteenDays:
                            return PowerPlannerResources.GetRelativeDateWithinXDays(14);

                        case AgendaViewModel.ItemsGroup.WithinThirtyDays:
                            return PowerPlannerResources.GetRelativeDateWithinXDays(30);

                        case AgendaViewModel.ItemsGroup.WithinSixtyDays:
                            return PowerPlannerResources.GetRelativeDateWithinXDays(60);

                        case AgendaViewModel.ItemsGroup.InTheFuture:
                            return PowerPlannerResources.GetRelativeDateFuture();

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}