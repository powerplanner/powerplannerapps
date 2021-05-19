using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUIPickerViewItemTextWithColorCircle : BareUIView
    {
        public UILabel LabelView { get; private set; }
        public BareUIEllipseView CircleView { get; private set; }

        public string Text
        {
            get { return LabelView.Text; }
            set { LabelView.Text = value; }
        }

        public CGColor Color
        {
            get { return CircleView.FillColor; }
            set { CircleView.FillColor = value; }
        }

        public BareUIPickerViewItemTextWithColorCircle(string text, CGColor color) : this()
        {
            Text = text;
            Color = color;
        }

        public BareUIPickerViewItemTextWithColorCircle()
        {
            const int CIRCLE_HEIGHT = 16;

            var leftSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
            var rightSpacer = new UIView() { TranslatesAutoresizingMaskIntoConstraints = false };
            this.Add(leftSpacer);
            this.Add(rightSpacer);

            CircleView = new BareUIEllipseView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AspectRatio = BareUIEllipseView.AspectRatios.Circle
            };
            this.Add(CircleView);
            CircleView.SetWidth(CIRCLE_HEIGHT);
            CircleView.StretchHeight(this);

            LabelView = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredTitle3,
                Lines = 1
            };
            this.Add(LabelView);
            LabelView.StretchHeight(this);

            this.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[left][circle]-8-[label][right(==left)]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "left", leftSpacer,
                "right", rightSpacer,
                "circle", CircleView,
                "label", LabelView));
        }
    }
}
