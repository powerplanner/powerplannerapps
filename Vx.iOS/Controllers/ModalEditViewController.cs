/*
 * Copyright (C) 2014 
 * Author: Ruben Macias
 * http://sharpmobilecode.com @SharpMobileCode
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * https://github.com/SharpMobileCode/ModalPickerViewController
 * 
 */

using System;
using UIKit;
using CoreGraphics;
using InterfacesiOS.Views;
using Foundation;
using ToolsPortable;
using InterfacesiOS.Helpers;

namespace InterfacesiOS.Controllers
{
    public delegate void ModalPickerDimissedEventHandler(object sender, EventArgs e);

    public class ModalEditViewController : UIViewController
    {
        public event ModalPickerDimissedEventHandler OnModalEditSubmitted;
        const float _headerBarHeight = 40;

        public UIColor HeaderBackgroundColor { get; set; }
        public UIColor HeaderTextColor { get; set; }
        public UIColor CancelButtonColor { get; set; }
        public UIColor DoneButtonColor { get; set; }
        public string HeaderText { get; set; }
        public string DoneButtonText { get; set; }
        public string CancelButtonText { get; set; }

        public UIView ContentView { get; private set; }

        UILabel _headerLabel;
        UIButton _doneButton;
        UIButton _cancelButton;
        UIViewController _parent;
        internal UIView _slidingView;
        internal UIView _dialogView;

        public ModalEditViewController(UIView contentView, string headerText, UIViewController parent)
        {
            HeaderBackgroundColor = UIColorCompat.SystemBackgroundColor;
            HeaderTextColor = UIColorCompat.LabelColor;
            HeaderText = headerText;
            _parent = parent;
            ContentView = contentView;
            DoneButtonText = "Done";
            CancelButtonText = "Cancel";
        }

        private bool m_preparedForModal;
        public void ShowAsModal()
        {
            if (!m_preparedForModal)
            {
                this.TransitioningDelegate = new ModalPickerTransitionDelegate();
                this.ModalPresentationStyle = UIModalPresentationStyle.Custom;
                m_preparedForModal = true;
            }

            _parent.PresentViewController(this, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitializeControls();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            UpdateLayout();
        }

        void InitializeControls()
        {
            View.BackgroundColor = UIColor.Clear;
            _slidingView = new UIView();
            _dialogView = new UIView();

            _headerLabel = new UILabel(new CGRect(0, 0, 320 / 2, 44));
            _headerLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _headerLabel.BackgroundColor = HeaderBackgroundColor;
            _headerLabel.TextColor = HeaderTextColor;
            _headerLabel.Text = HeaderText;
            _headerLabel.TextAlignment = UITextAlignment.Center;

            _cancelButton = UIButton.FromType(UIButtonType.System);
            if (CancelButtonColor != null)
            {
                _cancelButton.SetTitleColor(CancelButtonColor, UIControlState.Normal);
            }
            _cancelButton.BackgroundColor = UIColor.Clear;
            _cancelButton.SetTitle(CancelButtonText, UIControlState.Normal);
            _cancelButton.TouchUpInside += new WeakEventHandler(CancelButtonTapped).Handler;

            _doneButton = UIButton.FromType(UIButtonType.System);
            if (DoneButtonColor != null)
            {
                _doneButton.SetTitleColor(DoneButtonColor, UIControlState.Normal);
            }
            _doneButton.BackgroundColor = UIColor.Clear;
            _doneButton.SetTitle(DoneButtonText, UIControlState.Normal);
            _doneButton.Font = _doneButton.Font.Bold();
            _doneButton.TouchUpInside += new WeakEventHandler(DoneButtonTapped).Handler;

            ContentView.BackgroundColor = UIColorCompat.SystemBackgroundColor;
            _dialogView.AddSubview(ContentView);

            _dialogView.BackgroundColor = HeaderBackgroundColor;

            _dialogView.AddSubview(_headerLabel);
            _dialogView.AddSubview(_cancelButton);
            _dialogView.AddSubview(_doneButton);

            _slidingView.Add(_dialogView);
            Add(_slidingView);

            var backgroundTouchTarget = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            backgroundTouchTarget.TouchUpInside += new WeakEventHandler(DoneButtonTapped).Handler;
            Add(backgroundTouchTarget);
            backgroundTouchTarget.StretchWidth(View);
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|[backgroundTouchTarget][internalView]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                "backgroundTouchTarget", backgroundTouchTarget,
                "internalView", _dialogView)));
        }

        private nfloat? _originalContentViewHeight;

        private void UpdateLayout(bool onRotate = false)
        {
            var buttonSize = new CGSize(71, 30);

            nfloat screenWidth = _parent.View.Frame.Width;
            UIEdgeInsets screenEdgeInsets;
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                screenEdgeInsets = _parent.View.SafeAreaInsets;
            }
            else
            {
                screenEdgeInsets = new UIEdgeInsets(
                    top: UIApplication.SharedApplication.StatusBarFrame.Height,
                    left: 0,
                    bottom: 0,
                    right: 0);
            }
            nfloat screenHeight = _parent.View.Frame.Height;
            nfloat screenHeightWithTopPadding = screenHeight - screenEdgeInsets.Top;

            // Stash the original requested height since we might change it
            if (_originalContentViewHeight == null)
            {
                _originalContentViewHeight = ContentView.Frame.Height;
            }

            // Content view's height can be reduced in order to fit on screen
            // We include padding for the bottom inserts too
            nfloat contentViewHeight = _originalContentViewHeight.Value;
            if (contentViewHeight + _headerBarHeight + screenEdgeInsets.Bottom > screenHeightWithTopPadding)
            {
                contentViewHeight = screenHeightWithTopPadding - _headerBarHeight - screenEdgeInsets.Bottom;
            }

            // Dialog view is the dialog with done/cancel and the content view
            var dialogViewSize = new CGSize(screenWidth, contentViewHeight + _headerBarHeight + screenEdgeInsets.Bottom);

            _dialogView.Frame = new CGRect(
                0,
                screenHeight - dialogViewSize.Height,
                dialogViewSize.Width,
                dialogViewSize.Height);

            // We accomodate for left and right edge inserts here (other inserts already taken into account)
            _cancelButton.Frame = new CGRect(
                screenEdgeInsets.Left + 10,
                7,
                buttonSize.Width,
                buttonSize.Height);

            _doneButton.Frame = new CGRect(
                _dialogView.Frame.Width - buttonSize.Width - 10 - screenEdgeInsets.Right,
                7,
                buttonSize.Width,
                buttonSize.Height);

            _headerLabel.Frame = new CGRect(
                _cancelButton.Frame.Right + 10,
                4,
                _doneButton.Frame.Left - _cancelButton.Frame.Right - 20,
                35);

            ContentView.Frame = new CGRect(
                screenEdgeInsets.Left,
                _headerBarHeight,
                _dialogView.Frame.Width - screenEdgeInsets.Left - screenEdgeInsets.Right,
                contentViewHeight);
        }

        void DoneButtonTapped(object sender, EventArgs e)
        {
            DismissViewController(true, null);
            OnModalEditSubmitted?.Invoke(this, e);
        }

        void CancelButtonTapped(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            if (InterfaceOrientation == UIInterfaceOrientation.Portrait ||
                InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
                InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
            {
                UpdateLayout(true);
                View.SetNeedsDisplay();
            }
        }
    }

    public class ModalDatePickerViewController : ModalEditViewController
    {
        public UIDatePicker DatePicker => ContentView as UIDatePicker;

        public ModalDatePickerViewController(string headerText, UIViewController parent)
            : base(CreateDatePicker(), headerText, parent) { }

        private static UIDatePicker CreateDatePicker()
        {
            return new UIDatePicker(CGRect.Empty);
        }
    }

    public class ModalPickerViewController : ModalEditViewController
    {
        public UIPickerView PickerView => ContentView as UIPickerView;

        public ModalPickerViewController(string headerText, UIViewController parent)
            : base(CreatePickerView(), headerText, parent) { }

        private static UIPickerView CreatePickerView()
        {
            return new UIPickerView(CGRect.Empty);
        }
    }

    public class ModalPickerTransitionDelegate : UIViewControllerTransitioningDelegate
    {
        public ModalPickerTransitionDelegate()
        {
        }

        public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForPresentedController(UIViewController presented, UIViewController presenting, UIViewController source)
        {
            var controller = new ModalPickerAnimatedTransitioning();
            controller.IsPresenting = true;

            return controller;
        }

        public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForDismissedController(UIViewController dismissed)
        {
            var controller = new ModalPickerAnimatedDismissed();
            controller.IsPresenting = false;

            return controller;
        }

        public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForPresentation(IUIViewControllerAnimatedTransitioning animator)
        {
            return null;
        }

        public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForDismissal(IUIViewControllerAnimatedTransitioning animator)
        {
            return null;
        }

    }

    internal class ModalPickerAnimatedDismissed : UIViewControllerAnimatedTransitioning
    {
        public bool IsPresenting { get; set; }
        float _transitionDuration = 0.4f;

        public ModalPickerAnimatedDismissed()
        {
        }

        public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext)
        {
            return _transitionDuration;
        }

        public override void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
        {

            var fromViewController = transitionContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey) as ModalEditViewController;
            var toViewController = transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);

            var screenBounds = UIScreen.MainScreen.Bounds;
            var fromFrame = fromViewController._slidingView.Frame;

            UIView.AnimateNotify(_transitionDuration,
                                 () =>
                                 {
                                     //toViewController.View.Alpha = 1.0f;
                                     fromViewController.View.BackgroundColor = UIColor.Clear;

                                     switch (fromViewController.InterfaceOrientation)
                                     {
                                         case UIInterfaceOrientation.Portrait:
                                             fromViewController._slidingView.Frame = new CGRect(0, screenBounds.Height, fromFrame.Width, fromFrame.Height);
                                             break;
                                         case UIInterfaceOrientation.LandscapeLeft:
                                             fromViewController._slidingView.Frame = new CGRect(screenBounds.Width, 0, fromFrame.Width, fromFrame.Height);
                                             break;
                                         case UIInterfaceOrientation.LandscapeRight:
                                             fromViewController._slidingView.Frame = new CGRect(screenBounds.Width * -1, 0, fromFrame.Width, fromFrame.Height);
                                             break;
                                         default:
                                             break;
                                     }

                                 },
                                 (finished) => transitionContext.CompleteTransition(true));
        }
    }

    internal class ModalPickerAnimatedTransitioning : UIViewControllerAnimatedTransitioning
    {
        public bool IsPresenting { get; set; }

        float _transitionDuration = 0.4f;

        public ModalPickerAnimatedTransitioning()
        {

        }

        public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext)
        {
            return _transitionDuration;
        }

        public override void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
        {
            var inView = transitionContext.ContainerView;

            var toViewController = transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey) as ModalEditViewController;
            var fromViewController = transitionContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);

            inView.AddSubview(toViewController.View);

            toViewController._slidingView.Frame = CGRect.Empty;
            toViewController.View.Frame = new CGRect(0, 0, fromViewController.View.Frame.Width,
                                                             fromViewController.View.Frame.Height);

            var startingPoint = GetStartingPoint(fromViewController.InterfaceOrientation);
            if (fromViewController.InterfaceOrientation == UIInterfaceOrientation.Portrait)
            {
                toViewController._slidingView.Frame = new CGRect(startingPoint.X, startingPoint.Y,
                                                             fromViewController.View.Frame.Width,
                                                             fromViewController.View.Frame.Height);
            }
            else
            {
                toViewController._slidingView.Frame = new CGRect(startingPoint.X, startingPoint.Y,
                                                             fromViewController.View.Frame.Height,
                                                             fromViewController.View.Frame.Width);
            }

            UIView.AnimateNotify(_transitionDuration,
                                 () =>
                                 {
                                     toViewController.View.BackgroundColor = new UIColor(0, 0.3f);

                                     var endingPoint = GetEndingPoint(fromViewController.InterfaceOrientation);
                                     toViewController._slidingView.Frame = new CGRect(endingPoint.X, endingPoint.Y, fromViewController.View.Frame.Width,
                                                                                      fromViewController.View.Frame.Height);
                                     //fromViewController.View.Alpha = 0.5f;
                                 },
                                 (finished) => transitionContext.CompleteTransition(true)
                                );
        }

        CGPoint GetStartingPoint(UIInterfaceOrientation orientation)
        {
            var screenBounds = UIScreen.MainScreen.Bounds;
            var coordinate = CGPoint.Empty;
            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                    coordinate = new CGPoint(0, screenBounds.Height);
                    break;
                case UIInterfaceOrientation.LandscapeLeft:
                    coordinate = new CGPoint(screenBounds.Width, 0);
                    break;
                case UIInterfaceOrientation.LandscapeRight:
                    coordinate = new CGPoint(screenBounds.Width * -1, 0);
                    break;
                default:
                    coordinate = new CGPoint(0, screenBounds.Height);
                    break;
            }

            return coordinate;
        }

        CGPoint GetEndingPoint(UIInterfaceOrientation orientation)
        {
            var screenBounds = UIScreen.MainScreen.Bounds;
            var coordinate = CGPoint.Empty;
            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                    coordinate = new CGPoint(0, 0);
                    break;
                case UIInterfaceOrientation.LandscapeLeft:
                    coordinate = new CGPoint(0, 0);
                    break;
                case UIInterfaceOrientation.LandscapeRight:
                    coordinate = new CGPoint(0, 0);
                    break;
                default:
                    coordinate = new CGPoint(0, 0);
                    break;
            }

            return coordinate;
        }
    }

}
