using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Vx.Uwp.Views
{
    public class UwpSlideView : UwpView<Vx.Views.SlideView, UwpSlideView.MySlideView>
    {
        private int _position;
        private Border _left => View.ThreeViews.Children[0] as Border;
        private Border _center => View.ThreeViews.Children[1] as Border;
        private Border _right => View.ThreeViews.Children[2] as Border;

        private Vx.Views.DataTemplateHelper.VxDataTemplateComponent _leftComponent => (_left.Child as INativeComponent).Component as Vx.Views.DataTemplateHelper.VxDataTemplateComponent;
        private Vx.Views.DataTemplateHelper.VxDataTemplateComponent _centerComponent => (_center.Child as INativeComponent).Component as Vx.Views.DataTemplateHelper.VxDataTemplateComponent;
        private Vx.Views.DataTemplateHelper.VxDataTemplateComponent _rightComponent => (_right.Child as INativeComponent).Component as Vx.Views.DataTemplateHelper.VxDataTemplateComponent;

        public UwpSlideView()
        {
            View.ScrollViewer.ViewChanged += _scrollViewer_ViewChanged;
        }

        public class MySlideView : Panel
        {
            public bool IgnoreChangeView { get; set; }

            public double? DesiredHorizontalOffset { get; set; }

            public ScrollViewer ScrollViewer { get; } = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollMode = ScrollMode.Disabled,
                HorizontalSnapPointsType = SnapPointsType.MandatorySingle,
                ZoomMode = ZoomMode.Disabled
            };

            public MyThreeViews ThreeViews { get; } = new MyThreeViews();

            public MySlideView()
            {
                ScrollViewer.Content = ThreeViews;
                Children.Add(ScrollViewer);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                // Only initialize width on the first load
                if (ThreeViews.ViewportWidth == 0)
                {
                    // Set silently... measure will be called later anyways
                    ThreeViews._viewportWidth = availableSize.Width;
                }

                ScrollViewer.Measure(availableSize);
                return availableSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                ThreeViews.ViewportWidth = finalSize.Width;

                IgnoreChangeView = true;
                ScrollViewer.Arrange(new Rect(new Point(), finalSize));
                SetWithoutSnap(finalSize.Width);
                IgnoreChangeView = false;

                return finalSize;
            }

            public void SetWithoutSnap(double x)
            {
                if (ScrollViewer.HorizontalOffset == x)
                    return;

                ScrollViewer.HorizontalSnapPointsType = SnapPointsType.None;
                SetX(x);
                ScrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
            }
            public void SetX(double x)
            {
                DesiredHorizontalOffset = x;
                ScrollViewer.ChangeView(x, null, null, true);
            }
        }

        public class MyThreeViews : Panel, IScrollSnapPointsInfo
        {
            public double _viewportWidth;
            public double ViewportWidth
            {
                get => _viewportWidth;
                set
                {
                    if (value != _viewportWidth)
                    {
                        _viewportWidth = value;
                        InvalidateMeasure();
                    }
                }
            }

            public MyThreeViews()
            {
                for (int i = 0; i < 3; i++)
                {
                    Children.Add(new Border());
                }
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                var childSize = new Size(ViewportWidth, availableSize.Height);
                foreach (var child in Children)
                {
                    child.Measure(childSize);
                }

                return new Size(ViewportWidth * 3, availableSize.Height);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                var childSize = new Size(ViewportWidth, finalSize.Height);
                for (int i = 0; i < Children.Count; i++)
                {
                    double x = ViewportWidth * i;
                    Children[i].Arrange(new Rect(new Point(x, 0), childSize));
                }

                return new Size(ViewportWidth * 3, finalSize.Height);
            }

            public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
            {
                return new List<float>();
            }

            public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
            {
                offset = 0;
                if (orientation == Orientation.Horizontal)
                {
                    return (float)ViewportWidth;
                }
                return 0;
            }

            public bool AreHorizontalSnapPointsRegular => true;

            public bool AreVerticalSnapPointsRegular => true;

            public event EventHandler<object> HorizontalSnapPointsChanged;
            public event EventHandler<object> VerticalSnapPointsChanged;
        }

        private Func<object, Vx.Views.View> _itemTemplate;
        protected override void ApplyProperties(Vx.Views.SlideView oldView, Vx.Views.SlideView newView)
        {
            base.ApplyProperties(oldView, newView);

            if (!object.ReferenceEquals(oldView?.ItemTemplate, newView.ItemTemplate))
            {
                _itemTemplate = i =>
                {
                    if (i == null)
                    {
                        return null;
                    }

                    return newView.ItemTemplate((int)i);
                };

                _left.Child = RenderChildComponent();
                _center.Child = RenderChildComponent();
                _right.Child = RenderChildComponent();

                _position = newView.Position.Value;
                SetPositions();
            }
            else if (_position != newView.Position.Value)
            {
                _position = newView.Position.Value;
                SetPositions();
            }
        }

        private FrameworkElement RenderChildComponent()
        {
            var el = new Vx.Views.DataTemplateHelper.VxDataTemplateComponent
            {
                Template = _itemTemplate
            }.Render();

            el.HorizontalAlignment = HorizontalAlignment.Stretch;
            el.VerticalAlignment = VerticalAlignment.Stretch;

            return el;
        }

        private void SetPositions()
        {
            _leftComponent.Data = _position - 1;
            _centerComponent.Data = _position;
            _rightComponent.Data = _position + 1;
        }

        /// <summary>
        /// Returns column width combined with half of each side's spacing (thus the effective column width including padding)
        /// </summary>
        /// <returns></returns>
        private double TotalColumnWidth()
        {
            return View.ThreeViews.ViewportWidth;
        }


        void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //as we scroll, we'll dynamically add upcoming/previous items
            if (View.DesiredHorizontalOffset != null)
            {

                // If horizontal offset was 350, and then I resize it down to 200, would it adjust automatically? I suppose depends on the view content width...
                // So say horizontal offset was 500 (so total scrollable is 1500) and I resize window down to 100 (so total scrollable becomes 300), horizontal offset would then change as a result
                // Hence I have this following logic...
                if (View.DesiredHorizontalOffset.Value == View.ScrollViewer.HorizontalOffset)
                {
                    View.DesiredHorizontalOffset = null;
                }

                return;
            }

            if (View.IgnoreChangeView)
            {
                return;
            }

            //moving next
            // Scrollable - Offset = 0 when scrolled all the way to the right
            if ((View.ScrollViewer.ScrollableWidth - View.ScrollViewer.HorizontalOffset) < TotalColumnWidth() * 0.5)
            {
                ShowNextVisual();
            }

            //moving previous
            else if (View.ScrollViewer.HorizontalOffset < TotalColumnWidth() * 0.5)
            {
                ShowPreviousVisual();
            }

            // If we've stopped moving
            if (!e.IsIntermediate)
            {
                // Only update current item when we stopped moving
                if (VxView.Position != null && VxView.Position.Value != _position)
                {
                    VxView.Position.ValueChanged?.Invoke(_position);
                }
            }
        }

        private void ShowNextVisual()
        {
            _position++;

            View.ThreeViews.Children.Move(0, 2);

            _rightComponent.Data = _position + 1;

            View.SetWithoutSnap(View.ScrollViewer.HorizontalOffset - TotalColumnWidth());
        }

        private void ShowPreviousVisual()
        {
            _position--;

            View.ThreeViews.Children.Move(2, 0);

            _leftComponent.Data = _position - 1;

            View.SetWithoutSnap(View.ScrollViewer.HorizontalOffset + TotalColumnWidth());
        }
    }
}
