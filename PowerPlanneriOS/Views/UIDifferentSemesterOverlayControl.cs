using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlanneriOS.Views
{
    public class DifferentSemesterOverlayControl : UIControl
    {
        /// <summary>
        /// If you specify int.MaxValue for top padding, it'll become vertically centered
        /// </summary>
        /// <param name="topPadding"></param>
        /// <param name="leftPadding"></param>
        /// <param name="rightPadding"></param>
        public DifferentSemesterOverlayControl(int topPadding, int leftPadding, int rightPadding)
        {
            base.BackgroundColor = UIColor.FromWhiteAlpha(1, 0.4f);

            var control = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.FromWhiteAlpha(0, 0.7f)
            };
            {
                var labelHeader = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "Different semester",
                    Font = UIFont.PreferredSubheadline,
                    TextAlignment = UITextAlignment.Center,
                    TextColor = UIColor.White
                };
                control.Add(labelHeader);
                labelHeader.StretchWidth(control, left: 4, right: 4);

                var labelBody = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "This is a different semester. Tap this to view all of your semesters",
                    Font = UIFont.PreferredCaption1,
                    TextAlignment = UITextAlignment.Center,
                    TextColor = UIColor.White,
                    Lines = 0
                };
                control.Add(labelBody);
                labelBody.StretchWidth(control, left: 4, right: 4);

                control.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-4-[header][body]-4-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "header", labelHeader,
                    "body", labelBody));

                control.TouchUpInside += new WeakEventHandler(delegate {
                    var mainScreenViewModel = PowerPlannerApp.Current.GetMainScreenViewModel();
                    if (mainScreenViewModel != null)
                    {
                        mainScreenViewModel.OpenOrShowYears();
                    }
                }).Handler;
            }
            this.Add(control);
            control.StretchWidth(this, left: leftPadding, right: rightPadding);
            if (topPadding == int.MaxValue)
            {
                var topView = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                var bottomView = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
                this.Add(topView);
                this.Add(bottomView);
                this.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[topView][control][bottomView(topView)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "topView", topView,
                    "control", control,
                    "bottomView", bottomView));
            }
            else
            {
                control.PinToTop(this, topPadding);
            }

            this.TouchUpInside += new WeakEventHandler(delegate {
                this.RemoveFromSuperview();
            }).Handler;
        }
    }
}