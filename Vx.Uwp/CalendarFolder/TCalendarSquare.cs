using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace InterfacesUWP.CalendarFolder
{
    public abstract class TCalendarSquare : MyGrid
    {
        public enum DisplayType
        {
            ThisMonth, ThisMonthSelected, OtherMonth, OtherMonthSelected, Today, TodaySelected
        }

        protected virtual bool AutoInitialize
        {
            get { return true; }
        }

        public TCalendarGrid CalendarGrid { get; private set; }

        public TCalendarView CalendarView
        {
            get { return CalendarGrid.CalendarView; }
        }

        public TCalendarSquare(TCalendarGrid calendarGrid, DateTime date)
        {
            CalendarGrid = calendarGrid;
            Date = date;

            if (AutoInitialize)
                Initialize();
        }

        protected void Initialize()
        {
            //fill the grid contents
            Render(Date);
        }

        protected override void OnMouseDownChanged(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            setColors();
        }

        protected override void OnMouseOverChanged(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            setColors();
        }

        public DateTime Date { get; private set; }

        public bool IsSelected
        {
            set
            {
                if (value)
                {
                    switch (Type)
                    {
                        case DisplayType.OtherMonth:
                            Type = DisplayType.OtherMonthSelected;
                            break;

                        case DisplayType.ThisMonth:
                            Type = DisplayType.ThisMonthSelected;
                            break;

                        case DisplayType.Today:
                            Type = DisplayType.TodaySelected;
                            break;
                    }
                }

                else
                {
                    switch (Type)
                    {
                        case DisplayType.TodaySelected:
                            Type = DisplayType.Today;
                            break;

                        case DisplayType.ThisMonthSelected:
                            Type = DisplayType.ThisMonth;
                            break;

                        case DisplayType.OtherMonthSelected:
                            Type = DisplayType.OtherMonth;
                            break;
                    }
                }
            }
        }

        private DisplayType _type;
        /// <summary>
        /// Must be set after constructing!
        /// </summary>
        public DisplayType Type
        {
            get { return _type; }

            set
            {
                _type = value;

                setColors();
            }
        }

        private void setColors()
        {
            SetBackgroundColor(Type, IsMouseOver, IsMouseDown);
            SetForegroundColor(Type, IsMouseOver, IsMouseDown);
        }

        protected abstract void Render(DateTime date);

        protected abstract void SetBackgroundColor(DisplayType type, bool isMouseHovering, bool isMouseDown);
        protected abstract void SetForegroundColor(DisplayType type, bool isMouseHovering, bool isMouseDown);

        public abstract void InsertItem(int index, IRenderable el);

        protected abstract UIElement removeItem(IRenderable el);
        public void RemoveItem(IRenderable el)
        {
            UIElement removed = removeItem(el);

            if (removed != null)
                UIGenerator.RemoveRenderedElement(el, removed);
        }

        public abstract void ClearItems();
    }
}
