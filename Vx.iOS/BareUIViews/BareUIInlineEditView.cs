using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Controllers;
using ToolsPortable;
using UIKit;

namespace InterfacesiOS.Views
{
    /// <summary>
    /// Displays inline UI, and when clicked displays a popup where you can have a custom view
    /// </summary>
    public abstract class BareUIInlineEditView : UIControl
    {
        private WeakReference<UIViewController> _controller;
        private UILabel _labelHeader;
        public UILabel LabelDisplayValue { get; private set; }

        public BareUIInlineEditView(UIViewController controller, int left = 0, int right = 0)
        {
            _controller = new WeakReference<UIViewController>(controller);

            _labelHeader = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Header",
                Lines = 1,
                Font = UIFont.PreferredBody
            };
            Add(_labelHeader);
            _labelHeader.StretchHeight(this);

            LabelDisplayValue = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 1,
                Font = UIFont.PreferredBody
            };
            Add(LabelDisplayValue);
            LabelDisplayValue.StretchHeight(this);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[labelHeader]->=16-[labelValue]-({right})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                "labelHeader", _labelHeader,
                "labelValue", LabelDisplayValue)));

            this.TouchUpInside += new WeakEventHandler(OnTouchUpInside).Handler;
        }

        public string HeaderText
        {
            get { return _labelHeader.Text; }
            set { _labelHeader.Text = value; }
        }

        public string DisplayValue
        {
            get { return LabelDisplayValue.Text; }
            protected set { LabelDisplayValue.Text = value; }
        }

        protected ModalEditViewController ModalController { get; private set; }

        /// <summary>
        /// Method invoked when user taps on this inline element. This should display the modal popup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTouchUpInside(object sender, EventArgs e)
        {
            if (_controller.TryGetTarget(out UIViewController controller))
            {
                if (ModalController == null)
                {
                    // http://sharpmobilecode.com/a-replacement-for-actionsheet-date-picker/
                    ModalController = CreateModalEditViewController(controller);

                    ModalController.OnModalEditSubmitted += new WeakEventHandler(ModalController_OnModalEditSubmitted).Handler;
                }

                PrepareModalControllerValues();

                ModalController.ShowAsModal();
            }
        }

        protected abstract ModalEditViewController CreateModalEditViewController(UIViewController parent);

        /// <summary>
        /// This is called when the modal is about to be displayed. The modal content view should be updated with latest values.
        /// </summary>
        protected abstract void PrepareModalControllerValues();

        private void ModalController_OnModalEditSubmitted(object sender, EventArgs e)
        {
            UpdateValuesFromModalController();
        }

        /// <summary>
        /// This is called when the user submitted their changes from the modal view.
        /// </summary>
        protected abstract void UpdateValuesFromModalController();
    }
}