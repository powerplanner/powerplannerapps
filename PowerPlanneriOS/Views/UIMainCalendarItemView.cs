using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlanneriOS.Views
{
    public class UIMainCalendarItemView : BareUIView
    {
        private UIView _completedBarContainer;
        private UILabel _labelTitle;

        public Action AfterOpenedHomeworkAction { get; set; }

        public UIMainCalendarItemView()
        {
            UIControl touchControl = new UIControl()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _completedBarContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            touchControl.Add(_completedBarContainer);
            _completedBarContainer.StretchHeight(touchControl);

            _labelTitle = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.White,
                Font = UIFont.PreferredCaption1
            };
            touchControl.Add(_labelTitle);
            _labelTitle.StretchHeight(touchControl, top: 6, bottom: 6);

            touchControl.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[completedBar(0)]-6-[title]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "completedBar", _completedBarContainer,
                "title", _labelTitle));

            this.Add(touchControl);
            touchControl.StretchWidthAndHeight(this);

            touchControl.TouchUpInside += TouchControl_TouchUpInside;
        }

        public static nfloat GetHeight()
        {
            return UIFont.PreferredCaption1.LineHeight + 6 + 6;
        }

        private void TouchControl_TouchUpInside(object sender, EventArgs e)
        {
            if (DataContext is BaseViewItemHomeworkExam)
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(DataContext as BaseViewItemHomeworkExam);

                try
                {
                    AfterOpenedHomeworkAction?.Invoke();
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
            else if (DataContext is ViewItemHoliday)
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.ViewHoliday(DataContext as ViewItemHoliday);
            }
        }

        protected override void OnDataContextChanged()
        {
            if (DataContext is BaseViewItemHomeworkExam)
            {
                var item = DataContext as BaseViewItemHomeworkExam;

                this.BackgroundColor = BareUIHelper.ToColor(item.GetClassOrNull()?.Color);
                _labelTitle.Text = item.Name;

                if (item.IsComplete())
                {
                    if (!_completedBarContainer.Subviews.Any())
                    {
                        var bar = new UIView()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            BackgroundColor = UIColor.FromWhiteAlpha(0, 0.2f)
                        };
                        _completedBarContainer.Add(bar);
                        bar.StretchWidth(_completedBarContainer);
                        bar.StretchHeight(_completedBarContainer);

                        _completedBarContainer.SetWidth(10);
                    }
                    _labelTitle.Alpha = 0.7f;
                }
                else
                {
                    foreach (var view in _completedBarContainer.Subviews)
                    {
                        view.RemoveFromSuperview();
                        _completedBarContainer.SetWidth(0);
                    }
                    _labelTitle.Alpha = 1;
                }
            }

            else if (DataContext is ViewItemHoliday)
            {
                var item = DataContext as ViewItemHoliday;

                this.BackgroundColor = UIColor.FromRGB(228 / 255f, 0, 137 / 255f);
                _labelTitle.Text = item.Name;

                foreach (var view in _completedBarContainer.Subviews)
                {
                    view.RemoveFromSuperview();
                    _completedBarContainer.SetWidth(0);
                }
                _labelTitle.Alpha = 1;
            }

            base.OnDataContextChanged();
        }
    }
}