using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Collections;
using ToolsPortable;
using CoreGraphics;
using CoreAnimation;
using InterfacesiOS.Helpers;

namespace InterfacesiOS.Views.Calendar
{
    public abstract class BareUICalendarItemsSourceProvider
    {
        public abstract IEnumerable GetItemsSource(DateTime date);

        public virtual CGColor GetBackgroundColorForDate(DateTime date)
        {
            return null;
        }
    }

    public enum BareUICalendarMonthViewSize
    {
        Compact,
        Full
    }

    public class BareUICalendarMonthView : UIView
    {

        public enum DayType
        {
            ThisMonth,
            PrevMonth,
            NextMonth
        }

        public event EventHandler<DateTime> DateClicked;

        private UILabel _labelMonth;
        private IBareUICalendarDayView[,] _dayViews = new IBareUICalendarDayView[6, 7];
        private BaseBareUIViewItemsSourceAdapter[,] _itemsSourceAdapters = new BaseBareUIViewItemsSourceAdapter[6, 7];

        public virtual float TopPadding => 16;
        public virtual float SpacingAfterTitle => 16;
        public virtual float SpacingAfterDayHeaders => 4;

        private UIView _viewTitle;
        private DayHeadersView _viewDayHeaders;
        private UIView[] _viewRows;

        public DayOfWeek FirstDayOfWeek { get; private set; }

        ~BareUICalendarMonthView()
        {
            System.Diagnostics.Debug.WriteLine("Month view disposed");
        }

        /// <summary>
        /// Assign the Month property to initialize the view
        /// </summary>
        public BareUICalendarMonthView(DayOfWeek firstDayOfWeek)
        {
            FirstDayOfWeek = firstDayOfWeek;

            var title = CreateTitle();
            this.Add(title);
            _viewTitle = title;

            var dayHeaders = CreateDayHeaders();
            this.Add(dayHeaders);
            _viewDayHeaders = dayHeaders;

            _viewRows = new UIView[6];

            for (int i = 0; i < 6; i++)
            {
                var row = CreateRow(out List<IBareUICalendarDayView> createdDays);
                for (int x = 0; x < 7; x++)
                {
                    var dayView = createdDays[x];
                    _dayViews[i, x] = dayView;
                    // Can't store these adapters on the day view items themselves, since that gives the items
                    // a strong reference to the Month view via the method for creating the item views
                    _itemsSourceAdapters[i, x] = CreateItemsSourceAdapter(dayView.ItemsView);

                    if (dayView is UIControl)
                    {
                        (dayView as UIControl).TouchUpInside += new WeakEventHandler(DayView_TouchUpInside).Handler;
                    }
                }

                this.Add(row);
                _viewRows[i] = row;
            }
        }

        public void UpdateAllBackgroundColors()
        {
            if (Provider == null)
            {
                return;
            }

            foreach (var dayView in _dayViews)
            {
                dayView.SetBackgroundColor(Provider.GetBackgroundColorForDate(dayView.Date));
            }
        }

        private void DayView_TouchUpInside(object sender, EventArgs e)
        {
            IBareUICalendarDayView dayView = (IBareUICalendarDayView)sender;
            DateClicked?.Invoke(this, dayView.Date);
        }

        public override CGRect Bounds
        {
            get => base.Bounds;
            set
            {
                // Either Bounds or Frame will be called, so we override both
                UpdateDisplaySize(value);

                base.Bounds = value;
            }
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                UpdateDisplaySize(value);

                base.Frame = value;
            }
        }

        private bool _displayMonth = false;
        public bool DisplayMonth
        {
            get => _displayMonth;
            set
            {
                if (_displayMonth != value)
                {
                    _displayMonth = value;
                    SetNeedsLayout();
                }
            }
        }

        private void UpdateDisplaySize(CGRect newSize)
        {
            if (newSize.Height >= 450)
            {
                DisplaySize = BareUICalendarMonthViewSize.Full;
            }
            else
            {
                DisplaySize = BareUICalendarMonthViewSize.Compact;
            }
        }

        public override void LayoutSubviews()
        {
            nfloat y = TopPadding;

            if (DisplayMonth)
            {
                _viewTitle.Hidden = false;

                var titleSize = _viewTitle.SizeThatFits(this.Frame.Size);
                _viewTitle.Frame = new CGRect(
                    x: 16,
                    y: y,
                    width: this.Frame.Width,
                    height: titleSize.Height);

                y += titleSize.Height + SpacingAfterTitle;
            }

            else
            {
                _viewTitle.Hidden = true;
            }

            var dayHeadersSize = _viewDayHeaders.SizeThatFits(new CGSize(this.Frame.Width, this.Frame.Height));

            _viewDayHeaders.Frame = new CGRect(
                x: 0,
                y: y,
                width: this.Frame.Width,
                height: dayHeadersSize.Height);

            y += dayHeadersSize.Height + SpacingAfterDayHeaders;

            nfloat remainingHeight = this.Frame.Height - y;
            if (remainingHeight > 0)
            {
                nfloat rowHeight = remainingHeight / 6;

                foreach (var row in _viewRows)
                {
                    row.Frame = new CGRect(
                        x: 0,
                        y: y,
                        width: this.Frame.Width,
                        height: rowHeight);

                    y += rowHeight;
                }
            }
        }

        private BareUICalendarItemsSourceProvider _provider;
        public BareUICalendarItemsSourceProvider Provider
        {
            get { return _provider; }
            set
            {
                _provider = value;

                RefreshData();
            }
        }

        private DateTime _month;
        public DateTime Month
        {
            get { return _month; }
            set
            {
                value = DateTools.GetMonth(value);

                if (_month == value)
                {
                    return;
                }

                _month = value;

                OnMonthChanged();
            }
        }

        private IBareUICalendarDayView _currSelectedDayView;
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if ((_selectedDate == null && value == null)
                    || (_selectedDate != null && value != null && _selectedDate.Value == value.Value.Date))
                {
                    return;
                }

                var prevDate = _selectedDate;
                _selectedDate = value?.Date;

                if (_currSelectedDayView != null && prevDate != null)
                {
                    UpdateDay(_currSelectedDayView, prevDate.Value, false);
                    _currSelectedDayView = null;
                }

                OnSelectedDateChanged();
            }
        }

        private BareUICalendarMonthViewSize _displaySize;
        public BareUICalendarMonthViewSize DisplaySize
        {
            get => _displaySize;
            private set
            {
                if (_displaySize != value)
                {
                    _displaySize = value;
                    _viewDayHeaders.UpdateDisplaySize(value);

                    switch (value)
                    {
                        case BareUICalendarMonthViewSize.Compact:
                            _labelMonth.Font = UIFont.PreferredCaption1.Bold();
                            break;

                        case BareUICalendarMonthViewSize.Full:
                            _labelMonth.Font = UIFont.PreferredTitle3;
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    UpdateAllDays();
                }
            }
        }

        private void RefreshData()
        {
            if (Provider == null || Month == DateTime.MinValue)
            {
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                for (int x = 0; x < 7; x++)
                {
                    var adapter = _itemsSourceAdapters[i, x];
                    if (adapter != null)
                    {
                        var date = _dayViews[i, x].Date;
                        adapter.ItemsSource = Provider.GetItemsSource(date);
                    }
                }
            }

            UpdateAllBackgroundColors();
        }

        protected virtual void OnMonthChanged()
        {
            if (_labelMonth != null)
                _labelMonth.Text = Month.ToString("MMMM yyyy");

            UpdateAllDays();

            RefreshData();
        }

        private void UpdateAllDays()
        {
            DateTime[,] array = CalendarArray.Generate(Month, FirstDayOfWeek);

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    DateTime date = array[row, col];
                    IBareUICalendarDayView dayView = _dayViews[row, col];

                    UpdateDay(dayView, date, isSelected: date == SelectedDate);
                }
            }
        }

        private void UpdateDay(IBareUICalendarDayView dayView, DateTime date, bool isSelected)
        {
            DayType dayType;

            if (DateTools.SameMonth(date, Month))
            {
                dayType = DayType.ThisMonth;
            }
            else if (date < Month)
            {
                dayType = DayType.PrevMonth;
            }
            else
            {
                dayType = DayType.NextMonth;
            }

            dayView.UpdateDay(date, dayType, date.Date == DateTime.Today, isSelected, DisplaySize);

            if (isSelected)
            {
                _currSelectedDayView = dayView;
            }
        }

        protected virtual void OnSelectedDateChanged()
        {
            if (SelectedDate == null)
            {
                return;
            }

            DateTime[,] array = CalendarArray.Generate(Month, FirstDayOfWeek);

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    DateTime date = array[row, col];

                    if (date == SelectedDate)
                    {
                        IBareUICalendarDayView dayView = _dayViews[row, col];

                        UpdateDay(dayView, date, isSelected: true);
                        return;
                    }
                }
            }
        }

        protected virtual UIView CreateTitle()
        {
            _labelMonth = new UILabel()
            {
                Font = UIFont.PreferredCaption1.Bold()
            };
            return _labelMonth;
        }

        protected virtual DayHeadersView CreateDayHeaders()
        {
            return new DayHeadersView(FirstDayOfWeek);
        }

        protected class DayHeadersView : UIView
        {
            private nfloat heightOfText;

            public DayHeadersView(DayOfWeek firstDayOfWeek)
            {
                DayOfWeek day = firstDayOfWeek;
                for (int i = 0; i < 7; i++, day++)
                {
                    var label = new UILabel()
                    {
                        Text = DateTools.ToLocalizedString(day).Substring(0, 1).ToUpper(),
                        Font = UIFont.PreferredCaption2,
                        TextAlignment = UITextAlignment.Center
                    };

                    if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                    {
                        label.TextColor = UIColor.Gray;
                    }

                    this.Add(label);
                }

                heightOfText = this.Subviews.First().SizeThatFits(new CGSize(nfloat.MaxValue, nfloat.MaxValue)).Height;
            }

            public override CGSize SizeThatFits(CGSize size)
            {
                // We're always going to ignore the width anyways
                return new CGSize(0, heightOfText);
            }

            public override void LayoutSubviews()
            {
                int total = this.Subviews.Length;
                nfloat each = this.Frame.Width / total;
                for (int i = 0; i < total; i++)
                {
                    this.Subviews[i].Frame = new CGRect(
                        x: each * i,
                        y: 0,
                        width: each,
                        height: heightOfText);
                }
            }

            public void UpdateDisplaySize(BareUICalendarMonthViewSize displaySize)
            {
                UIFont font;

                switch (displaySize)
                {
                    case BareUICalendarMonthViewSize.Compact:
                        font = UIFont.PreferredCaption2;
                        break;

                    case BareUICalendarMonthViewSize.Full:
                        font = UIFont.PreferredCaption1;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                foreach (var label in this.Subviews.OfType<UILabel>())
                {
                    label.Font = font;
                }
            }
        }

        protected virtual UIView CreateRow(out List<IBareUICalendarDayView> createdDays)
        {
            return new RowView(CreateDay, out createdDays);
        }

        private class RowView : UIView
        {
            private static readonly nfloat SEPARATOR_HEIGHT = 0.5f;
            private UIView _separator;
            private UIView[] _days;

            public RowView(Func<UIView> createDay, out List<IBareUICalendarDayView> createdDays)
            {
                _separator = new UIView()
                {
                    BackgroundColor = UIColorCompat.SystemGray2Color
                };
                this.Add(_separator);

                _days = new UIView[7];
                createdDays = new List<IBareUICalendarDayView>();

                for (int i = 0; i < 7; i++)
                {
                    UIView dayView = createDay();

                    if (!(dayView is IBareUICalendarDayView))
                    {
                        throw new InvalidOperationException("CreateDay must return a UIView that implements IBareUICalendarDayView");
                    }
                    createdDays.Add(dayView as IBareUICalendarDayView);
                    _days[i] = dayView;

                    this.Add(dayView);
                }
            }

            public override void LayoutSubviews()
            {
                _separator.Frame = new CGRect(
                    x: 0,
                    y: 0,
                    width: this.Frame.Width,
                    height: SEPARATOR_HEIGHT);

                nfloat colWidth = this.Frame.Width / _days.Length;
                for (int i = 0; i < _days.Length; i++)
                {
                    _days[i].Frame = new CGRect(
                        x: colWidth * i,
                        y: 0,
                        width: colWidth,
                        height: this.Frame.Height);
                }
            }
        }

        protected virtual UIControl CreateDay()
        {
            return new BareUICalendarDayView();
        }

        /// <summary>
        /// The items source adapter for the individal day items
        /// </summary>
        /// <param name="stackView"></param>
        /// <returns></returns>
        protected virtual BaseBareUIViewItemsSourceAdapter CreateItemsSourceAdapter(UIView itemsView)
        {
            return new BareUIViewItemsSourceAdapter(itemsView, DefaultItemView);
        }

        private UIView DefaultItemView(object item)
        {
            return new BareUIEllipseView()
            {
                FillColor = GetColorForItem(item),
                UserInteractionEnabled = false
            };
        }

        protected virtual CGColor GetColorForItem(object item)
        {
            return UIColor.Gray.CGColor;
        }
    }

    public interface IBareUICalendarDayView
    {
        DateTime Date { get; }
        UIView ItemsView { get; }

        void UpdateDay(DateTime date, BareUICalendarMonthView.DayType dayType, bool isToday, bool isSelected, BareUICalendarMonthViewSize displaySize);

        void SetBackgroundColor(CGColor color);
    }

    public class BareUICalendarDayView : UIControl, IBareUICalendarDayView
    {
        private BareUIEllipseView _backgroundCircle;
        private UILabel _labelDay;
        private BareUICalendarCircleItemsView _itemsView;

        public BareUICalendarDayView()
        {
            _backgroundCircle = new BareUIEllipseView()
            {
                AspectRatio = BareUIEllipseView.AspectRatios.Circle,
                UserInteractionEnabled = false
            };
            this.Add(_backgroundCircle);

            _labelDay = new UILabel()
            {
                Font = UIFont.PreferredCaption1,
                TextAlignment = UITextAlignment.Center
            };
            this.Add(_labelDay);

            _itemsView = new BareUICalendarCircleItemsView();
            this.Add(_itemsView);
        }

        private static readonly nfloat SPACING_AFTER_CIRCLE = 4;

        private nfloat GetSpacing()
        {
            switch (_displaySize)
            {
                case BareUICalendarMonthViewSize.Compact:
                    return 3;

                case BareUICalendarMonthViewSize.Full:
                    return 4;

                default:
                    throw new NotImplementedException();
            }
        }

        private nfloat GetCircleHeight()
        {
            switch (_displaySize)
            {
                case BareUICalendarMonthViewSize.Compact:
                    return 24;

                case BareUICalendarMonthViewSize.Full:
                    return 35;

                default:
                    throw new NotImplementedException();
            }
        }

        private BareUICalendarMonthViewSize _displaySize;

        public override void LayoutSubviews()
        {
            var circleHeight = GetCircleHeight();
            var spacing = GetSpacing();

            _backgroundCircle.Frame = new CGRect(
                x: 0,
                y: spacing,
                width: this.Frame.Width,
                height: circleHeight);

            _labelDay.Frame = new CGRect(
                x: 0,
                y: spacing,
                width: this.Frame.Width,
                height: circleHeight);

            nfloat y = spacing + circleHeight + spacing;
            nfloat remainingHeight = this.Frame.Height - y;
            remainingHeight = remainingHeight >= 0 ? remainingHeight : 0;

            _itemsView.Frame = new CGRect(
                x: 0,
                y: y,
                width: this.Frame.Width,
                height: remainingHeight);

            if (_backgroundLayer != null)
            {
                _backgroundLayer.Path = CGPath.FromRect(new CGRect(
                    x: 0,
                    y: 0,
                    width: this.Frame.Width,
                    height: this.Frame.Height));
            }
        }

        public DateTime Date { get; private set; }

        public UIView ItemsView => _itemsView;

        public void UpdateDay(DateTime date, BareUICalendarMonthView.DayType dayType, bool isToday, bool isSelected, BareUICalendarMonthViewSize displaySize)
        {
            Date = date.Date;

            _labelDay.Text = date.Day.ToString();

            if (_displaySize != displaySize)
            {
                _displaySize = displaySize;
                _itemsView.DisplaySize = displaySize;

                switch (displaySize)
                {
                    case BareUICalendarMonthViewSize.Compact:
                        _labelDay.Font = UIFont.PreferredCaption1;
                        break;

                    case BareUICalendarMonthViewSize.Full:
                        _labelDay.Font = UIFont.PreferredBody;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                SetNeedsLayout();
            }

            if (dayType == BareUICalendarMonthView.DayType.NextMonth || dayType == BareUICalendarMonthView.DayType.PrevMonth)
            {
                _labelDay.TextColor = UIColorCompat.SystemGray2Color;
            }
            else if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                _labelDay.TextColor = UIColor.Gray;
            }
            else
            {
                _labelDay.TextColor = UIColorCompat.LabelColor;
            }

            if (isSelected)
            {
                _backgroundCircle.FillColor = this.TintColor.CGColor;
                _labelDay.TextColor = UIColor.White;
            }
            else if (isToday)
            {
                _backgroundCircle.FillColor = UIColor.DarkGray.CGColor;
                _labelDay.TextColor = UIColor.White;
            }
            else
            {
                _backgroundCircle.FillColor = UIColor.Clear.CGColor;
            }
        }

        private CAShapeLayer _backgroundLayer;
        public void SetBackgroundColor(CGColor color)
        {
            if (color == null)
            {
                if (_backgroundLayer != null)
                {
                    _backgroundLayer.RemoveFromSuperLayer();
                    _backgroundLayer = null;
                }
                return;
            }

            if (_backgroundLayer == null)
            {
                _backgroundLayer = new CAShapeLayer();
                base.Layer.AddSublayer(_backgroundLayer);
            }
            _backgroundLayer.FillColor = color;
        }
    }

    public class BareUICalendarCircleItemsView : UIView
    {
        public BareUICalendarCircleItemsView()
        {
            UserInteractionEnabled = false;
        }

        private BareUICalendarMonthViewSize _displaySize;
        public BareUICalendarMonthViewSize DisplaySize
        {
            get => _displaySize;
            set
            {
                if (_displaySize != value)
                {
                    _displaySize = value;
                    SetNeedsLayout();
                }
            }
        }

        public nfloat GetCircleSize()
        {
            switch (DisplaySize)
            {
                case BareUICalendarMonthViewSize.Compact:
                    return 5;

                case BareUICalendarMonthViewSize.Full:
                    return 8;

                default:
                    throw new NotImplementedException();
            }
        }

        public nfloat GetSpacing()
        {
            switch (DisplaySize)
            {
                case BareUICalendarMonthViewSize.Compact:
                    return 2;

                case BareUICalendarMonthViewSize.Full:
                    return 4;

                default:
                    throw new NotImplementedException();
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            int count = Subviews.Length;
            if (count == 0 || size.Width == 0)
            {
                return new CGSize();
            }

            nfloat totalSpacing = (count - 1) * GetSpacing();
            nfloat totalWidth = count * GetCircleSize() + totalSpacing;
            nfloat itemSize = GetCircleSize();

            if (totalWidth > size.Width)
            {
                if (totalSpacing >= size.Width)
                {
                    itemSize = 1;
                }
                else
                {
                    itemSize = (size.Width - totalSpacing) / count;
                    if (itemSize < 1)
                    {
                        itemSize = 1;
                    }
                }

                return new CGSize(size.Width, itemSize);
            }
            else
            {
                return new CGSize(totalWidth, itemSize);
            }
        }

        public override void LayoutSubviews()
        {
            var finalSize = SizeThatFits(this.Frame.Size);
            if (finalSize.Width == 0)
            {
                return;
            }

            nfloat itemSize = finalSize.Height;

            nfloat x = 0;
            nfloat y = 0;

            if (itemSize < GetCircleSize())
            {
                y = (GetCircleSize() - itemSize) / 2;
            }

            if (finalSize.Width < this.Frame.Width)
            {
                x = (this.Frame.Width - finalSize.Width) / 2;
            }

            foreach (var subview in Subviews)
            {
                subview.Frame = new CGRect(
                    x: x,
                    y: y,
                    width: itemSize,
                    height: itemSize);

                x += itemSize + GetSpacing();
            }
        }
    }
}