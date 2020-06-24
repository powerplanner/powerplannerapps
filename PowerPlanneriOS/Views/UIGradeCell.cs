using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.Converters;
using InterfacesiOS.Helpers;

namespace PowerPlanneriOS.Views
{
    public class UIGradeCell : BareUITableViewCell<BaseViewItemMegaItem>
    {
        private UIView _completionBar;
        private NSLayoutConstraint _constraintCompletionBarHeight;

        private UIStackView _stackView;

        private UILabel _labelTitle;
        private UILabel _labelDescription;
        private UILabel _labelGrade;

        public UIGradeCell(string cellId) : base(cellId)
        {
            var graySideBar = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColorCompat.SystemGray2Color
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

            var titleAndGrade = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                _labelTitle = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption1
                };
                BindingHost.SetLabelTextBinding(_labelTitle, nameof(DataContext.Name));
                titleAndGrade.Add(_labelTitle);
                _labelTitle.StretchHeight(titleAndGrade);

                _labelGrade = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredCaption2.Bold()
                };
                BindingHost.SetLabelTextBinding(_labelGrade, nameof(DataContext.GradeSubtitle));
                titleAndGrade.Add(_labelGrade);
                _labelGrade.StretchHeight(titleAndGrade);

                titleAndGrade.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[title][grade]-16-|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "title", _labelTitle,
                    "grade", _labelGrade));

                // Don't let grade compress, the title should be the one that shrinks
                _labelGrade.SetContentCompressionResistancePriority(501, UILayoutConstraintAxis.Horizontal);
                _labelTitle.SetContentCompressionResistancePriority(499, UILayoutConstraintAxis.Horizontal);

                // Don't let the grade unnecessarily expand either
                _labelGrade.SetContentHuggingPriority(501, UILayoutConstraintAxis.Horizontal);
            }
            _stackView.AddArrangedSubview(titleAndGrade);
            titleAndGrade.StretchWidth(_stackView);

            _labelDescription = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredCaption1,
                TextColor = UIColorCompat.SecondaryLabelColor
            };
            BindingHost.SetLabelTextBinding(_labelDescription, nameof(DataContext.Details));
            _stackView.AddArrangedSubview(_labelDescription);
            _labelDescription.StretchWidth(_stackView);

            ContentView.AddSubview(_stackView);
            _stackView.StretchWidthAndHeight(ContentView, left: 16, top: 8, bottom: 8);

            BindingHost.SetBinding(nameof(DataContext.IsDropped), delegate
            {
                if (_constraintCompletionBarHeight != null)
                {
                    ContentView.RemoveConstraint(_constraintCompletionBarHeight);
                }

                nfloat multiplier = 1;
                if (DataContext.IsDropped)
                {
                    multiplier = 0;
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
            });
        }

        protected override void OnDataContextChanged()
        {
            if (DataContext != null)
            {
                _completionBar.BackgroundColor = BareUIHelper.ToColor(DataContext.WeightCategory.Class.Color);
                _labelGrade.TextColor = BareUIHelper.ToColor(DataContext.WeightCategory.Class.Color);
            }

            base.OnDataContextChanged();
        }
    }
}