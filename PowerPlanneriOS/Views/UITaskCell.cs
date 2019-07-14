using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlanneriOS.Views
{
    public class UITaskCell : BareUITableViewCell<BaseViewItemHomeworkExam>
    {
        private UIView _completionBar;
        private NSLayoutConstraint _constraintCompletionBarHeight;

        private UIStackView _stackView;

        private UILabel _labelTitle;
        private UILabel _labelSubtitle;
        private UILabel _labelDescription;

        public UITaskCell(string cellId) : base(cellId)
        {
            var graySideBar = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.FromWhiteAlpha(180 / 255f, 1)
            };
            ContentView.AddSubview(graySideBar);
            graySideBar.SetWidth(8);
            graySideBar.PinToLeft(ContentView);
            graySideBar.StretchHeight(ContentView);

            _completionBar = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.AddSubview(_completionBar);
            _completionBar.SetWidth(8);
            _completionBar.PinToLeft(ContentView);
            ContentView.AddConstraint(NSLayoutConstraint.Create(
                _completionBar,
                NSLayoutAttribute.CenterY,
                NSLayoutRelation.Equal,
                ContentView,
                NSLayoutAttribute.CenterY,
                1,
                0));

            _stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical
            };

            var titleAndDescription = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                _labelTitle = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1
                };
                titleAndDescription.Add(_labelTitle);
                _labelTitle.StretchHeight(titleAndDescription);

                _labelDescription = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption2,
                    TextColor = UIColor.DarkGray
                };
                titleAndDescription.Add(_labelDescription);
                _labelDescription.StretchHeight(titleAndDescription);

                titleAndDescription.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[title][description]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "title", _labelTitle,
                    "description", _labelDescription));

                // Don't let title compress, the description should be the one that shrinks
                _labelTitle.SetContentCompressionResistancePriority(501, UILayoutConstraintAxis.Horizontal);
                _labelDescription.SetContentCompressionResistancePriority(499, UILayoutConstraintAxis.Horizontal);

                // Don't let the title unnecessarily expand either
                _labelTitle.SetContentHuggingPriority(501, UILayoutConstraintAxis.Horizontal);
            }
            _stackView.AddArrangedSubview(titleAndDescription);
            titleAndDescription.StretchWidth(_stackView);

            _labelSubtitle = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1
            };
            _stackView.AddArrangedSubview(_labelSubtitle);
            _labelSubtitle.StretchWidth(_stackView);

            ContentView.AddSubview(_stackView);
            _stackView.StretchWidthAndHeight(ContentView, left: 16, top: 8, bottom: 8);
        }

        protected override void OnDataContextAssigned()
        {
            if (DataContext != null)
            {
                _completionBar.BackgroundColor = BareUIHelper.ToColor(DataContext.GetClassOrNull()?.Color);

                if (_constraintCompletionBarHeight != null)
                {
                    ContentView.RemoveConstraint(_constraintCompletionBarHeight);
                }

                nfloat multiplier = 1;
                if (DataContext.IsComplete())
                {
                    multiplier = 0;
                }
                else if (DataContext is ViewItemHomework)
                {
                    multiplier = 1 - (nfloat)(DataContext as ViewItemHomework).PercentComplete;
                }
                _constraintCompletionBarHeight = NSLayoutConstraint.Create(
                    _completionBar,
                    NSLayoutAttribute.Height,
                    NSLayoutRelation.Equal,
                    ContentView,
                    NSLayoutAttribute.Height,
                    multiplier,
                    0);
                ContentView.AddConstraint(_constraintCompletionBarHeight);

                _labelTitle.AttributedText = new NSAttributedString(DataContext.Name, strikethroughStyle: DataContext.IsComplete() ? NSUnderlineStyle.Single : NSUnderlineStyle.None);

                _labelSubtitle.Text = DataContext.GetSubtitleOrNull();
                _labelSubtitle.TextColor = BareUIHelper.ToColor(DataContext.GetClassOrNull()?.Color);

                if (string.IsNullOrWhiteSpace(DataContext.Details))
                {
                    _labelDescription.Text = "";
                }
                else
                {
                    _labelDescription.Text = " - " + DataContext.Details;
                }
            }

            base.OnDataContextChanged();
        }
    }
}