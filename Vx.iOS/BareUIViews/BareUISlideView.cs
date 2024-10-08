﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;

namespace InterfacesiOS.Views
{
    public abstract class BareUISlideView<TUIView> : UIScrollView
        where TUIView : UIView
    {
        private TUIView _prevView;
        private TUIView _currView;
        private TUIView _nextView;

        /// <summary>
        /// Overload to postpone initialization. You MUST call <see cref="Initialize"/>. Useful if you need to set some properties before <see cref="CreateView"/> is called.
        /// </summary>
        /// <param name="initialization"></param>
        public BareUISlideView(bool postponeInitialization)
        {
            if (!postponeInitialization)
            {
                Initialize();
            }
        }

        public BareUISlideView()
        {
            Initialize();
        }

        /// <summary>
        /// Only call this if you used the overload constructor to postpone initialization
        /// </summary>
        protected void Initialize()
        {
            this.PagingEnabled = true;
            this.Bounces = false;
            this.ShowsHorizontalScrollIndicator = false;

            _prevView = CreateView();
            _currView = CreateView();
            _nextView = CreateView();

            this.Add(_prevView);
            this.Add(_currView);
            this.Add(_nextView);

            this.Scrolled += new WeakEventHandler(_scrollView_Scrolled).Handler;
        }

        public IEnumerable<TUIView> GetViews()
        {
            yield return _prevView;
            yield return _currView;
            yield return _nextView;
        }

        private nfloat _prevFrameWidth;

        private void _scrollView_Scrolled(object sender, EventArgs e)
        {
            // Don't alter while resizing (happens when rotating from landscape/portrait)
            if (this.Frame.Width != _prevFrameWidth)
            {
                _prevFrameWidth = this.Frame.Width;
                return;
            }

            // Moving to the left
            if (this.ContentOffset.X <= 0)
            {
                if (HasPrevious())
                {
                    // prev becomes curr
                    // curr becomes next
                    // next moves over to prev

                    var newPrev = _nextView;
                    _nextView = _currView;
                    _currView = _prevView;
                    _prevView = newPrev;

                    EarlyOnMovedToPrevious();

                    ConfigureFrames();

                    OnMovedToPrevious();
                    UpdatePrevView(_prevView);
                }
            }

            // Moving to the right
            else if (this.ContentOffset.X >= this.ContentSize.Width - this.Frame.Width)
            {
                if (HasNext())
                {
                    var newNext = _prevView;
                    _prevView = _currView;
                    _currView = _nextView;
                    _nextView = newNext;

                    EarlyOnMovedToNext();

                    ConfigureFrames();

                    OnMovedToNext();
                    UpdateNextView(_nextView);
                }
            }
        }

        private void ConfigureFrames()
        {
            ConfigureContentSize();
            ConfigureViewFrames();

            // And then adjust the offset to move to the new one
            CenterScrollView();
        }

        protected abstract void OnMovedToPrevious();
        protected abstract void OnMovedToNext();
        protected virtual void EarlyOnMovedToPrevious()
        {

        }

        protected virtual void EarlyOnMovedToNext()
        {

        }

        protected abstract void UpdateCurrView(TUIView curr);
        protected abstract void UpdateNextView(TUIView next);
        protected abstract void UpdatePrevView(TUIView prev);

        protected virtual bool HasPrevious()
        {
            return true;
        }

        protected virtual bool HasNext()
        {
            return true;
        }

        private bool _hasDelayedUpdateAllViews;
        public bool DelayUpdates { get; set; }

        public void ApplyDelayedUpdates()
        {
            var orig = DelayUpdates;
            DelayUpdates = false;

            if (_hasDelayedUpdateAllViews)
            {
                UpdateAllViews();
                _hasDelayedUpdateAllViews = false;
            }

            DelayUpdates = orig;
        }

        protected void UpdateAllViews()
        {
            if (DelayUpdates)
            {
                _hasDelayedUpdateAllViews = true;
                return;
            }

            ConfigureFrames();
            if (HasPrevious())
            {
                UpdatePrevView(_prevView);
            }
            UpdateCurrView(_currView);
            if (HasNext())
            {
                UpdateNextView(_nextView);
            }
        }

        private void CenterScrollView()
        {
            this.SetContentOffset(new CoreGraphics.CGPoint(
                x: HasPrevious() ? this.Frame.Width : 0,
                y: this.ContentOffset.Y), animated: false);
        }

        private nfloat _currWidth;
        private nfloat _currHeight;
        public override void LayoutSubviews()
        {
            if (_currWidth != this.Frame.Width || _currHeight != this.Frame.Height)
            {
                _currWidth = this.Frame.Width;
                _currHeight = this.Frame.Height;

                ConfigureFrames();
            }
        }

        private void ConfigureContentSize()
        {
            this.ContentSize = new CoreGraphics.CGSize(
                this.Frame.Width * (1 + (HasPrevious() ? 1 : 0) + (HasNext() ? 1 : 0)),
                this.Frame.Height);
        }

        private void ConfigureViewFrames()
        {
            if (HasPrevious())
            {
                ConfigureViewFrame(_prevView, 0);
            }
            else
            {
                _prevView.Frame = new CoreGraphics.CGRect();
            }
            ConfigureViewFrame(_currView, HasPrevious() ? 1 : 0);
            if (HasNext())
            {
                ConfigureViewFrame(_nextView, HasPrevious() ? 2 : 1);
            }
            else
            {
                _nextView.Frame = new CoreGraphics.CGRect();
            }
        }

        private void ConfigureViewFrame(UIView view, int index)
        {
            view.Frame = new CoreGraphics.CGRect(
                x: this.Frame.Width * index,
                y: 0,
                width: this.Frame.Width,
                height: this.Frame.Height);
        }

        protected abstract TUIView CreateView();
    }
}