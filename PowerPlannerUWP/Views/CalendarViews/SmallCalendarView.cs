using InterfacesUWP;
using InterfacesUWP.CalendarFolder;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PowerPlannerUWP.Views.CalendarViews
{
    public class SmallCalendarView : DefaultSmallCalendarView
    {
        private Dictionary<DateTime, List<SolidColorBrush>> _storedDates = new Dictionary<DateTime, List<SolidColorBrush>>();
        private HashSet<DateTime> _storedHolidays = new HashSet<DateTime>();

        private NotifyCollectionChangedEventHandler _itemsChangedHandler;
        private MyObservableList<BaseViewItemHomeworkExamGrade> _items;
        private MyObservableList<BaseViewItemHomeworkExamGrade> Items
        {
            get
            {
                return _items;
            }

            set
            {
                //stop watching old
                if (_items != null && _itemsChangedHandler != null)
                    _items.CollectionChanged -= _itemsChangedHandler;

                _items = value;

                //if the new list isn't null, watch it for changes
                if (_items != null)
                {
                    _itemsChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(_items_CollectionChanged).Handler;
                    _items.CollectionChanged += _itemsChangedHandler;
                }

                //clears all the items, and if the Items list isn't null, assigns the new items properly
                setAllItems();
                ResetHolidays();
            }
        }

        private SemesterItemsViewGroup _viewModel;
        public SemesterItemsViewGroup ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;

                Items = value?.Items;
            }
        }

        private double _diameterOfCircles = 4;
        public double DiameterOfCircles
        {
            get
            {
                return _diameterOfCircles;
            }

            set
            {
                if (value < 0)
                    _diameterOfCircles = 0;
                else
                    _diameterOfCircles = value;

                //redisplay all circles, since they need to change
                setAllItems();
            }
        }

        void _items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:

                    foreach (var date in e.NewItems.OfType<BaseViewItemHomeworkExamGrade>().Select(i => i.Date.Date).Distinct())
                    {
                        setDay(date);
                    }
                    ResetHolidays();

                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:

                    foreach (var date in e.OldItems.OfType<BaseViewItemHomeworkExamGrade>().Select(i => i.Date.Date).Distinct())
                    {
                        setDay(date);
                    }
                    ResetHolidays();

                    break;

                default:
                    setAllItems();
                    ResetHolidays();
                    break;
            }
        }

        /// <summary>
        /// Clears everything displayed, and then sets all of the items again
        /// </summary>
        private void setAllItems()
        {
            //first clear all the old items
            base.ClearItems();
            _storedDates.Clear();

            if (Items == null)
                return;

            //now walk through the list and add the items
            foreach (var date in Items.Select(i => i.Date.Date).Distinct())
            {
                setDay(date);
            }
        }

        private void ResetHolidays()
        {
            if (Items == null)
            {
                foreach (var h in _storedHolidays)
                {
                    setSquareOverlay(null, h);
                }
                return;
            }

            HashSet<DateTime> currHolidays = new HashSet<DateTime>();

            foreach (var h in Items.OfType<ViewItemHoliday>())
            {
                for (var date = h.Date.Date; date <= h.EndTime; date = date.AddDays(1))
                {
                    currHolidays.Add(date);
                }
            }

            foreach (var oldDate in _storedHolidays.ToArray())
            {
                if (!currHolidays.Remove(oldDate))
                {
                    setSquareOverlay(null, oldDate);
                    _storedHolidays.Remove(oldDate);
                }
            }

            foreach (var newDate in currHolidays)
            {
                if (_storedHolidays.Add(newDate))
                {
                    setSquareOverlay(OVERLAY_BRUSH, newDate);
                }
            }
        }

        private static SolidColorBrush OVERLAY_BRUSH = new SolidColorBrush(Colors.Red)
        {
            Opacity = 0.2
        };

        private void setDay(DateTime date)
        {
            List<SolidColorBrush> colors = new List<SolidColorBrush>();

            foreach (var item in Items)
            {
                //add the current item to the colors list (don't add complete homeworks)
                if (item is BaseViewItemHomeworkExam && item.Date.Date == date && ShouldIncludeItem(item))
                    colors.Add(getBrush(item));
            }

            setSquare(colors, date);
        }

        private static bool ShouldIncludeItem(BaseViewItemHomeworkExamGrade item)
        {
            // If homework and complete, don't include
            if (item is BaseViewItemHomework && (item as BaseViewItemHomework).IsComplete)
                return false;

            // If exam and complete (in the past), don't include
            if (item is ViewItemExam && (item as ViewItemExam).IsComplete)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the brush for the homework item, based on its class color
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private SolidColorBrush getBrush(BaseViewItemHomeworkExamGrade item)
        {
            if (item is ViewItemHomework)
                return new SolidColorBrush(ColorTools.GetColor((item as ViewItemHomework).Class.Color));

            else if (item is ViewItemExam)
                return new SolidColorBrush(ColorTools.GetColor((item as ViewItemExam).Class.Color));

            return Brushes.Black;
        }

        /// <summary>
        /// Sets the date squares to display circles for each color item, for the given date
        /// </summary>
        /// <param name="colorItems"></param>
        private void setSquare(List<SolidColorBrush> colorItems, DateTime date)
        {
            //store it
            _storedDates[date] = colorItems;

            //get all the squares for that date
            List<TCalendarSquare> squares = GetSquares(date);

            //set each square
            for (int i = 0; i < squares.Count; i++)
                setSquare(colorItems, squares[i]);
        }

        protected override void GenerateEventsOnGrid(TCalendarGrid grid, DateTime displayMonth)
        {
            base.GenerateEventsOnGrid(grid, displayMonth);

            Dictionary<DateTime, TCalendarSquare>.Enumerator i = grid.GetSquareEnumerator();

            while (i.MoveNext())
            {
                List<SolidColorBrush> colors;
                if (_storedDates.TryGetValue(i.Current.Key, out colors))
                    setSquare(colors, i.Current.Value);

                if (_storedHolidays.Contains(i.Current.Key))
                    setSquareOverlay(OVERLAY_BRUSH, i.Current.Value);
            }
        }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="colorItems"></param>
        /// <param name="square"></param>
        private void setSquare(List<SolidColorBrush> colorItems, TCalendarSquare square)
        {
            StackPanel sp = (square as DefaultSmallCalendarSquare).GetContent() as StackPanel;


            //if we can get the existing stack panel
            if (sp != null)
            {
                //clear any items in it
                sp.Children.Clear();
            }

            //otherwise we need to create the stack panel
            else
            {
                //make the stack panel, oriented horizontal
                sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Windows.UI.Xaml.Thickness(6),
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom,
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left
                };

                //set it to the left column
                Grid.SetColumn(sp, 0);

                //add the stack panel
                (square as DefaultSmallCalendarSquare).SetContent(sp);
            }

            //now add a circle for each item, in order
            for (int i = 0; i < colorItems.Count; i++)
                sp.Children.Add(new Ellipse()
                {
                    Fill = colorItems[i],
                    Width = _diameterOfCircles,
                    Height = _diameterOfCircles,
                    Margin = new Windows.UI.Xaml.Thickness(0, 0, _diameterOfCircles, 0)
                });
        }

        private void setSquareOverlay(SolidColorBrush overlayBrush, DateTime date)
        {
            //get all the squares for that date
            List<TCalendarSquare> squares = GetSquares(date);

            //set each square
            for (int i = 0; i < squares.Count; i++)
                setSquareOverlay(overlayBrush, squares[i]);
        }

        private void setSquareOverlay(SolidColorBrush overlayBrush, TCalendarSquare square)
        {
            (square as DefaultSmallCalendarSquare).SetBackgroundOverlayColor(overlayBrush);
        }

        protected override TCalendarGrid GenerateCalendarGrid(DateTime displayMonth)
        {
            return new SmallCalendarGrid(this, displayMonth, IsMouseOver);
        }
    }
}
