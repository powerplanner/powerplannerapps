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
using Android.Graphics.Drawables;
using Android.Graphics;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.App;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Views;
using InterfacesDroid.DataTemplates;
using System.Collections.Specialized;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System.ComponentModel;
using BareMvvm.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.View;

namespace PowerPlannerAndroid.Views
{
    public abstract class MyBaseEventVisual : FrameLayout
    {
        private MyAdditionalItemsVisual _additionalItemsVisual;
        private LinearLayout _normalGrid;
        private FrameLayout _expandedContainer;

        public const string TELEMETRY_ON_CLICK_EVENT_NAME = "Click_ScheduleEventItem";

        public MyBaseEventVisual(Context context) : base(context)
        {
            _normalGrid = new LinearLayout(context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            _normalGrid.SetVerticalGravity(GravityFlags.Top);

            base.SetForegroundGravity(GravityFlags.Top | GravityFlags.FillHorizontal);

            _additionalItemsVisual = new MyAdditionalItemsVisual(context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent)
            };
            _normalGrid.AddView(_additionalItemsVisual);

            base.AddView(_normalGrid);

            var expandedMargin = ThemeHelper.AsPx(context, -3);

            _expandedContainer = new FrameLayout(context)
            {
                Visibility = ViewStates.Gone,
                Background = ContextCompat.GetDrawable(context, Resource.Drawable.expanded_schedule_items_background),
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
                {
                    LeftMargin = expandedMargin,
                    TopMargin = expandedMargin,
                    RightMargin = expandedMargin,
                    BottomMargin = expandedMargin
                }
            };
            _expandedContainer.SetPadding(0, ThemeHelper.AsPx(context, 6), 0, ThemeHelper.AsPx(context, 6));
            
            base.AddView(_expandedContainer);
        }

        public bool IsFullItem
        {
            set
            {
                if (_normalGrid.ChildCount >= 2)
                {
                    _normalGrid.GetChildAt(0).LayoutParameters.Width = value ? 0 : ThemeHelper.AsPx(Context, MyCollapsedEventItem.WIDTH_OF_COLLAPSED_ITEM);
                    (_normalGrid.GetChildAt(0).LayoutParameters as LinearLayout.LayoutParams).Weight = value ? 1 : 0;
                }
            }
        }

        private DayScheduleItemsArranger.EventItem _item;
        public DayScheduleItemsArranger.EventItem Item
        {
            get { return _item; }
            set
            {
                _item = value;

                _additionalItemsVisual.AdditionalItems = null;

                if (_normalGrid.ChildCount == 2)
                {
                    _normalGrid.RemoveViewAt(0);
                }

                if (value != null)
                {
                    var height = ThemeHelper.AsPx(Context, value.Height);
                    var content = GenerateContent(value);
                    _normalGrid.AddView(content, 0);
                    _additionalItemsVisual.AdditionalItems = value.AdditionalItems;
                    _normalGrid.LayoutParameters.Height = height;
                    _expandedContainer.SetMinimumHeight(height);

                    if (value.CanExpand())
                    {
                        content.Click += MyBaseEventVisual_Click;
                    }
                    else
                    {
                        base.Click += MyBaseEventVisual_TappedForOpen;
                    }
                }
            }
        }

        private void MyBaseEventVisual_Click(object sender, EventArgs e)
        {
            ShowFull();
        }

        private void MyBaseEventVisual_TappedForOpen(object sender, EventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item.Item);
            TelemetryExtension.Current?.TrackEvent(TELEMETRY_ON_CLICK_EVENT_NAME);
        }

        private bool _isFullShown = false;
        private EventHandler<CancelEventArgs> _backPressedEventHandler;
        private BareMvvm.Core.Windows.PortableAppWindow _window;

        private bool ShowFull()
        {
            if (Item == null || !Item.CanExpand())
            {
                return false;
            }

            if (_backPressedEventHandler == null)
            {
                try
                {
                    // Wire the back press event
                    _backPressedEventHandler = new WeakEventHandler<CancelEventArgs>(MyBaseEventVisual_BackPressed).Handler;
                    _window = PortableApp.Current.GetCurrentWindow();
                    _window.BackPressed += _backPressedEventHandler;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            if (_expandedContainer.ChildCount == 0)
            {
                _expandedContainer.AddView(GenerateFullContent(Item));
            }

            _isFullShown = true;

            //var dontWait = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
            //{
            //    if (_isFullShown)
            //    {
                    _expandedContainer.Visibility = ViewStates.Visible;
            //    }
            //});

            return true;
        }

        protected override void OnDetachedFromWindow()
        {
            if (_backPressedEventHandler != null && _window != null)
            {
                try
                {
                    _window.BackPressed -= _backPressedEventHandler;
                    _window = null;
                    _backPressedEventHandler = null;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            base.OnDetachedFromWindow();
        }

        private void MyBaseEventVisual_BackPressed(object sender, CancelEventArgs e)
        {
            try
            {
                if (HideFull())
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Returns true if expanded was open and it actually closed it
        /// </summary>
        /// <returns></returns>
        public bool HideFull()
        {
            if (_isFullShown)
            {
                _isFullShown = false;
                _expandedContainer.Visibility = ViewStates.Gone;
                return true;
            }

            return false;
        }

        protected abstract View GenerateContent(DayScheduleItemsArranger.EventItem item);

        protected View GenerateFullContent(DayScheduleItemsArranger.EventItem item)
        {
            LinearLayout sp = new LinearLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent),
                Orientation = Orientation.Vertical
            };
            sp.AddView(new MainCalendarItemView(Context)
            {
                Item = item.Item,

                // After opened, we hide this popup, otherwise when the user presses back, it'll close the popup rather than the task
                // Ideally we would implement the back handling as part of the view model like we did for UWP, but for simplicity we're going
                // to leave it like this for now
                AfterOpenedTaskOrEventAction = delegate { HideFull(); }
            });
            if (item.AdditionalItems != null)
            {
                foreach (var i in item.AdditionalItems)
                {
                    sp.AddView(new MainCalendarItemView(Context)
                    {
                        Item = i,
                        AfterOpenedTaskOrEventAction = delegate { HideFull(); }
                    });
                }
            }

            return sp;
        }
    }

    public class MyFullEventItem : MyBaseEventVisual
    {
        public MyFullEventItem(Context context) : base(context)
        {
            IsFullItem = true;
        }

        protected override View GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new FrameLayout(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(0, LayoutParams.MatchParent)
                {
                    Weight = 1
                },
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.schedule_item_rounded_rectangle)
            };

            ViewCompat.SetBackgroundTintList(grid, GetBackgroundColorStateList(item.Item));

            var tb = new TextView(Context)
            {
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                {
                    LeftMargin = ThemeHelper.AsPx(Context, 6),
                    TopMargin = ThemeHelper.AsPx(Context, 6)
                },
                Text = item.Item.Name
            };
            tb.SetTextColor(Color.White);
            if (item.Item.IsComplete)
            {
                tb.SetStrikethrough(true);
            }
            grid.AddView(tb);

            return grid;
        }

        public static Android.Content.Res.ColorStateList GetBackgroundColorStateList(ViewItemTaskOrEvent item)
        {
            return new Android.Content.Res.ColorStateList(new int[][]
            {
                new int[] { }
            },
            new int[]
            {
                item.IsComplete ? new Color(180, 180, 180).ToArgb() : ColorTools.GetColor(item.Class.Color).ToArgb()
            });
        }
    }

    public class MyCollapsedEventItem : MyBaseEventVisual
    {
        public const double WIDTH_OF_COLLAPSED_ITEM = 36;
        public static readonly double SPACING_WITH_NO_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 6;
        public static readonly double SPACING_WITH_ADDITIONAL = WIDTH_OF_COLLAPSED_ITEM + 14;

        public MyCollapsedEventItem(Context context) : base(context) { }

        protected override View GenerateContent(DayScheduleItemsArranger.EventItem item)
        {
            var grid = new FrameLayout(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ThemeHelper.AsPx(Context, WIDTH_OF_COLLAPSED_ITEM), LayoutParams.MatchParent),
                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.schedule_item_rounded_rectangle)
            };

            ViewCompat.SetBackgroundTintList(grid, MyFullEventItem.GetBackgroundColorStateList(item.Item));

            var tb = new TextView(Context)
            {
                //FontSize = 18,
                Gravity = GravityFlags.CenterHorizontal | GravityFlags.Top,
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                {
                    TopMargin = ThemeHelper.AsPx(Context, 6)
                },
                Text = item.Item.Name.Substring(0, 1)
            };
            tb.SetTextColor(Color.White);
            grid.AddView(tb);

            return grid;
        }
    }

    public class MyAdditionalItemsVisual : LinearLayout
    {
        public MyAdditionalItemsVisual(Context context) : base(context)
        {
            base.SetPaddingRelative(ThemeHelper.AsPx(context, 2), 0, 0, 0);
            Visibility = ViewStates.Gone;
        }

        private IEnumerable<ViewItemTaskOrEvent> _additionalItems;
        public IEnumerable<ViewItemTaskOrEvent> AdditionalItems
        {
            get { return _additionalItems; }
            set
            {
                _additionalItems = value;

                if (value == null)
                {
                    base.Visibility = ViewStates.Gone;
                    return;
                }

                foreach (var additional in value)
                {
                    this.AddView(CreateCircle(this, additional));
                }

                base.Visibility = this.ChildCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private View CreateCircle(ViewGroup root, BaseViewItemMegaItem item)
        {
            View view = new View(root.Context)
            {
                Background = ContextCompat.GetDrawable(root.Context, Resource.Drawable.circle),
                LayoutParameters = new LinearLayout.LayoutParams(
                    ThemeHelper.AsPx(Context, 9),
                    ThemeHelper.AsPx(Context, 9))
                {
                    BottomMargin = ThemeHelper.AsPx(Context, 2)
                }
            };

            if (item is ViewItemTaskOrEvent)
            {
                ViewCompat.SetBackgroundTintList(view, MyFullEventItem.GetBackgroundColorStateList(item as ViewItemTaskOrEvent));
            }

            return view;
        }
    }
}