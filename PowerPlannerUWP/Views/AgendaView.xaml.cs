using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerUWP.Views.TaskOrEventViews;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using ToolsUniversal;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AgendaView : MainScreenContentViewHostGeneric
    {
        private class FilteredList : MyObservableList<ViewItemTaskOrEvent>
        {
            private class DateFilter : IFilter<ViewItemTaskOrEvent>
            {
                private DateTime _min, _max;

                /// <summary>
                /// Min is inclusive, max is NOT inclusive
                /// </summary>
                /// <param name="min"></param>
                /// <param name="max"></param>
                public DateFilter(DateTime min, DateTime max)
                {
                    _min = min;
                    _max = max;
                }

                public bool ShouldInsert(ViewItemTaskOrEvent itemToBeInserted)
                {
                    return itemToBeInserted.EffectiveDateForDisplayInDateBasedGroups.Date >= _min && itemToBeInserted.EffectiveDateForDisplayInDateBasedGroups.Date <= _max;
                }
            }
            
            /// <summary>
            /// Only date (not time) is compared, dates are inclusive
            /// </summary>
            /// <param name="allItems"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            public FilteredList(MyObservableList<ViewItemTaskOrEvent> allItems, DateTime min, DateTime max)
            {
                base.Filter = new DateFilter(DateTime.SpecifyKind(min.Date, DateTimeKind.Utc), DateTime.SpecifyKind(max.Date, DateTimeKind.Utc));

                base.InsertSorted(allItems);
            }
        }

        public new AgendaViewModel ViewModel
        {
            get { return base.ViewModel as AgendaViewModel; }
            set { base.ViewModel = value; }
        }

        public AgendaView()
        {
            this.InitializeComponent();
        }

#if DEBUG
        ~AgendaView()
        {
            System.Diagnostics.Debug.WriteLine("AgendaView disposed");
        }
#endif

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            try
            {
                _inThePastColumn = GenerateColumn(LocalizedResources.Common.GetRelativeDateInThePast().ToUpper(), DateTime.Today.AddDays(-1), "ItemsInThePast");

                AddColumn(LocalizedResources.Common.GetRelativeDateToday().ToUpper(), DateTime.Today, "ItemsToday");
                AddColumn(LocalizedResources.Common.GetRelativeDateTomorrow().ToUpper(), DateTime.Today.AddDays(1), "ItemsTomorrow");
                AddColumn(LocalizedResources.Common.GetRelativeDateInXDays(2).ToUpper(), DateTime.Today.AddDays(2), "ItemsInTwoDays");
                AddColumn(LocalizedResources.Common.GetRelativeDateWithinXDays(7).ToUpper(), DateTime.Today.AddDays(3), "ItemsWithinSevenDays");
                AddColumn(LocalizedResources.Common.GetRelativeDateWithinXDays(14).ToUpper(), DateTime.Today.AddDays(8), "ItemsWithinFourteenDays");
                AddColumn(LocalizedResources.Common.GetRelativeDateWithinXDays(30).ToUpper(), DateTime.Today.AddDays(15), "ItemsWithinThirtyDays");
                AddColumn(LocalizedResources.Common.GetRelativeDateWithinXDays(60).ToUpper(), DateTime.Today.AddDays(31), "ItemsWithinSixtyDays");
                AddColumn(LocalizedResources.Common.GetRelativeDateFuture().ToUpper(), DateTime.Today.AddDays(61), "ItemsInTheFuture");
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                SetCommandBarCommands(new ICommandBarElement[]
                {
                AppBarAdd
                }, null);

                var items = ViewModel.AgendaViewItemsGroup.Items;
                var today = ViewModel.Today;

                ItemsInThePast = new FilteredList(items, DateTime.MinValue, today.AddDays(-1));
                ItemsToday = new FilteredList(items, today, today);
                ItemsTomorrow = new FilteredList(items, today.AddDays(1), today.AddDays(1));
                ItemsInTwoDays = new FilteredList(items, today.AddDays(2), today.AddDays(2));
                ItemsWithinSevenDays = new FilteredList(items, today.AddDays(3), today.AddDays(7));
                ItemsWithinFourteenDays = new FilteredList(items, today.AddDays(8), today.AddDays(14));
                ItemsWithinThirtyDays = new FilteredList(items, today.AddDays(15), today.AddDays(30));
                ItemsWithinSixtyDays = new FilteredList(items, today.AddDays(31), today.AddDays(60));
                ItemsInTheFuture = new FilteredList(items, today.AddDays(61), DateTime.MaxValue);

                ItemsInThePast.CollectionChanged += ItemsInThePast_CollectionChanged;
                ResetInThePastVisibility();
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private AppBarButton _appBarAdd;
        private AppBarButton AppBarAdd
        {
            get
            {
                if (_appBarAdd == null)
                    _appBarAdd = CreateAppBarButton(Symbol.Add, LocalizedResources.Common.GetStringNewItem(), appBarAdd_Click);

                return _appBarAdd;
            }
        }

        private void appBarAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ShowFlyoutAddTaskOrEvent(
                    elToCenterFrom: sender as FrameworkElement,
                    addTaskAction: ViewModel.AddTask,
                    addEventAction: ViewModel.AddEvent);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private FrameworkElement _inThePastColumn;

        private FrameworkElement AddColumn(string header, DateTime dateForNewItems, string itemsSourcePath)
        {
            var col = GenerateColumn(header, dateForNewItems, itemsSourcePath);
            
            stackPanel.Children.Add(col);

            return col;
        }

        private FrameworkElement GenerateColumn(string header, DateTime dateForNewItems, string itemsSourcePath)
        {
            Grid g = new Grid()
            {
                Margin = new Thickness(10, 10, 10, 24)
            };
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            var headerView = new TasksOrEventsListHeader()
            {
                Header = header,
                DateToUseForNewItems = dateForNewItems,
                Classes = ViewModel.Classes
            };

            g.Children.Add(headerView);

            var list = new TasksOrEventsItemsControl()
            {
                Margin = new Thickness(0, 5, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            list.SetBinding(TasksOrEventsItemsControl.ItemsSourceProperty, new Binding() { Path = new PropertyPath(itemsSourcePath), Source = this });
            Grid.SetRow(list, 1);

            g.Children.Add(list);

            return g;
        }

        private void ItemsInThePast_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                ResetInThePastVisibility();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ResetInThePastVisibility()
        {
            if (stackPanel.Children.Count == 0)
                return;
            
            bool shouldDisplay = false;

            if (_inThePastColumn != null && ItemsInThePast != null)
                shouldDisplay = ItemsInThePast.Count > 0;

            if (shouldDisplay)
            {
                if (stackPanel.Children[0] != _inThePastColumn)
                    stackPanel.Children.Insert(0, _inThePastColumn);
            }

            else
                stackPanel.Children.Remove(_inThePastColumn);
        }
        
        private static readonly DependencyProperty ItemsInThePastProperty = DependencyProperty.Register("ItemsInThePast", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsInThePast
        {
            get { return GetValue(ItemsInThePastProperty) as FilteredList; }
            set { SetValue(ItemsInThePastProperty, value); }
        }

        private static readonly DependencyProperty ItemsTodayProperty = DependencyProperty.Register("ItemsToday", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsToday
        {
            get { return GetValue(ItemsTodayProperty) as FilteredList; }
            set { SetValue(ItemsTodayProperty, value); }
        }

        private static readonly DependencyProperty ItemsTomorrowProperty = DependencyProperty.Register("ItemsTomorrow", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsTomorrow
        {
            get { return GetValue(ItemsTomorrowProperty) as FilteredList; }
            set { SetValue(ItemsTomorrowProperty, value); }
        }

        private static readonly DependencyProperty ItemsInTwoDaysProperty = DependencyProperty.Register("ItemsInTwoDays", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsInTwoDays
        {
            get { return GetValue(ItemsInTwoDaysProperty) as FilteredList; }
            set { SetValue(ItemsInTwoDaysProperty, value); }
        }

        private static readonly DependencyProperty ItemsWithinSevenDaysProperty = DependencyProperty.Register("ItemsWithinSevenDays", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsWithinSevenDays
        {
            get { return GetValue(ItemsWithinSevenDaysProperty) as FilteredList; }
            set { SetValue(ItemsWithinSevenDaysProperty, value); }
        }

        private static readonly DependencyProperty ItemsWithinFourteenDaysProperty = DependencyProperty.Register("ItemsWithinFourteenDays", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsWithinFourteenDays
        {
            get { return GetValue(ItemsWithinFourteenDaysProperty) as FilteredList; }
            set { SetValue(ItemsWithinFourteenDaysProperty, value); }
        }

        private static readonly DependencyProperty ItemsWithinThirtyDaysProperty = DependencyProperty.Register("ItemsWithinThirtyDays", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsWithinThirtyDays
        {
            get { return GetValue(ItemsWithinThirtyDaysProperty) as FilteredList; }
            set { SetValue(ItemsWithinThirtyDaysProperty, value); }
        }

        private static readonly DependencyProperty ItemsWithinSixtyDaysProperty = DependencyProperty.Register("ItemsWithinSixtyDays", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsWithinSixtyDays
        {
            get { return GetValue(ItemsWithinSixtyDaysProperty) as FilteredList; }
            set { SetValue(ItemsWithinSixtyDaysProperty, value); }
        }

        private static readonly DependencyProperty ItemsInTheFutureProperty = DependencyProperty.Register("ItemsInTheFuture", typeof(FilteredList), typeof(AgendaView), null);

        private FilteredList ItemsInTheFuture
        {
            get { return GetValue(ItemsInTheFutureProperty) as FilteredList; }
            set { SetValue(ItemsInTheFutureProperty, value); }
        }
    }
}
