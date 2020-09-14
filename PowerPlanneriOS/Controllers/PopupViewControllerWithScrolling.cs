using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlanneriOS.Helpers;
using InterfacesiOS.Helpers;

namespace PowerPlanneriOS.Controllers
{
    public abstract class PopupViewControllerWithScrolling<T> : PopupViewController<T> where T : BaseViewModel
    {
        protected UIScrollView _scrollView;
        private UIView _stackViewContainer;

        public PopupViewControllerWithScrolling()
        {
            _scrollView = new UIScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ShowsHorizontalScrollIndicator = false
            };
            base.ContentView.AddSubview(_scrollView);
            _scrollView.StretchWidthAndHeight(base.ContentView);

            // This is here simply to allow assigning a background to the stack view
            _stackViewContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _scrollView.AddSubview(_stackViewContainer);
            _stackViewContainer.ConfigureForVerticalScrolling(_scrollView, top: AdditionalTopPadding, bottom: 16, left: LeftPadding, right: RightPadding);

            StackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical
            };
            _stackViewContainer.AddSubview(StackView);
            StackView.StretchWidthAndHeight(_stackViewContainer);

            EnableKeyboardScrollOffsetHandling(_scrollView, topOffset: 0);
        }

        protected virtual int AdditionalTopPadding { get { return 0; } }
        protected virtual int LeftPadding { get { return 0; } }
        protected virtual int RightPadding { get { return 0; } }

        /// <summary>
        /// Styles the background grey in preperation for section dividers and inputs to be added
        /// </summary>
        protected void ConfigureForInputsStyle()
        {
            base.View.BackgroundColor = ColorResources.InputSectionDividers;
            _stackViewContainer.BackgroundColor = UIColorCompat.SecondarySystemGroupedBackgroundColor;
        }

        public UIStackView StackView { get; private set; }

        protected void AddSpacing(int height)
        {
            StackView.AddArrangedSubview(new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            }.SetHeight(height));
        }

        protected void AddSectionDivider()
        {
            StackView.AddSectionDivider();
        }

        protected void AddTopSectionDivider()
        {
            StackView.AddTopSectionDivider();
        }

        protected void AddBottomSectionDivider()
        {
            StackView.AddBottomSectionDivider();
        }

        private void AddSectionDividerWithoutBorder()
        {
            var divider = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = ColorResources.InputSectionDividers
            };
            StackView.AddArrangedSubview(divider);
            divider.StretchWidth(StackView);
            divider.SetHeight(34);
        }

        protected void AddDivider(bool fullWidth = false)
        {
            AddDivider(StackView, fullWidth);
        }

        protected static void AddDivider(UIStackView stackView, bool fullWidth = false) 
        {
            stackView.AddDivider(fullWidth);
        }

        protected void AddDeleteButton(string text, EventHandler<EventArgs> onClickAction)
        {
            var buttonDelete = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TintColor = UIColor.Red
            };
            buttonDelete.SetTitle(text, UIControlState.Normal);
            buttonDelete.TouchUpInside += new WeakEventHandler(onClickAction).Handler;
            StackView.AddArrangedSubview(buttonDelete);
            buttonDelete.StretchWidth(StackView);
            buttonDelete.SetHeight(44);
        }

        private List<EventHandler<EventArgs>> _buttonDeleteEventHandlers = new List<EventHandler<EventArgs>>();

        protected void AddDeleteButtonWithConfirmation(string text, Action deleteAction, string confirmTitle, string confirmMessage)
        {
            var handler = new EventHandler<EventArgs>(delegate
            {
                PowerPlannerUIHelper.ConfirmDelete(confirmMessage, confirmTitle, deleteAction);
            });
            _buttonDeleteEventHandlers.Add(handler);

            AddDeleteButton(text, handler);
        }

        /// <summary>
        /// Adds a text field
        /// </summary>
        /// <param name="textField"></param>
        /// <param name="textBindingPropertyName"></param>
        /// <param name="firstResponder">Whether this text box should get the first focus on the page, causing the keyboard to appear</param>
        protected void AddTextField(UITextField textField, string textBindingPropertyName = null, bool firstResponder = false)
        {
            AddTextField(StackView, textField, textBindingPropertyName: textBindingPropertyName, firstResponder: firstResponder);
        }

        protected void AddTextField(BareUITextField textField, string textFieldBindingPropertyName = null, bool firstResponder = false)
        {
            if (textFieldBindingPropertyName != null)
            {
                BindingHost.SetTextFieldBinding(textField, textFieldBindingPropertyName);
            }

            // We don't bind again here, already did binding
            AddTextField(textField, firstResponder: firstResponder);
        }

        protected void AddUnderVisiblity(UIView view, string propertyName)
        {
            StackView.AddUnderVisiblity(view, BindingHost, propertyName);
        }

        private UIView _loadingIndicatorView;
        private UIActivityIndicatorView _loadingIndicator;
        private UILabel _loadingIndicatorLabel;
        public void ShowLoadingOverlay(bool coverTopNavBar = true, string loadingText = null)
        {
            if (IsLoadingOverlayVisible)
            {
                return;
            }

            if (_loadingIndicatorView == null)
            {
                _loadingIndicatorView = new UIView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BackgroundColor = UIColor.FromWhiteAlpha(1, loadingText == null ? 0.5f : 0.8f)
                };

                var topSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                var bottomSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                _loadingIndicatorView.Add(topSpacer);
                _loadingIndicatorView.Add(bottomSpacer);

                _loadingIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Color = ColorResources.PowerPlannerAccentBlue
                };
                _loadingIndicatorView.Add(_loadingIndicator);
                _loadingIndicator.StretchWidth(_loadingIndicatorView);

                _loadingIndicatorLabel = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Hidden = true,
                    TextColor = ColorResources.PowerPlannerAccentBlue,
                    TextAlignment = UITextAlignment.Center
                };
                _loadingIndicatorView.Add(_loadingIndicatorLabel);
                _loadingIndicatorLabel.StretchWidth(_loadingIndicatorView);

                _loadingIndicatorView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[topSpacer][indicator][label][bottomSpacer(==topSpacer)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                    "indicator", _loadingIndicator,
                    "label", _loadingIndicatorLabel,
                    "topSpacer", topSpacer,
                    "bottomSpacer", bottomSpacer)));
            }

            if (coverTopNavBar)
            {
                View.Add(_loadingIndicatorView);
                _loadingIndicatorView.StretchWidthAndHeight(View);
            }
            else
            {
                ContentView.Add(_loadingIndicatorView);
                _loadingIndicatorView.StretchWidthAndHeight(ContentView);
            }

            if (loadingText == null)
            {
                _loadingIndicatorLabel.Hidden = true;
                _loadingIndicatorLabel.SetHeight(0);
            }
            else
            {
                _loadingIndicatorLabel.Hidden = false;
                _loadingIndicatorLabel.Text = loadingText;
            }

            _loadingIndicator.StartAnimating();

            _isLoadingOverlayVisible = true;
        }

        public void HideLoadingOverlay()
        {
            if (!IsLoadingOverlayVisible)
            {
                return;
            }

            if (_loadingIndicatorView != null)
            {
                _loadingIndicator.StopAnimating();
                _loadingIndicatorView.RemoveFromSuperview();
            }

            _isLoadingOverlayVisible = false;
        }

        private bool _isLoadingOverlayVisible;
        public bool IsLoadingOverlayVisible
        {
            set
            {
                if (value)
                {
                    ShowLoadingOverlay();
                }
                else
                {
                    HideLoadingOverlay();
                }
            }

            get { return _isLoadingOverlayVisible; }
        }
    }
}