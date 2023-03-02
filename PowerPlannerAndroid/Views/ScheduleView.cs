using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.Views.ListItems;
using InterfacesDroid.Themes;
using ToolsPortable;
using PowerPlannerAndroid.ViewHosts;
using PowerPlannerAppDataLibrary.ViewLists;
using System.ComponentModel;
using Android.Graphics.Drawables;
using Android.Graphics;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAndroid.Views.Controls;
using InterfacesDroid.Helpers;
using AndroidX.Core.Content;
using PowerPlannerAndroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class ScheduleView : MainScreenViewHostDescendant<ScheduleViewModel>
    {
        private const int INITIAL_MARGIN = 50;
        public const int HEIGHT_OF_HOUR = 120;
        private const int ALL_DAY_TOP_MARGIN = -16;

        private View _normalContent;
        private View _editingContent;
        private View _welcomeContent;
        private ItemsControlWrapper _itemsWrapperEditingClasses;
        private AndroidX.AppCompat.Widget.Toolbar _weekToolbar;

        private LinearLayout _scheduleHost;

        public ScheduleView(ViewGroup root) : base(Resource.Layout.Schedule, root)
        {
            _normalContent = FindViewById(Resource.Id.NormalContent);
            _editingContent = FindViewById(Resource.Id.EditingContent);
            _welcomeContent = FindViewById(Resource.Id.WelcomeContent);

            _weekToolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.WeekToolbar);
            MenuInflater inflater = new MenuInflater(Context);
            inflater.Inflate(Resource.Menu.schedule_week_menu, _weekToolbar.Menu);
            LocalizationHelper.LocalizeMenu(_weekToolbar.Menu);
            _weekToolbar.MenuItemClick += _weekToolbar_MenuItemClick;

            var scrollViewSchedule = FindViewById<MyZoomAndPanView>(Resource.Id.ScrollViewSchedule);
            scrollViewSchedule.ViewChanging += ScrollViewSchedule_ViewChanging;

            FindViewById<Button>(Resource.Id.ButtonLogIn).Click += new WeakEventHandler(ScheduleView_Click).Handler;
        }

        private void ScheduleView_Click(object sender, EventArgs e)
        {
            ViewModel.LogIn();
        }

        private void ScrollViewSchedule_ViewChanging(object sender, EventArgs e)
        {
            CollapseExpandedEvents();
        }

        private IEnumerable<MyBaseEventVisual> FindAllEventVisuals()
        {
            foreach (var col in _scheduleHost.GetAllChildren().Skip(1).OfType<ViewGroup>())
            {
                foreach (var child in col.GetAllChildren())
                {
                    if (child is MyBaseEventVisual)
                    {
                        yield return child as MyBaseEventVisual;
                    }
                    else if (child is ViewGroup)
                    {
                        foreach (var eventVisual in (child as ViewGroup).GetAllChildren().OfType<MyBaseEventVisual>())
                        {
                            yield return eventVisual;
                        }
                    }
                }
            }
        }

        private void ScheduleHost_Click(object sender, EventArgs e)
        {
            CollapseExpandedEvents();
        }

        private void CollapseExpandedEvents()
        {
            foreach (var e in FindAllEventVisuals())
            {
                e.HideFull();
            }
        }

        private void _weekToolbar_MenuItemClick(object sender, AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemPreviousWeek:
                    ViewModel.PreviousWeek();
                    break;

                case Resource.Id.MenuItemNextWeek:
                    ViewModel.NextWeek();
                    break;
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.InitializeArrangers(HEIGHT_OF_HOUR, MyCollapsedEventItem.SPACING_WITH_NO_ADDITIONAL, MyCollapsedEventItem.SPACING_WITH_ADDITIONAL, MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM);

            FindViewById<Button>(Resource.Id.ButtonAddClass).Click += delegate { ViewModel.AddClass(); };
            FindViewById<Button>(Resource.Id.ButtonWelcomeAddClass).Click += delegate { ViewModel.AddClass(); };

            ViewModel.OnFullReset += new WeakEventHandler(ViewModel_OnFullReset).Handler;
            ViewModel.OnItemsForDateChanged += new WeakEventHandler<DateTime>(ViewModel_OnItemsForDateChanged).Handler;
            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            _itemsWrapperEditingClasses = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.EditingClassesViewGroup))
            {
                ItemsSource = ViewModel.SemesterViewItemsGroup.Classes,
                ItemTemplate = new CustomDataTemplate<ViewItemClass>(CreateListItemEditingScheduleClass)
            };

            UpdateLayoutMode();

            // First prepare the schedule host
            _scheduleHost = FindViewById<LinearLayout>(Resource.Id.ScheduleHost);
            _scheduleHost.Click += ScheduleHost_Click;
            _scheduleHost.AddView(CreateColumn(0));
            DayOfWeek dayOfWeek = ViewModel.FirstDayOfWeek;
            for (int i = 0; i < 7; i++, dayOfWeek++)
            {
                var column = CreateColumn(i + 1);

                TextView dayHeader = new TextView(Context)
                {
                    Text = DateTools.ToLocalizedString(dayOfWeek)
                };

                column.AddView(dayHeader);

                _scheduleHost.AddView(column);
            }

            RenderSchedule();
        }

        private void ViewModel_OnItemsForDateChanged(object sender, DateTime e)
        {
            RenderAllDayItems(e);

            if (ResetAllDayItemsViewsHeight())
            {
                // If the height of all day items have changed cause of this,
                // we need to re-render all items on all days
                RenderAllDates();
            }
            else
            {
                RenderDate(e);
            }
        }

        private void ViewModel_OnFullReset(object sender, EventArgs e)
        {
            RenderSchedule();
        }

        private AllDayItemsView GetAllDayItemsView(DayOfWeek day)
        {
            return _allDayItemViews[getColumn(day) - 1]; // Minus 1 since we don't have a times column in this case
        }

        private List<AllDayItemsView> _allDayItemViews = new List<AllDayItemsView>();

        private ViewGroup CreateColumn(int index)
        {
            var view = new FrameLayout(Context);

            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                index == 0 ? LinearLayout.LayoutParams.WrapContent : ThemeHelper.AsPx(Context, 220),
                LinearLayout.LayoutParams.MatchParent);

            // TODO: Alternate background colors

            view.LayoutParameters = lp;

            // Don't add the all day items on the first column (the times column)
            if (index > 0)
            {
                var allDayItemView = new AllDayItemsView(Context)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, 0)
                    {
                        TopMargin = ThemeHelper.AsPx(Context, INITIAL_MARGIN + ALL_DAY_TOP_MARGIN)
                    }
                };
                _allDayItemViews.Add(allDayItemView);
                view.AddView(allDayItemView);
            }

            return view;
        }

        private View CreateListItemEditingScheduleClass(ViewGroup root, ViewItemClass c)
        {
            var view = new ListItemEditingScheduleClassView(root, c);

            view.OnAddClassTimeRequested += View_OnAddClassTimeRequested;
            view.OnEditClassTimesRequested += View_OnEditClassTimesRequested;
            view.OnEditClassRequested += View_OnEditClassRequested;

            return view;
        }

        private void View_OnEditClassRequested(object sender, ViewItemClass e)
        {
            ViewModel.EditClass(e);
        }

        private void View_OnEditClassTimesRequested(object sender, ViewItemSchedule[] e)
        {
            ViewModel.EditTimes(e, useNewStyle: true);
        }

        private void View_OnAddClassTimeRequested(object sender, ViewItemClass e)
        {
            ViewModel.AddTime(e, useNewStyle: true);
        }

        private void ViewModel_OnChangesOccurred(object sender, PowerPlannerAppDataLibrary.DataLayer.DataChangedEvent e)
        {
            RenderSchedule();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.LayoutMode):
                    UpdateLayoutMode();
                    break;

                case nameof(ViewModel.HasAllDayItems):
                    RenderSchedule();
                    break;
            }
        }

        private void UpdateLayoutMode()
        {
            switch (ViewModel.LayoutMode)
            {
                case ScheduleViewModel.LayoutModes.Normal:
                    _normalContent.Visibility = ViewStates.Visible;
                    _editingContent.Visibility = ViewStates.Gone;
                    _welcomeContent.Visibility = ViewStates.Gone;
                    break;

                case ScheduleViewModel.LayoutModes.FullEditing:
                    _normalContent.Visibility = ViewStates.Gone;
                    _editingContent.Visibility = ViewStates.Visible;
                    _welcomeContent.Visibility = ViewStates.Gone;
                    break;

                case ScheduleViewModel.LayoutModes.SplitEditing:
                    _normalContent.Visibility = ViewStates.Visible;
                    _editingContent.Visibility = ViewStates.Visible;
                    _welcomeContent.Visibility = ViewStates.Gone;
                    break;

                case ScheduleViewModel.LayoutModes.Welcome:
                    _normalContent.Visibility = ViewStates.Gone;
                    _editingContent.Visibility = ViewStates.Gone;
                    _welcomeContent.Visibility = ViewStates.Visible;
                    break;
            }

            RequestUpdateMenu();
        }

        private void RenderSchedule()
        {
            var timesViewGroup = _scheduleHost.GetChildAt(0) as ViewGroup;

            // Remove the existing content
            timesViewGroup.RemoveAllViews();
            for (int i = 1; i < _scheduleHost.ChildCount; i++)
            {
                var col = _scheduleHost.GetChildAt(i) as ViewGroup;

                // Remove all but the day header and all items
                while (col.ChildCount > 2)
                    col.RemoveViewAt(2);
            }

            // If there's no items, stop
            if (!ViewModel.IsValid())
            {
                return;
            }

            // Get earliest start and end date
            DateTime today = DateTime.Today;
            DateTime classStartTime = today.Add(ViewModel.StartTime);
            DateTime classEndTime = today.Add(ViewModel.EndTime);

            // Fill in the times on the left column
            for (DateTime tempClassStartTime = classStartTime; classEndTime >= tempClassStartTime; tempClassStartTime = tempClassStartTime.AddHours(1))
            {
                timesViewGroup.AddView(SetMargin(new TextView(Context)
                {
                    Text = DateHelper.ToShortTimeString(tempClassStartTime).TrimEnd(' ', 'P', 'A', 'M', 'a', 'p', 'm')
                },
                left: ThemeHelper.AsPx(Context, 12),
                top: getTopMarginAsPx(tempClassStartTime.TimeOfDay, classStartTime.TimeOfDay) - ThemeHelper.AsPx(Context, 4),
                right: ThemeHelper.AsPx(Context, 12),
                bottom: ThemeHelper.AsPx(Context, 24)));
            }

            // First render the all day items so we'll know the heights
            for (int i = 0; i < 7; i++)
            {
                RenderAllDayItems(ViewModel.StartDate.AddDays(i));
            }

            ResetAllDayItemsViewsHeight();

            RenderAllDates();
        }

        private void RenderAllDates()
        {
            for (int i = 0; i < 7; i++)
            {
                RenderDate(ViewModel.StartDate.AddDays(i));
            }
        }

        private void RenderAllDayItems(DateTime date)
        {
            var arranger = ViewModel.Items[date.DayOfWeek];
            var allDayItemsView = GetAllDayItemsView(date.DayOfWeek);
            allDayItemsView.SetItems(arranger.HolidayAndAllDayItems);
        }

        private int _currAllDayItemsViewsHeight;

        private bool ResetAllDayItemsViewsHeight()
        {
            // Update visibilities
            foreach (var v in _allDayItemViews)
            {
                v.Visibility = ViewModel.HasAllDayItems ? ViewStates.Visible : ViewStates.Gone;
            }

            bool changed = false;

            if (!ViewModel.HasAllDayItems)
            {
                if (_currAllDayItemsViewsHeight != 0)
                {
                    _currAllDayItemsViewsHeight = 0;
                    changed = true;
                    _scheduleHost.GetChildAt(0).SetPadding(0, 0, 0, 0);
                }
            }
            else
            {
                int max = _allDayItemViews.Max(i => i.HeightInDP);
                int finalHeight = max + ALL_DAY_TOP_MARGIN + 8;
                if (_currAllDayItemsViewsHeight != finalHeight)
                {
                    _currAllDayItemsViewsHeight = finalHeight;
                    foreach (var v in _allDayItemViews)
                    {
                        v.LayoutParameters.Height = ThemeHelper.AsPx(Context, max);
                    }
                    changed = true;
                    _scheduleHost.GetChildAt(0).SetPadding(0, ThemeHelper.AsPx(Context, finalHeight), 0, 0);
                }
            }

            return changed;
        }

        private void RenderDate(DateTime date)
        {
            // Clear current items
            var col = _scheduleHost.GetChildAt(getColumn(date.DayOfWeek)) as ViewGroup;

            // Remove all but the day header and all day items
            while (col.ChildCount > 2)
                col.RemoveViewAt(2);

            // Get arranger for the date
            var arranger = ViewModel.Items[date.DayOfWeek];

            foreach (var s in arranger.ScheduleItems)
            {
                var scheduleItem = new MyScheduleItemView(Context, s.Item, date);
                scheduleItem.Clickable = true;
                scheduleItem.Click += ScheduleItem_Click;

                AddVisualItem(scheduleItem, s, date.DayOfWeek);
            }

            // Reverse the order so that when items expand, they appear on top of the items beneath them.
            // Otherwise I would have to do some crazy Z-order logic.
            foreach (var e in arranger.EventItems.Reverse())
            {
                View visual;
                if (e.IsCollapsedMode)
                {
                    visual = new MyCollapsedEventItem(Context)
                    {
                        Item = e
                    };
                }
                else
                {
                    visual = new MyFullEventItem(Context)
                    {
                        Item = e
                    };
                }

                AddVisualItem(visual, e, arranger.Date.DayOfWeek);
            }

            if (!arranger.HolidayAndAllDayItems.Any() && !arranger.EventItems.Any() && !arranger.ScheduleItems.Any() && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
            {
                col.Visibility = ViewStates.Gone;
            }
            else
            {
                col.Visibility = ViewStates.Visible;
            }

            if (arranger.HasHolidays)
            {
                var overlay = new View(Context)
                {
                    Background = new ColorDrawable(new Color(228, 0, 137, 40)),
                    LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                };
                col.AddView(overlay);
            }

            if (arranger.IsDifferentSemester)
            {
                var diffSemesterOverlay = new DifferentSemesterOverlayControl(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(
                        ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.MatchParent)
                };
                diffSemesterOverlay.PinButtonToTop(60);
                col.AddView(diffSemesterOverlay);
            }

            //UpdateTimePaddingBasedOnAllDayItems();
        }

        private void AddVisualItem(View visual, DayScheduleItemsArranger.BaseScheduleItem item, DayOfWeek day)
        {
            View root;

            if (item.NumOfColumns > 1)
            {
                var grid = new LinearLayout(Context)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent),
                    Orientation = Orientation.Horizontal
                };

                for (int i = 0; i < item.NumOfColumns; i++)
                {
                    View colView;

                    if (i == item.Column)
                    {
                        colView = visual;
                    }
                    else
                    {
                        colView = new View(Context);
                    }

                    colView.LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent)
                    {
                        Weight = 1
                    };
                    grid.AddView(colView);
                }

                root = grid;
            }
            else
            {
                root = visual;
            }

            double leftMargin = item.LeftOffset;
            leftMargin += 12;


            root.LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
            {
                LeftMargin = ThemeHelper.AsPx(Context, leftMargin),
                TopMargin = ThemeHelper.AsPx(Context, item.TopOffset + INITIAL_MARGIN + _currAllDayItemsViewsHeight),
                RightMargin = ThemeHelper.AsPx(Context, 12),
                BottomMargin = ThemeHelper.AsPx(Context, 24)
            };

            if (item is DayScheduleItemsArranger.ScheduleItem)
            {
                root.LayoutParameters.Height = ThemeHelper.AsPx(Context, item.Height);
            }

            ViewGroup viewGroup = _scheduleHost.GetChildAt(getColumn(day)) as ViewGroup;

            viewGroup.AddView(root);
        }

        private void ScheduleItem_Click(object sender, EventArgs e)
        {
            var scheduleItem = sender as MyScheduleItemView;

            if (ViewModel.LayoutMode != ScheduleViewModel.LayoutModes.Normal)
            {
                ViewModel.EditTimes(scheduleItem.Schedule);
            }

            else
            {
                ViewModel.ViewClass(scheduleItem.Schedule.Class);
            }
        }

        private int getColumn(DayOfWeek dayOfWeek)
        {
            int answer = (int)dayOfWeek - (int)ViewModel.FirstDayOfWeek;
            if (answer < 0)
            {
                answer = answer + 7;
            }
            return answer + 1; // Plus 1 since we skip the times column
        }

        private View SetMargin(View view, int left, int top, int right, int bottom)
        {
            view.SetPadding(left, top, right, bottom);
            return view;
        }

        private  int getTopMarginAsPx(TimeSpan itemTime, TimeSpan baseTime)
        {
            return ThemeHelper.AsPx(Context, (INITIAL_MARGIN + Math.Max((itemTime - baseTime).TotalHours * HEIGHT_OF_HOUR, 0)));
        }

        protected override int GetMenuResource()
        {
            switch (ViewModel.LayoutMode)
            {
                case ScheduleViewModel.LayoutModes.Normal:
                    return Resource.Menu.schedule_normal;

                case ScheduleViewModel.LayoutModes.FullEditing:
                case ScheduleViewModel.LayoutModes.SplitEditing:
                    return Resource.Menu.schedule_editing;
            }

            return base.GetMenuResource();
        }

        public override void OnMenuItemClick(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemDone:
                    ViewModel.ExitEditMode();
                    break;

                case Resource.Id.MenuItemEdit:
                    ViewModel.EnterEditMode();
                    break;
            }
        }

        private class AllDayItemsView : LinearLayout
        {
            public AllDayItemsView(Context context) : base(context)
            {
                base.Orientation = Orientation.Vertical;
                base.Background = ContextCompat.GetDrawable(Context, Resource.Drawable.all_day_schedule_items_background);
            }

            public void SetItems(IEnumerable<object> items)
            {
                base.RemoveAllViews();

                foreach (var i in items)
                {
                    if (i is ViewItemTaskOrEvent)
                    {
                        var itemView = new MainCalendarItemView(Context)
                        {
                            Item = i as ViewItemTaskOrEvent
                        };
                        (itemView.LayoutParameters as LinearLayout.LayoutParams).RightMargin = ThemeHelper.AsPx(Context, 2);
                        base.AddView(itemView);
                    }
                    else if (i is ViewItemHoliday)
                    {
                        var itemView = new ListItemHolidayScheduleView(Context)
                        {
                            Holiday = i as ViewItemHoliday
                        };
                        (itemView.LayoutParameters as LinearLayout.LayoutParams).RightMargin = ThemeHelper.AsPx(Context, 2);
                        base.AddView(itemView);
                    }
                }

                // Adding a scroll view doesn't seem to work (nested scroll views probably aren't happy),
                // so we'll just let it be infinitely tall for now. We could add a "tap for more" in the future.
                HeightInDP = (base.ChildCount) * MainCalendarItemView.TOTAL_HEIGHT_IN_DP;
            }

            public int HeightInDP { get; private set; }
        }
    }
}