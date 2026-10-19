using CoreGraphics;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Views;
using PowerPlanneriOS.Helpers;
using System;
using ToolsPortable;
using UIKit;
using Vx.iOS.Views;

namespace PowerPlanneriOS.Views
{
    internal class iOSFloatingAddItemButton : iOSView<FloatingAddItemButton, UIFloatingAddItemView>
    {
        public iOSFloatingAddItemButton() : base(new UIFloatingAddItemView())
        {
            View.OnRequestAddTask += new WeakEventHandler(View_OnRequestAddTask).Handler;
            View.OnRequestAddEvent += new WeakEventHandler(View_OnRequestAddEvent).Handler;
            View.OnRequestAddHoliday += new WeakEventHandler(View_OnRequestAddHoliday).Handler;
        }

        protected override void ApplyProperties(FloatingAddItemButton oldView, FloatingAddItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            View.SupportsAddHoliday = newView.AddHoliday != null;
        }

        private void View_OnRequestAddTask(object sender, EventArgs e)
        {
            VxView?.AddTask?.Invoke();
        }

        private void View_OnRequestAddEvent(object sender, EventArgs e)
        {
            VxView?.AddEvent?.Invoke();
        }

        private void View_OnRequestAddHoliday(object sender, EventArgs e)
        {
            VxView?.AddHoliday?.Invoke();
        }
    }

    internal class UIFloatingAddItemView : UIView
    {
        private static readonly float MainSize = Vx.Views.FloatingActionButton.DefaultSize;
        private const float MiniSize = 40;
        private const float EdgeMargin = 16;
        private const float RowSpacing = 12;
        private const float LabelSpacing = 12;
        private const float SlideDistance = 40;
        private const float OverlayAlpha = 0.9f;
        private const double AnimationDuration = 0.2;

        public event EventHandler OnRequestAddTask;
        public event EventHandler OnRequestAddEvent;
        public event EventHandler OnRequestAddHoliday;

        public bool SupportsAddHoliday { get; set; }

        private readonly UIControl _overlay;
        private readonly Row _taskRow;
        private readonly Row _eventRow;
        private readonly Row _holidayRow;
        private bool _isExpanded;

        public UIFloatingAddItemView()
        {
            _overlay = new UIControl { Alpha = 0, Hidden = true };
            _overlay.TouchUpInside += new WeakEventHandler(Overlay_Tapped).Handler;
            AddSubview(_overlay);

            _holidayRow = CreateRow(R.S("String_AddHoliday"), MiniSize);
            _eventRow = CreateRow(R.S("String_AddEvent"), MiniSize);
            _taskRow = CreateRow(R.S("String_AddTask"), MainSize);

            foreach (var row in new[] { _holidayRow, _eventRow })
            {
                row.Alpha = 0;
                row.Hidden = true;
                row.Transform = CGAffineTransform.MakeTranslation(0, SlideDistance);
            }
            _taskRow.Label.Alpha = 0;

            ApplyThemeColors();
            iOSThemeColorApplier.ThemeChanged += new WeakEventHandler<ThemeColors>(ThemeColorApplier_ThemeChanged).Handler;
        }

        private Row CreateRow(string text, float size)
        {
            var row = new Row(text, size);
            row.TouchUpInside += new WeakEventHandler(Row_Tapped).Handler;
            row.Button.TouchUpInside += new WeakEventHandler(Row_Tapped).Handler;
            AddSubview(row);
            return row;
        }

        private void ThemeColorApplier_ThemeChanged(object sender, ThemeColors e)
        {
            ApplyThemeColors();
        }

        private void ApplyThemeColors()
        {
            var primary = iOSThemeColorApplier.ToUIColor(iOSThemeColorApplier.Current.Primary);
            var foreground = iOSThemeColorApplier.ToUIColor(Vx.Views.Theme.Current.ForegroundColor);

            _overlay.BackgroundColor = iOSThemeColorApplier.ToUIColor(Vx.Views.Theme.Current.BackgroundColor);

            foreach (var row in new[] { _taskRow, _eventRow, _holidayRow })
            {
                row.Button.BackgroundColor = primary;
                row.Label.TextColor = foreground;
            }
        }

        private void Row_Tapped(object sender, EventArgs e)
        {
            if (sender == _taskRow.Button || sender == _taskRow)
            {
                if (_isExpanded)
                {
                    OnRequestAddTask?.Invoke(this, EventArgs.Empty);
                    Collapse();
                }
                else
                {
                    Expand();
                }
            }
            else if (sender == _eventRow.Button || sender == _eventRow)
            {
                OnRequestAddEvent?.Invoke(this, EventArgs.Empty);
                Collapse();
            }
            else if (sender == _holidayRow.Button || sender == _holidayRow)
            {
                OnRequestAddHoliday?.Invoke(this, EventArgs.Empty);
                Collapse();
            }
        }

        private void Overlay_Tapped(object sender, EventArgs e)
        {
            Collapse();
        }

        private void Expand()
        {
            if (_isExpanded)
            {
                return;
            }

            _isExpanded = true;

            _overlay.Hidden = false;
            _eventRow.Hidden = false;
            _holidayRow.Hidden = !SupportsAddHoliday;

            Animate(() =>
            {
                _overlay.Alpha = OverlayAlpha;
                _taskRow.Label.Alpha = 1;

                _eventRow.Alpha = 1;
                _eventRow.Transform = CGAffineTransform.MakeIdentity();

                if (SupportsAddHoliday)
                {
                    _holidayRow.Alpha = 1;
                    _holidayRow.Transform = CGAffineTransform.MakeIdentity();
                }
            }, null);
        }

        private void Collapse()
        {
            if (!_isExpanded)
            {
                return;
            }

            _isExpanded = false;

            Animate(() =>
            {
                _overlay.Alpha = 0;
                _taskRow.Label.Alpha = 0;

                foreach (var row in new[] { _eventRow, _holidayRow })
                {
                    row.Alpha = 0;
                    row.Transform = CGAffineTransform.MakeTranslation(0, SlideDistance);
                }
            }, () =>
            {
                // A new expand may have started before this finished
                if (!_isExpanded)
                {
                    _overlay.Hidden = true;
                    _eventRow.Hidden = true;
                    _holidayRow.Hidden = true;
                }
            });
        }

        private static void Animate(Action animations, Action completed)
        {
            UIView.Animate(
                AnimationDuration,
                0,
                UIViewAnimationOptions.CurveEaseOut | UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.AllowUserInteraction,
                animations,
                completed);
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var hit = base.HitTest(point, uievent);

            // When collapsed, only the plus button is interactive so touches pass through to the content below
            if (!_isExpanded && (hit == null || !hit.IsDescendantOfView(_taskRow.Button)))
            {
                return null;
            }

            return hit;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _overlay.Frame = Bounds;

            // Move the buttons left/up only as far as needed to stay clear of nooks (rounded corners, home indicator, etc.)
            var nookInsets = GetNookInsets();
            var right = Bounds.Width - (nfloat)Math.Max(EdgeMargin, nookInsets.Right);
            nfloat y = Bounds.Height - (nfloat)Math.Max(EdgeMargin, nookInsets.Bottom) - MainSize;
            PlaceRow(_taskRow, right, y);

            // Center the smaller buttons over the main button
            var miniRight = right - (MainSize - MiniSize) / 2;

            y -= RowSpacing + MiniSize;
            PlaceRow(_eventRow, miniRight, y);

            y -= RowSpacing + MiniSize;
            PlaceRow(_holidayRow, miniRight, y);
        }

        public override void SafeAreaInsetsDidChange()
        {
            base.SafeAreaInsetsDidChange();
            SetNeedsLayout();
        }

        /// <summary>
        /// How far this view extends into the window's unsafe area. Uses the window's safe area rather than this view's own
        /// insets, since a view that doesn't reach the screen edge (for example one sitting above the tab bar) reports none.
        /// </summary>
        private UIEdgeInsets GetNookInsets()
        {
            var window = Window;
            if (window == null)
            {
                return UIEdgeInsets.Zero;
            }

            var windowInsets = window.SafeAreaInsets;
            var safeRect = ConvertRectFromView(
                new CGRect(
                    windowInsets.Left,
                    windowInsets.Top,
                    window.Bounds.Width - windowInsets.Left - windowInsets.Right,
                    window.Bounds.Height - windowInsets.Top - windowInsets.Bottom),
                window);

            return new UIEdgeInsets(
                (nfloat)Math.Max(0, safeRect.Top - Bounds.Top),
                (nfloat)Math.Max(0, safeRect.Left - Bounds.Left),
                (nfloat)Math.Max(0, Bounds.Bottom - safeRect.Bottom),
                (nfloat)Math.Max(0, Bounds.Right - safeRect.Right));
        }

        // Uses Bounds/Center rather than Frame since rows are animated with a transform
        private static void PlaceRow(Row row, nfloat right, nfloat y)
        {
            var width = row.DesiredWidth;
            row.Bounds = new CGRect(0, 0, width, row.Size);
            row.Center = new CGPoint(right - width / 2, y + row.Size / 2);
        }

        private class Row : UIControl
        {
            public UIButton Button { get; }
            public UILabel Label { get; }
            public float Size { get; }

            public Row(string text, float size)
            {
                Size = size;

                Label = new UILabel
                {
                    Text = text,
                    TextAlignment = UITextAlignment.Right,
                    Font = UIFont.BoldSystemFontOfSize(UIFont.LabelFontSize)
                };
                AddSubview(Label);

                Button = new UIButton(UIButtonType.System)
                {
                    TintColor = UIColor.White,
                    AccessibilityLabel = text
                };
                Button.SetImage(UIImage.GetSystemImage("plus"), UIControlState.Normal);
                Button.Layer.CornerRadius = size / 2;
                Button.Layer.ShadowColor = UIColor.Black.CGColor;
                Button.Layer.ShadowOpacity = 0.3f;
                Button.Layer.ShadowOffset = new CGSize(0, 2);
                Button.Layer.ShadowRadius = 4;
                AddSubview(Button);
            }

            public nfloat DesiredWidth => Label.SizeThatFits(new CGSize(10000, Size)).Width + LabelSpacing + Size;

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                Button.Frame = new CGRect(Bounds.Width - Size, 0, Size, Size);
                Label.Frame = new CGRect(0, 0, Bounds.Width - Size - LabelSpacing, Size);
            }
        }
    }
}
