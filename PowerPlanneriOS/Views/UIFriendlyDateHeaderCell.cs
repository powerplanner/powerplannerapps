using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using InterfacesiOS.Helpers;

namespace PowerPlanneriOS.Views
{
    public class UIFriendlyDateHeaderCell : BareUITableViewCell<DateTime>
    {
        private UILabel _labelText;

        public UIFriendlyDateHeaderCell(string cellId) : base(cellId)
        {
            // Don't allow clicking on this header cell
            UserInteractionEnabled = false;

            ContentView.BackgroundColor = UIColorCompat.SecondarySystemBackgroundColor;

            _labelText = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredSubheadline,
                TextColor = UIColorCompat.SecondaryLabelColor
            };
            ContentView.AddSubview(_labelText);
            _labelText.StretchWidthAndHeight(ContentView, left: 16, top: 8, bottom: 8);

            ContentView.SetHeight(44);
        }

        protected override void OnDataContextChanged()
        {
            _labelText.Text = GetHeaderText();

            base.OnDataContextChanged();
        }

        private string GetHeaderText()
        {
            var date = DataContext;

            if (date.Date >= DateTime.Today)
            {
                if (date == DateTime.Today)
                    return PowerPlannerResources.GetRelativeDateToday();
                else if (date == DateTime.Today.AddDays(1))
                    return PowerPlannerResources.GetRelativeDateTomorrow();
                else if (date == DateTime.Today.AddDays(2))
                    return PowerPlannerResources.GetRelativeDateInXDays(2);
                else if (date < DateTime.Today.AddDays(7))
                    return PowerPlannerResources.GetRelativeDateThisDayOfWeek(date.DayOfWeek);
                else if (date < DateTime.Today.AddDays(14))
                    return PowerPlannerResources.GetRelativeDateNextDayOfWeek(date.DayOfWeek);

                return date.ToString("dddd, MMMM d");
            }

            // In the past
            if (date.Date > DateTime.Today.AddYears(-1))
            {
                return date.ToString("dddd, MMMM d");
            }
            else
            {
                return date.ToString("MMMM d, yyyy");
            }
        }
    }
}