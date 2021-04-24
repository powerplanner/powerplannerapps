//  CAPSPageMenu.swift
//
//  Niklas Fahl
//
//  Copyright (c) 2014 The Board of Trustees of The University of Alabama All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//  Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//
//  Neither the name of the University nor the names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
//  PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//  https://github.com/PageMenu/PageMenu/tree/master/Classes

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;
using System.Threading;
using ToolsPortable;
using System.Threading.Tasks;
using InterfacesiOS.Helpers;

namespace InterfacesiOS.Views
{
    public class CAPSPageMenuController : UIViewController
    {
        public event EventHandler<CAPSPageMenuSelectionChangedEventArgs> SelectionChanged;

        internal CAPSPageMenuConfiguration _configuration;

        private UIScrollView _menuScrollView = new UIScrollView();
        private UIScrollView _controllerScrollView = new UIScrollView();
        internal List<UIViewController> _controllers = new List<UIViewController>();
        private List<CAPSMenuItemView> _menuItems = new List<CAPSMenuItemView>();
        private List<nfloat> _menuItemWidths = new List<nfloat>();

        private nfloat _totalMenuItemWidthIfDifferentWidths = 0;

        private nfloat _startingMenuMargin = 0;
        internal nfloat MenuItemMargin { get; set; } = 0;

        private UIView _selectionIndicatorView = new UIView();

        public int CurrentPageIndex { get; set; } = 0;
        private int _lastPageIndex = 0;

        private bool _currentOrientationIsPortrait = true;
        private bool _didLayoutSubviewsAfterRotation = false;
        private bool _didScrollAlready = false;

        private nfloat _lastControllerScrollViewContentOffset = 0;

        private CAPSPageMenuScrollDirection _lastScrollDirection = CAPSPageMenuScrollDirection.Other;
        private int _startingPageForScroll = 0;
        private bool _didTapMenuItemToScroll = false;

        private List<int> _pagesAddedDictionary = new List<int>();

        private UITapGestureRecognizer _gestureRecognizer;

        public CAPSPageMenuController(UIViewController[] viewControllers, CAPSPageMenuConfiguration configuration = null, int initialPageIndex = 0)
        {
            _configuration = configuration ?? new CAPSPageMenuConfiguration();

            _controllers.AddRange(viewControllers);

            SetUpUserInterface();
            if (_menuScrollView.Subviews.Length == 0)
            {
                ConfigureUserInterface();
            }

            if (initialPageIndex > 0)
            {
                _ = MoveToPageAsync(initialPageIndex);
            }
        }

        private void SetUpUserInterface()
        {
            // Despite setting AutomaticallyAdjustsScrollViewInsets to false, it keeps on inserting the spacing for
            // the status bar. So we'll instead add a dummy scroll view at the beginning which that one will receive the insert,
            // which won't matter
            this.View.Add(new UIScrollView());

            NSDictionary viewsDictionary = new NSDictionary(
                "menuScrollView", _menuScrollView,
                "controllerScrollView", _controllerScrollView);

            // Set up controller scroll view
            _controllerScrollView.PagingEnabled = true;
            _controllerScrollView.AlwaysBounceHorizontal = _configuration.EnableHorizontalBounce;
            _controllerScrollView.Bounces = _configuration.EnableHorizontalBounce;

            this.View.Add(_controllerScrollView);

            // Set up menu scroll view
            this.View.Add(_menuScrollView);

            // Add hairline to menu scroll view
            if (_configuration.AddBottomMenuHairline)
            {
                var menuBottomHairline = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = _configuration.BottomMenuHairlineColor
                };

                this.View.Add(menuBottomHairline);

                this.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[menuBottomHairline]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("menuBottomHairline", menuBottomHairline)));
                this.View.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-{_configuration.MenuHeight}-[menuBottomHairline(0.5)]", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("menuBottomHairline", menuBottomHairline)));
            }

            // Disable scroll bars
            _menuScrollView.ShowsHorizontalScrollIndicator = false;
            _menuScrollView.ShowsVerticalScrollIndicator = false;
            _controllerScrollView.ShowsHorizontalScrollIndicator = false;
            _controllerScrollView.ShowsVerticalScrollIndicator = false;

            // Set background color behind scroll views and for menu scroll view
            this.View.BackgroundColor = _configuration.ViewBackgroundColor;
            _menuScrollView.BackgroundColor = _configuration.ScrollMenuBackgroundColor;

            UpdateFrames();
        }

        private void UpdateFrames()
        {
            // Content paging scroll container
            _controllerScrollView.Frame = new CGRect(
                x: 0,
                y: _configuration.MenuHeight,
                width: this.View.Frame.Width,
                height: this.View.Frame.Height - _configuration.MenuHeight);

            // Configure controller scroll view content size
            _controllerScrollView.ContentSize = new CGSize(
                width: this.View.Frame.Width * _controllers.Count,
                height: 0);

            // Menu scroll container
            _menuScrollView.Frame = new CGRect(
                x: 0,
                y: 0,
                width: this.View.Frame.Width,
                height: _configuration.MenuHeight);

            // Individual pages
            foreach (var view in _controllerScrollView.Subviews)
            {
                int index = _controllers.FindIndex(i => i.View == view);
                if (index != -1)
                {
                    UpdateSubControllerFrame(_controllers[index], index);
                }
            }
        }

        private void ConfigureUserInterface()
        {
            // Add tap gesture recognizer to controller scroll view to recognize menu item selection
            _gestureRecognizer = new UITapGestureRecognizer(new Action(HandleMenuItemTap));
            _gestureRecognizer.NumberOfTapsRequired = 1;
            _gestureRecognizer.NumberOfTouchesRequired = 1;
            _menuScrollView.AddGestureRecognizer(_gestureRecognizer);

            _controllerScrollView.Scrolled += new WeakEventHandler(_controllerScrollView_Scrolled).Handler;
            _controllerScrollView.DecelerationEnded += new WeakEventHandler(_controllerScrollView_DecelerationEnded).Handler;

            // When the user taps the status bar, the scroll view beneath the touch which is closest to the status bar will be scrolled to top,
            // but only if its `scrollsToTop` property is YES, its delegate does not return NO from `shouldScrollViewScrollToTop`, and it is not already at the top.
            // If more than one scroll view is found, none will be scrolled.
            // Disable scrollsToTop for menu and controller scroll views so that iOS finds scroll views within our pages on status bar tap gesture.
            _menuScrollView.ScrollsToTop = false;
            _controllerScrollView.ScrollsToTop = false;

            // Configure menu scroll view
            if (_configuration.UseMenuLikeSegmentedControl)
            {
                _menuScrollView.ScrollEnabled = false;
                _menuScrollView.ContentSize = new CGSize(
                    width: this.View.Frame.Width,
                    height: _configuration.MenuHeight);
                _configuration.MenuMargin = 0;
            }
            else
            {
                _menuScrollView.ContentSize = new CGSize(
                    width: (_configuration.MenuItemWidth + _configuration.MenuMargin) * _controllers.Count + _configuration.MenuMargin,
                    height: _configuration.MenuHeight);
            }

            float index = 0;

            foreach (var controller in _controllers)
            {
                if (index == 0)
                {
                    // Add first two controllers to scrollview and as child view controller
                    controller.ViewWillAppear(true);
                    AddPageAtIndex(0);
                    controller.ViewDidAppear(true);
                }

                // Set up menu item for menu scroll view
                var menuItemFrame = new CGRect();

                if (_configuration.UseMenuLikeSegmentedControl)
                {
                    if (MenuItemMargin > 0)
                    {
                        var marginSum = MenuItemMargin * (_controllers.Count + 1);
                        var menuItemWidth = (this.View.Frame.Width - marginSum) / _controllers.Count;
                        menuItemFrame = new CGRect(
                            x: (MenuItemMargin * (index + 1)) + menuItemWidth * index,
                            y: 0,
                            width: this.View.Frame.Width / _controllers.Count,
                            height: _configuration.MenuHeight);
                    }
                    else
                    {
                        menuItemFrame = new CGRect(
                            x: this.View.Frame.Width / _controllers.Count * index,
                            y: 0,
                            width: this.View.Frame.Width / _controllers.Count,
                            height: _configuration.MenuHeight);
                    }
                }
                else if (_configuration.MenuItemWidthBasedOnTitleTextWidth)
                {
                    var controllerTitle = controller.Title;

                    var titleText = controllerTitle ?? "Menu " + (index + 1);
                    var itemWidthRect = new NSString(titleText).GetBoundingRect(
                        size: new CGSize(1000, 1000),
                        options: NSStringDrawingOptions.UsesLineFragmentOrigin,
                        attributes: new UIStringAttributes()
                        {
                            Font = _configuration.MenuItemFont
                        },
                        context: null);

                    _totalMenuItemWidthIfDifferentWidths += itemWidthRect.Width;
                    _menuItemWidths.Append(itemWidthRect.Width);
                }
                else
                {
                    if (_configuration.CenterMenuItems && index == 0)
                    {
                        _startingMenuMargin = ((this.View.Frame.Width - ((_controllers.Count * _configuration.MenuItemWidth) + (_controllers.Count - 1) * _configuration.MenuMargin)) / 2) -_configuration.MenuMargin;

                        if (_startingMenuMargin < 0)
                        {
                            _startingMenuMargin = 0;
                        }

                        menuItemFrame = new CGRect(
                            x: _startingMenuMargin + _configuration.MenuMargin,
                            y: 0,
                            width: _configuration.MenuItemWidth,
                            height: _configuration.MenuHeight);
                    }
                    else
                    {
                        menuItemFrame = new CGRect(
                            x: _configuration.MenuItemWidth * index + _configuration.MenuMargin * (index + 1) + _startingMenuMargin,
                            y: 0,
                            width: _configuration.MenuItemWidth,
                            height: _configuration.MenuHeight);
                    }
                }

                var menuItemView = new CAPSMenuItemView()
                {
                    Frame = menuItemFrame
                };
                menuItemView.Configure(this, controller, index);

                // Add menu item view to menu scroll view
                _menuScrollView.Add(menuItemView);
                _menuItems.Add(menuItemView);

                index += 1;
            }

            // Set new content size for menu scroll view if needed
            if (_configuration.MenuItemWidthBasedOnTitleTextWidth)
            {
                _menuScrollView.ContentSize = new CGSize(
                    width: (_totalMenuItemWidthIfDifferentWidths + _configuration.MenuMargin) + _controllers.Count * _configuration.MenuMargin,
                    height: _configuration.MenuHeight);
            }

            // Set selected color for title label of selected menu item
            if (_menuItems.Count > 0)
            {
                if (_menuItems[CurrentPageIndex].TitleLabel != null)
                {
                    _menuItems[CurrentPageIndex].TitleLabel.TextColor = _configuration.SelectedMenuItemLabelColor;
                }
            }

            // Configure selection indicator view
            CGRect selectionIndicatorFrame;

            if (_configuration.UseMenuLikeSegmentedControl)
            {
                selectionIndicatorFrame = new CGRect(
                    x: 0,
                    y: _configuration.MenuHeight - _configuration.SelectionIndicatorHeight,
                    width: this.View.Frame.Width / _controllers.Count,
                    height: _configuration.SelectionIndicatorHeight);
            }
            else if (_configuration.MenuItemWidthBasedOnTitleTextWidth)
            {
                selectionIndicatorFrame = new CGRect(
                    x: _configuration.MenuMargin,
                    y: _configuration.MenuHeight - _configuration.SelectionIndicatorHeight,
                    width: _configuration.MenuItemWidth,
                    height: _configuration.SelectionIndicatorHeight);
            }
            else
            {
                if (_configuration.CenterMenuItems)
                {
                    selectionIndicatorFrame = new CGRect(
                        x: _startingMenuMargin + _configuration.MenuMargin,
                        y: _configuration.MenuHeight - _configuration.SelectionIndicatorHeight,
                        width: _configuration.MenuItemWidth,
                        height: _configuration.SelectionIndicatorHeight);
                }
                else
                {
                    selectionIndicatorFrame = new CGRect(
                        x: _configuration.MenuMargin,
                        y: _configuration.MenuHeight - _configuration.SelectionIndicatorHeight,
                        width: _configuration.MenuItemWidth,
                        height: _configuration.SelectionIndicatorHeight);
                }
            }

            _selectionIndicatorView = new UIView(selectionIndicatorFrame);
            _selectionIndicatorView.BackgroundColor = _configuration.SelectionIndicatorColor;
            _menuScrollView.AddSubview(_selectionIndicatorView);
        }

        private void _controllerScrollView_Scrolled(object sender, EventArgs e)
        {
            if (!_didLayoutSubviewsAfterRotation)
            {
                var scrollView = _controllerScrollView;

                if (scrollView.ContentOffset.X >= 0 && scrollView.ContentOffset.X <= (_controllers.Count - 1) * this.View.Frame.Width)
                {
                    if ((_currentOrientationIsPortrait && UIApplication.SharedApplication.StatusBarOrientation.IsPortrait()) || (!_currentOrientationIsPortrait && UIApplication.SharedApplication.StatusBarOrientation.IsLandscape()))
                    {
                        // Check if scroll direction changed
                        if (!_didTapMenuItemToScroll)
                        {
                            if (_didScrollAlready)
                            {
                                var newScrollDirection = CAPSPageMenuScrollDirection.Other;

                                if (_startingPageForScroll * this.View.Frame.Width > scrollView.ContentOffset.X)
                                {
                                    newScrollDirection = CAPSPageMenuScrollDirection.Right;
                                }
                                else if (_startingPageForScroll * this.View.Frame.Width < scrollView.ContentOffset.X)
                                {
                                    newScrollDirection = CAPSPageMenuScrollDirection.Left;
                                }

                                if (newScrollDirection != CAPSPageMenuScrollDirection.Other)
                                {
                                    if (_lastScrollDirection != newScrollDirection)
                                    {
                                        int index = newScrollDirection == CAPSPageMenuScrollDirection.Left ? CurrentPageIndex + 1 : CurrentPageIndex - 1;

                                        if (index >= 0 && index < _controllers.Count)
                                        {
                                            // Check dictionary if page was already added
                                            if (!_pagesAddedDictionary.Contains(index))
                                            {
                                                AddPageAtIndex(index);
                                                _pagesAddedDictionary.Add(index);
                                            }
                                        }
                                    }
                                }

                                _lastScrollDirection = newScrollDirection;
                            }

                            if (!_didScrollAlready)
                            {
                                if (_lastControllerScrollViewContentOffset > scrollView.ContentOffset.X)
                                {
                                    if (CurrentPageIndex != _controllers.Count - 1)
                                    {
                                        // Add page to the left of current page
                                        int index = CurrentPageIndex - 1;

                                        if (!_pagesAddedDictionary.Contains(index) && index < _controllers.Count && index >= 0)
                                        {
                                            AddPageAtIndex(index);
                                            _pagesAddedDictionary.Add(index);
                                        }

                                        _lastScrollDirection = CAPSPageMenuScrollDirection.Right;
                                    }
                                }
                                else if (_lastControllerScrollViewContentOffset < scrollView.ContentOffset.X)
                                {
                                    if (CurrentPageIndex != 0)
                                    {
                                        // Add page to the right of current page
                                        int index = CurrentPageIndex + 1;

                                        if (!_pagesAddedDictionary.Contains(index) && index < _controllers.Count && index >= 0)
                                        {
                                            AddPageAtIndex(index);
                                            _pagesAddedDictionary.Add(index);
                                        }

                                        _lastScrollDirection = CAPSPageMenuScrollDirection.Left;
                                    }
                                }

                                _didScrollAlready = true;
                            }

                            _lastControllerScrollViewContentOffset = scrollView.ContentOffset.X;
                        }

                        nfloat ratio = 1;

                        // Calculate ratio between scroll views
                        ratio = (_menuScrollView.ContentSize.Width - this.View.Frame.Width) / (_controllerScrollView.ContentSize.Width - this.View.Frame.Width);

                        if (_menuScrollView.ContentSize.Width > this.View.Frame.Width)
                        {
                            var offset = _menuScrollView.ContentOffset;
                            offset.X = _controllerScrollView.ContentOffset.X * ratio;
                            offset.Y = 0;
                            _menuScrollView.SetContentOffset(offset, animated: false);
                        }

                        // Calculate current page
                        nfloat width = this.View.Frame.Size.Width;
                        int page = (int)((_controllerScrollView.ContentOffset.X + (0.5 * width)) / width);
                        if (page < 0)
                        {
                            page = 0;
                        }
                        else if (page >= _controllers.Count)
                        {
                            page = _controllers.Count - 1;
                        }

                        // Update page if changed
                        if (page != CurrentPageIndex)
                        {
                            _lastPageIndex = CurrentPageIndex;
                            CurrentPageIndex = page;

                            if (!_pagesAddedDictionary.Contains(page))
                            {
                                AddPageAtIndex(page);
                                _pagesAddedDictionary.Add(page);
                            }

                            if (!_didTapMenuItemToScroll)
                            {
                                // Add last page to pages dictionary to make sure it gets removed after scrolling
                                if (!_pagesAddedDictionary.Contains(_lastPageIndex))
                                {
                                    _pagesAddedDictionary.Add(_lastPageIndex);
                                }

                                // Make sure only up to 3 page views are in memory when fast scrolling, otherwise there should only be one in memory
                                int indexLeftTwo = page - 2;
                                if (_pagesAddedDictionary.Contains(indexLeftTwo))
                                {
                                    _pagesAddedDictionary.Remove(indexLeftTwo);
                                    RemovePageAtIndex(indexLeftTwo);
                                }

                                int indexRightTwo = page + 2;
                                if (_pagesAddedDictionary.Contains(indexRightTwo))
                                {
                                    _pagesAddedDictionary.Remove(indexRightTwo);
                                    RemovePageAtIndex(indexRightTwo);
                                }
                            }
                        }

                        // Move selection indicator view when swiping
                        MoveSelectionIndicator(page);
                    }
                }
                else
                {
                    nfloat ratio = 1;

                    ratio = (_menuScrollView.ContentSize.Width - this.View.Frame.Width) / (_controllerScrollView.ContentSize.Width - this.View.Frame.Width);

                    if (_menuScrollView.ContentSize.Width > this.View.Frame.Width)
                    {
                        var offset = _menuScrollView.ContentOffset;
                        offset.X = _controllerScrollView.ContentOffset.X * ratio;
                        offset.Y = 0;
                        _menuScrollView.SetContentOffset(offset, animated: false);
                    }
                }
            }
            else
            {
                _didLayoutSubviewsAfterRotation = false;

                // Move selection indicator view when swiping
                MoveSelectionIndicator(CurrentPageIndex);
            }
        }

        private void _controllerScrollView_DecelerationEnded(object sender, EventArgs e)
        {
            EndScrolling();
        }

        private void ScrollViewDidEndTapScrollingAnimation()
        {
            EndScrolling();
        }

        private void EndScrolling()
        {
            // View has been disposed
            if (_controllers.Count == 0)
            {
                return;
            }

            // Call DidMoveToPage delegate function
            var currentController = _controllers[CurrentPageIndex];
            SelectionChanged?.Invoke(this, new CAPSPageMenuSelectionChangedEventArgs(currentController, CurrentPageIndex));

            // Remove all but current page after decelerating
            foreach (var page in _pagesAddedDictionary)
            {
                if (page != CurrentPageIndex)
                {
                    RemovePageAtIndex(page);
                }
            }

            _didScrollAlready = false;
            _startingPageForScroll = CurrentPageIndex;

            // Empty out pages in dictionary
            _pagesAddedDictionary.Clear();
        }

        private CancellationTokenSource _tapTimerCancellationSource;
        private async void HandleMenuItemTap()
        {
            var tappedPoint = _gestureRecognizer.LocationInView(_menuScrollView);

            if (tappedPoint.Y < _configuration.MenuHeight)
            {
                // Calculate tapped page
                int itemIndex = 0;

                if (_configuration.UseMenuLikeSegmentedControl)
                {
                    itemIndex = (int)(tappedPoint.X / (this.View.Frame.Width / _controllers.Count));
                }
                else if (_configuration.MenuItemWidthBasedOnTitleTextWidth)
                {
                    // Base case being first item
                    nfloat menuItemLeftBound = 0;
                    nfloat menuItemRightBound = _menuItemWidths[0] + _configuration.MenuMargin + (_configuration.MenuMargin / 2);

                    if (!(tappedPoint.X >= menuItemLeftBound && tappedPoint.X <= menuItemRightBound))
                    {
                        for (int i = 1; i <= _controllers.Count - 1; i++)
                        {
                            menuItemLeftBound = menuItemRightBound + 1;
                            menuItemRightBound = menuItemLeftBound + _menuItemWidths[i] + _configuration.MenuMargin;

                            if (tappedPoint.X >= menuItemLeftBound && tappedPoint.X <= menuItemRightBound)
                            {
                                itemIndex = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    nfloat rawItemIndex = ((tappedPoint.X - _startingMenuMargin) - _configuration.MenuMargin / 2) / (_configuration.MenuMargin + _configuration.MenuItemWidth);

                    // Prevent moving to first item when tapping left to first item
                    if (rawItemIndex < 0)
                    {
                        itemIndex = -1;
                    }
                    else
                    {
                        itemIndex = (int)rawItemIndex;
                    }
                }

                await MoveToPageAsync(itemIndex);
            }
        }

        private void MoveSelectionIndicator(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < _controllers.Count)
            {
                UIView.Animate(0.15, delegate
                {
                    nfloat selectionIndicatorWidth = this._selectionIndicatorView.Frame.Width;
                    nfloat selectionIndicatorX = 0;

                    if (this._configuration.UseMenuLikeSegmentedControl)
                    {
                        selectionIndicatorX = pageIndex * (this.View.Frame.Width / _controllers.Count);
                        selectionIndicatorWidth = this.View.Frame.Width / _controllers.Count;
                    }
                    else if (_configuration.MenuItemWidthBasedOnTitleTextWidth)
                    {
                        selectionIndicatorWidth = _menuItemWidths[pageIndex];
                        selectionIndicatorX += _configuration.MenuMargin;

                        if (pageIndex > 0)
                        {
                            for (int i = 0; i <= pageIndex - 1; i++)
                            {
                                selectionIndicatorX += _configuration.MenuMargin + _menuItemWidths[i];
                            }
                        }
                    }
                    else
                    {
                        if (_configuration.CenterMenuItems && pageIndex == 0)
                        {
                            selectionIndicatorX = _startingMenuMargin + _configuration.MenuMargin;
                        }
                        else
                        {
                            selectionIndicatorX = _configuration.MenuItemWidth * pageIndex + _configuration.MenuMargin * (pageIndex + 1) + _startingMenuMargin;
                        }
                    }

                    _selectionIndicatorView.Frame = new CGRect(
                        x: selectionIndicatorX,
                        y: _selectionIndicatorView.Frame.Location.Y,
                        width: selectionIndicatorWidth,
                        height: _selectionIndicatorView.Frame.Height);

                    // Switch newly selected menu item title label to selected color and old one to unselected color
                    if (_menuItems.Count > 0)
                    {
                        if (_menuItems[_lastPageIndex].TitleLabel != null && _menuItems[this.CurrentPageIndex].TitleLabel != null)
                        {
                            _menuItems[_lastPageIndex].TitleLabel.TextColor = _configuration.UnselectedMenuItemLabelColor;
                            _menuItems[CurrentPageIndex].TitleLabel.TextColor = _configuration.SelectedMenuItemLabelColor;
                        }
                    }
                });
            }
        }

        private void AddPageAtIndex(int index)
        {
            // Call WillMoveToPage delegate function
            var currentController = _controllers[index];
            // TODO: Add a WillMoveToPage event

            var newVC = _controllers[index];

            newVC.WillMoveToParentViewController(this);

            UpdateSubControllerFrame(newVC, index);

            this.AddChildViewController(newVC);
            _controllerScrollView.Add(newVC.View);
            newVC.DidMoveToParentViewController(this);
        }

        private void UpdateSubControllerFrame(UIViewController controller, int index)
        {
            controller.View.Frame = new CGRect(
                x: this.View.Frame.Width * index,
                y: 0,
                width: this.View.Frame.Width,
                height: this.View.Frame.Height - _configuration.MenuHeight);
        }

        private void RemovePageAtIndex(int index)
        {
            var oldVC = _controllers[index];

            oldVC.WillMoveToParentViewController(null);

            oldVC.View.RemoveFromSuperview();
            oldVC.RemoveFromParentViewController();

            oldVC.DidMoveToParentViewController(null);
        }

        public override void ViewDidLayoutSubviews()
        {
            if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.Unknown)
            {
                _currentOrientationIsPortrait = UIDevice.CurrentDevice.Orientation.IsPortrait() || UIDevice.CurrentDevice.Orientation.IsFlat();
            }

            // If size changed
            if (_controllerScrollView.Frame.Width != this.View.Frame.Width
                || _controllerScrollView.Frame.Height != this.View.Frame.Height - _configuration.MenuHeight)
            {
                _didLayoutSubviewsAfterRotation = true;

                UpdateFrames();

                // Resize menu items if using as segmented control
                if (_configuration.UseMenuLikeSegmentedControl)
                {
                    _menuScrollView.ContentSize = new CGSize(
                        width: this.View.Frame.Width,
                        height: _configuration.MenuHeight);

                    // Resize selectionIndicator bar
                    nfloat selectionIndicatorX = CurrentPageIndex * (this.View.Frame.Width / _controllers.Count);
                    nfloat selectionIndicatorWidth = this.View.Frame.Width / _controllers.Count;

                    // Resize menu items
                    int index = 0;

                    foreach (var item in _menuItems)
                    {
                        item.Frame = new CGRect(
                            x: this.View.Frame.Width / _controllers.Count * index,
                            y: 0,
                            width: this.View.Frame.Width / _controllers.Count,
                            height: _configuration.MenuHeight);

                        item.TitleLabel.Frame = new CGRect(
                            x: 0,
                            y: 0,
                            width: this.View.Frame.Width / _controllers.Count,
                            height: _configuration.MenuHeight);

                        item.MenuItemSeparator.Frame = new CGRect(
                            x: item.Frame.Width - (_configuration.MenuItemSeparatorWidth / 2),
                            y: item.MenuItemSeparator.Frame.Location.Y,
                            width: item.MenuItemSeparator.Frame.Width,
                            height: item.MenuItemSeparator.Frame.Height);

                        index++;
                    }
                }
                else if (_configuration.CenterMenuItems)
                {
                    _startingMenuMargin = ((this.View.Frame.Width - _controllers.Count * _configuration.MenuItemWidth) + (_controllers.Count - 1) * _configuration.MenuMargin) / 2 - _configuration.MenuMargin;

                    if (_startingMenuMargin < 0)
                    {
                        _startingMenuMargin = 0;
                    }

                    nfloat selectionIndicatorX = _configuration.MenuItemWidth * CurrentPageIndex + _configuration.MenuMargin * (CurrentPageIndex + 1) + _startingMenuMargin;
                    _selectionIndicatorView.Frame = new CGRect(
                        x: selectionIndicatorX,
                        y: _selectionIndicatorView.Frame.Location.Y,
                        width: _selectionIndicatorView.Frame.Width,
                        height: _selectionIndicatorView.Frame.Height);

                    // Recalculate frame for menu items if centered
                    int index = 0;

                    foreach (var item in _menuItems)
                    {
                        if (index == 0)
                        {
                            item.Frame = new CGRect(
                                x: _startingMenuMargin + _configuration.MenuMargin,
                                y: 0,
                                width: _configuration.MenuItemWidth,
                                height: _configuration.MenuHeight);
                        }
                        else
                        {
                            item.Frame = new CGRect(
                                x: _configuration.MenuItemWidth * index + _configuration.MenuMargin * (index + 1) + _startingMenuMargin,
                                y: 0,
                                width: _configuration.MenuItemWidth,
                                height: _configuration.MenuHeight);
                        }

                        index++;
                    }
                }

                nfloat xOffset = CurrentPageIndex * this.View.Frame.Width;
                _controllerScrollView.SetContentOffset(new CGPoint(
                    x: xOffset,
                    y: 0),
                    animated: false);

                nfloat ratio = (_menuScrollView.ContentSize.Width - this.View.Frame.Width) / (_controllerScrollView.ContentSize.Width - this.View.Frame.Width);

                if (_menuScrollView.ContentSize.Width > this.View.Frame.Width)
                {
                    var offset = _menuScrollView.ContentOffset;
                    offset.X = _controllerScrollView.ContentOffset.X * ratio;
                    offset.Y = 0;
                    _menuScrollView.SetContentOffset(offset, animated: false);
                }

                // Hsoi 2015-02-05 - Running on iOS 7.1 complained: "'NSInternalInconsistencyException', reason: 'Auto Layout
                // still required after sending -viewDidLayoutSubviews to the view controller. ViewController's implementation
                // needs to send -layoutSubviews to the view to invoke auto layout.'"
                //
                // http://stackoverflow.com/questions/15490140/auto-layout-error
                //
                // Given the SO answer and caveats presented there, we'll call layoutIfNeeded() instead.
                this.View.LayoutIfNeeded();
            }

            base.ViewDidLayoutSubviews();
        }

        private async Task MoveToPageAsync(int index)
        {
            if (index >= 0 && index < _controllers.Count)
            {
                // Update page if changed
                if (index != CurrentPageIndex)
                {
                    _startingPageForScroll = index;
                    _lastPageIndex = CurrentPageIndex;
                    CurrentPageIndex = index;
                    _didTapMenuItemToScroll = true;

                    // Add pages in between current and tapped page if necessary
                    int smallerIndex = _lastPageIndex < CurrentPageIndex ? _lastPageIndex : CurrentPageIndex;
                    int largerIndex = _lastPageIndex > CurrentPageIndex ? _lastPageIndex : CurrentPageIndex;

                    if (smallerIndex + 1 != largerIndex)
                    {
                        for (int i = smallerIndex + 1; i <= largerIndex - 1; i++)
                        {
                            if (!_pagesAddedDictionary.Contains(i))
                            {
                                AddPageAtIndex(i);
                                _pagesAddedDictionary.Add(i);
                            }
                        }
                    }

                    AddPageAtIndex(index);

                    // Add page from which tap is initiated so it can be removed after tap is done
                    _pagesAddedDictionary.Add(_lastPageIndex);
                }

                // Move controller scroll view when tapping menu item
                double duration = _configuration.ScrollAnimationDurationOnMenuItemTap / 1000.0;

                UIView.Animate(duration, delegate
                {
                    nfloat xOffset = index * this.View.Frame.Width;
                    _controllerScrollView.SetContentOffset(new CGPoint(
                        x: xOffset,
                        y: 0), animated: false);
                });

                if (_tapTimerCancellationSource != null)
                {
                    _tapTimerCancellationSource.Cancel();
                }

                _tapTimerCancellationSource = new CancellationTokenSource();

                await System.Threading.Tasks.Task.Delay(_configuration.ScrollAnimationDurationOnMenuItemTap);

                ScrollViewDidEndTapScrollingAnimation();
            }
        }

        public override void DidMoveToParentViewController(UIViewController parent)
        {
            if (parent == null)
            {
                _menuScrollView.RemoveGestureRecognizer(_gestureRecognizer);

                foreach (var c in _controllers)
                {
                    c.View.RemoveFromSuperview();
                    c.RemoveFromParentViewController();
                    c.DidMoveToParentViewController(null);
                }
                _controllers.Clear();
            }

            base.DidMoveToParentViewController(parent);
        }
    }

    public class CAPSPageMenuSelectionChangedEventArgs : EventArgs
    {
        public UIViewController Controller { get; private set; }

        public int PageIndex { get; private set; }

        public CAPSPageMenuSelectionChangedEventArgs(UIViewController controller, int pageIndex)
        {
            Controller = controller;
            PageIndex = pageIndex;
        }
    }

    public class CAPSPageMenuConfiguration
    {
        public float MenuHeight { get; set; } = 34;
        public float MenuMargin { get; set; } = 15;
        public float MenuItemWidth { get; set; } = 111;
        public float SelectionIndicatorHeight { get; set; } = 3;

        /// <summary>
        /// Milliseconds
        /// </summary>
        public int ScrollAnimationDurationOnMenuItemTap { get; set; } = 500;

        public UIColor SelectionIndicatorColor { get; set; } = UIColor.Blue;
        public UIColor SelectedMenuItemLabelColor { get; set; } = UIColor.Blue;
        public UIColor UnselectedMenuItemLabelColor { get; set; } = UIColorCompat.SecondaryLabelColor;
        public UIColor ScrollMenuBackgroundColor { get; set; } = UIColorCompat.SecondarySystemBackgroundColor;
        public UIColor ViewBackgroundColor { get; set; } = UIColorCompat.SystemBackgroundColor;
        public UIColor BottomMenuHairlineColor { get; set; } = UIColorCompat.OpaqueSeparatorColor;
        public UIColor MenuItemSeparatorColor { get; set; } = UIColor.Clear;

        public UIFont MenuItemFont { get; set; } = UIFont.SystemFontOfSize(15);
        public float MenuItemSeparatorPercentageHeight { get; set; } = 0.2f;
        public float MenuItemSeparatorWidth { get; set; } = 0.5f;
        public bool MenuItemSeparatorRoundEdges { get; set; } = false;

        public bool AddBottomMenuHairline { get; set; } = true;
        public bool MenuItemWidthBasedOnTitleTextWidth { get; set; } = false;
        public bool TitleTextSizeBasedOnMenuItemWidth { get; set; } = false;
        public bool UseMenuLikeSegmentedControl { get; set; } = false;
        public bool CenterMenuItems { get; set; } = false;
        public bool EnableHorizontalBounce { get; set; } = true;
        public bool HideTopMenuBar { get; set; } = false;
    }

    public class CAPSMenuItemView : UIView
    {
        public UILabel TitleLabel { get; set; }
        public UIView MenuItemSeparator { get; set; }

        public void SetUpMenuItemView(
            nfloat menuItemWidth,
            float menuScrollViewHeight,
            float indicatorHeight,
            float separatorPercentageHeight, float separatorWidth,
            bool separatorRoundEdges,
            UIColor menuItemSeparatorColor)
        {
            TitleLabel = new UILabel(new CGRect(
                x: 0,
                y: 0,
                width: menuItemWidth,
                height: menuScrollViewHeight - indicatorHeight));

            MenuItemSeparator = new UIView(new CGRect(
                x: menuItemWidth - (separatorWidth / 2),
                y: Math.Floor(menuScrollViewHeight * ((1.0 - separatorPercentageHeight) / 2.0)),
                width: separatorWidth,
                height: Math.Floor(menuScrollViewHeight * separatorPercentageHeight)));
            MenuItemSeparator.BackgroundColor = menuItemSeparatorColor;

            if (separatorRoundEdges)
            {
                MenuItemSeparator.Layer.CornerRadius = MenuItemSeparator.Frame.Width / 2;
            }

            MenuItemSeparator.Hidden = true;

            this.Add(MenuItemSeparator);
            this.Add(TitleLabel);
        }

        public string TitleText
        {
            get { return TitleLabel?.Text; }
            set
            {
                TitleLabel.Text = value;
                TitleLabel.Lines = 0;
                TitleLabel.SizeToFit();
            }
        }

        public void Configure(CAPSPageMenuController pageMenu, UIViewController controller, float index)
        {
            nfloat menuItemWidth;

            if (pageMenu._configuration.UseMenuLikeSegmentedControl)
            {
                if (pageMenu.MenuItemMargin > 0)
                {
                    nfloat marginSum = pageMenu.MenuItemMargin * (pageMenu._controllers.Count + 1);
                    menuItemWidth = (pageMenu.View.Frame.Width - marginSum) / pageMenu._controllers.Count;
                }
                else
                {
                    menuItemWidth = pageMenu.View.Frame.Width / pageMenu._controllers.Count;
                }
            }
            else
            {
                menuItemWidth = pageMenu._configuration.MenuItemWidth;
            }

            this.SetUpMenuItemView(
                menuItemWidth,
                menuScrollViewHeight: pageMenu._configuration.MenuHeight,
                indicatorHeight: pageMenu._configuration.SelectionIndicatorHeight,
                separatorPercentageHeight: pageMenu._configuration.MenuItemSeparatorPercentageHeight,
                separatorWidth: pageMenu._configuration.MenuItemSeparatorWidth,
                separatorRoundEdges: pageMenu._configuration.MenuItemSeparatorRoundEdges,
                menuItemSeparatorColor: pageMenu._configuration.MenuItemSeparatorColor);

            // Configure menu item label font
            TitleLabel.Font = pageMenu._configuration.MenuItemFont;

            TitleLabel.TextAlignment = UITextAlignment.Center;
            TitleLabel.TextColor = pageMenu._configuration.UnselectedMenuItemLabelColor;

            TitleLabel.AdjustsFontSizeToFitWidth = pageMenu._configuration.TitleTextSizeBasedOnMenuItemWidth;

            TitleLabel.Text = controller.Title ?? "Menu " + (index + 1);

            // Add separator between menu items when using as segmented control
            if (pageMenu._configuration.UseMenuLikeSegmentedControl)
            {
                if (index < pageMenu._controllers.Count - 1)
                {
                    MenuItemSeparator.Hidden = false;
                }
            }
        }
    }

    public enum CAPSPageMenuScrollDirection
    {
        Left,
        Right,
        Other
    }
}