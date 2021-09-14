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
            contentView.TranslatesAutoresizingMaskIntoConstraints = false;
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

            //UpdateLayout();
        }

        void InitializeControls()
        {
            View.BackgroundColor = UIColor.Clear;
            _slidingView = new UIView();
            _dialogView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            var header = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _headerLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _headerLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _headerLabel.BackgroundColor = HeaderBackgroundColor;
            _headerLabel.TextColor = HeaderTextColor;
            _headerLabel.Text = HeaderText;
            _headerLabel.TextAlignment = UITextAlignment.Center;

            _cancelButton = UIButton.FromType(UIButtonType.System);
            _cancelButton.TranslatesAutoresizingMaskIntoConstraints = false;
            if (CancelButtonColor != null)
            {
                _cancelButton.SetTitleColor(CancelButtonColor, UIControlState.Normal);
            }
            _cancelButton.BackgroundColor = UIColor.Clear;
            _cancelButton.SetTitle(CancelButtonText, UIControlState.Normal);
            _cancelButton.TouchUpInside += new WeakEventHandler(CancelButtonTapped).Handler;

            _doneButton = UIButton.FromType(UIButtonType.System);
            _doneButton.TranslatesAutoresizingMaskIntoConstraints = false;
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

            header.AddSubview(_headerLabel);
            header.AddSubview(_cancelButton);
            header.AddSubview(_doneButton);
            _headerLabel.StretchHeight(header, top: 0);
            _cancelButton.StretchHeight(header, top: 0);
            _doneButton.StretchHeight(header, top: 0);
            _headerLabel.SetContentHuggingPriority(0, UILayoutConstraintAxis.Horizontal);
            header.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-{Math.Max(16, _parent.View.SafeAreaInsets.Left)}-[cancel][header][done]-{Math.Max(16, _parent.View.SafeAreaInsets.Right)}-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "cancel", _cancelButton,
                "header", _headerLabel,
                "done", _doneButton));

            _dialogView.AddSubview(header);
            header.StretchWidth(_dialogView);
            ContentView.StretchWidth(_dialogView);
            _dialogView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-{_parent.View.SafeAreaInsets.Top}-[header(40)][content]-{_parent.View.SafeAreaInsets.Bottom}-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "header", header,
                "content", ContentView));

            _slidingView.Add(_dialogView);
            _dialogView.StretchWidth(_slidingView);
            Add(_slidingView);

            var backgroundTouchTarget = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            backgroundTouchTarget.TouchUpInside += new WeakEventHandler(DoneButtonTapped).Handler;
            Add(backgroundTouchTarget);
            backgroundTouchTarget.StretchWidth(View);

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
            {
                backgroundTouchTarget.TopAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TopAnchor),

                backgroundTouchTarget.BottomAnchor.ConstraintEqualTo(_dialogView.TopAnchor),

                _dialogView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        private nfloat? _originalContentViewHeight;

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
                //UpdateLayout(true);
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
            var datePicker = new UIDatePicker(CGRect.Empty);

            if (SdkSupportHelper.IsUIDatePickerInlineStyleSupported)
            {
                datePicker.PreferredDatePickerStyle = UIDatePickerStyle.Inline;
            }

            return datePicker;
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
            var fromFrame = screenBounds;

            UIView.AnimateNotify(_transitionDuration,
                                 () =>
                                 {
                                     //toViewController.View.Alpha = 1.0f;
                                     fromViewController.View.BackgroundColor = UIColor.Clear;
                                     fromViewController._slidingView.Frame = new CGRect(0, screenBounds.Height, fromFrame.Width, fromFrame.Height);
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

            var fromFrame = UIScreen.MainScreen.Bounds;

            inView.AddSubview(toViewController.View);

            toViewController._slidingView.Frame = CGRect.Empty;
            toViewController.View.Frame = new CGRect(0, 0, fromFrame.Width,
                                                             fromFrame.Height);

            var startingPoint = GetStartingPoint(fromViewController.InterfaceOrientation);
            toViewController._slidingView.Frame = new CGRect(startingPoint.X, startingPoint.Y,
                                                             fromFrame.Width,
                                                             fromFrame.Height);

            UIView.AnimateNotify(_transitionDuration,
                                 () =>
                                 {
                                     toViewController.View.BackgroundColor = new UIColor(0, 0.3f);

                                     var endingPoint = GetEndingPoint(fromViewController.InterfaceOrientation);
                                     toViewController._slidingView.Frame = new CGRect(endingPoint.X, endingPoint.Y, fromFrame.Width,
                                                                                      fromFrame.Height);
                                     //fromViewController.View.Alpha = 0.5f;
                                 },
                                 (finished) => transitionContext.CompleteTransition(true)
                                );
        }

        CGPoint GetStartingPoint(UIInterfaceOrientation orientation)
        {
            var screenBounds = UIScreen.MainScreen.Bounds;
            return new CGPoint(0, screenBounds.Height);
        }

        CGPoint GetEndingPoint(UIInterfaceOrientation orientation)
        {
            return new CGPoint(0, 0);
        }
    }

}
