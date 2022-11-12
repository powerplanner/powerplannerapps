using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpSlideView : UwpView<Vx.Views.SlideView, Border>
    {
        private StackPanel _stackpanel;
        private ScrollViewer _scrollViewer;
        private int _position;
        private Border _left => _stackpanel.Children[0] as Border;
        private Border _center => _stackpanel.Children[1] as Border;
        private Border _right => _stackpanel.Children[2] as Border;

        private Vx.Views.DataTemplateHelper.VxDataTemplateComponent _leftComponent => (_left.Child as INativeComponent).Component as Vx.Views.DataTemplateHelper.VxDataTemplateComponent;
        private Vx.Views.DataTemplateHelper.VxDataTemplateComponent _centerComponent => (_center.Child as INativeComponent).Component as Vx.Views.DataTemplateHelper.VxDataTemplateComponent;
        private Vx.Views.DataTemplateHelper.VxDataTemplateComponent _rightComponent => (_right.Child as INativeComponent).Component as Vx.Views.DataTemplateHelper.VxDataTemplateComponent;

        public UwpSlideView()
        {
            _stackpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            for (int i = 0; i < 3; i++)
            {
                _stackpanel.Children.Add(new Border()
                {
                    Width = 0
                });
            }

            _scrollViewer = new ScrollViewer()
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollMode = ScrollMode.Disabled,
                HorizontalSnapPointsType = SnapPointsType.MandatorySingle,
                ZoomMode = ZoomMode.Disabled,
                Opacity = 0
            };
            _scrollViewer.Content = _stackpanel;
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
            _scrollViewer.Loaded += _scrollViewer_Loaded;
            base.View.Child = _scrollViewer;

            base.View.SizeChanged += View_SizeChanged;
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

        private void View_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            _left.Width = e.NewSize.Width;
            _left.Height = e.NewSize.Height;

            _center.Width = e.NewSize.Width;
            _center.Height = e.NewSize.Height;

            _right.Width = e.NewSize.Width;
            _right.Height = e.NewSize.Height;

            CenterWithoutSnap();
        }

        void _scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            CenterWithoutSnap();

            var dontWait = View.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
            {
                try
                {
                    _scrollViewer.Opacity = 1;
                }
                catch { }
            });
        }

        /// <summary>
        /// Returns column width combined with half of each side's spacing (thus the effective column width including padding)
        /// </summary>
        /// <returns></returns>
        private double TotalColumnWidth()
        {
            return View.ActualWidth;
        }

        private int _scrollingPosition;

        void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //as we scroll, we'll dynamically add upcoming/previous items

            //moving next
            // Scrollable - Offset = 0 when scrolled all the way to the right
            if ((_scrollViewer.ScrollableWidth - _scrollViewer.HorizontalOffset) < TotalColumnWidth() * 0.5)
            {
                ShowNextVisual();
            }

            //moving previous
            else if (_scrollViewer.HorizontalOffset < TotalColumnWidth() * 0.5)
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

                //if (actualCurrentIndex != CurrentItemIndex)
                //    UpdateCurrentItemIndex(actualCurrentIndex);

                //int countRemovedFromLeft = 0;

                //// Remove on the left
                //while (CurrentItemIndex > 1)
                //{
                //    if (!removePrevVisual())
                //        break;

                //    countRemovedFromLeft++;
                //}

                //// If we removed from the left, we need to adjust scroll offset
                //if (countRemovedFromLeft > 0)
                //    setWithoutSnap(_scrollViewer.HorizontalOffset - TotalColumnWidth() * countRemovedFromLeft);

                //// Remove on the right
                //int desiredCountOfVisuals = countOfVisuals();
                //while (_stackpanel.Children.Count > desiredCountOfVisuals)
                //{
                //    if (!removeNextVisual())
                //        break;
                //}
            }
        }

        private void ShowNextVisual()
        {
            _position++;

            var oldLeft = _left;

            _stackpanel.Children.Remove(_left);
            _stackpanel.Children.Add(oldLeft);

            _rightComponent.Data = _position + 1;

            SetWithoutSnap(_scrollViewer.HorizontalOffset - TotalColumnWidth());
        }

        private void ShowPreviousVisual()
        {
            _position--;

            var oldRight = _right;

            _stackpanel.Children.Remove(_right);
            _stackpanel.Children.Insert(0, oldRight);

            _leftComponent.Data = _position - 1;

            SetWithoutSnap(_scrollViewer.HorizontalOffset + TotalColumnWidth());
        }

        private void CenterWithoutSnap()
        {
            SetWithoutSnap(base.View.ActualWidth);
        }

        private void SetWithoutSnap(double x)
        {
            if (_scrollViewer.HorizontalOffset == x)
                return;

            _scrollViewer.HorizontalSnapPointsType = SnapPointsType.None;
            SetX(x);
            _scrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
        }
        private void SetX(double x)
        {
            _scrollViewer.ChangeView(x, null, null, true);
        }
    }
}
