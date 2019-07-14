using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using InterfacesiOS.Binding;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlanneriOS.Views
{
    public class UIScheduleItemView : UIControl
    {
        public ViewItemSchedule Item { get; private set; }
        private BindingHost m_classBindingHost = new BindingHost();

        public UIScheduleItemView(ViewItemSchedule item)
        {
            Item = item;
            m_classBindingHost.BindingObject = item.Class;

            m_classBindingHost.SetBackgroundColorBinding(this, nameof(item.Class.Color));

            var minTextHeight = UIFont.PreferredCaption1.LineHeight;

            var labelClass = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.White,
                Lines = 0,
                Font = UIFont.PreferredCaption1
            };
            m_classBindingHost.SetLabelTextBinding(labelClass, nameof(item.Class.Name));
            this.Add(labelClass);
            labelClass.StretchWidth(this, left: 4);

            // Time and room don't need to be data bound, since these views will be re-created if those change
            var labelTime = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(item.StartTime), DateTimeFormatterExtension.Current.FormatAsShortTime(item.EndTime)),
                TextColor = UIColor.White,
                Lines = 1,
                Font = UIFont.PreferredCaption1
            };
            this.Add(labelTime);
            labelTime.StretchWidth(this, left: 4);

            labelTime.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Vertical);

            if (string.IsNullOrWhiteSpace(item.Room))
            {
                this.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-2-[labelClass(>={minTextHeight})][labelTime]->=2-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                    "labelClass", labelClass,
                    "labelTime", labelTime)));
            }
            else
            {
                var labelRoom = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = item.Room.Trim(),
                    TextColor = UIColor.White,
                    Font = UIFont.PreferredCaption1,
                    Lines = 0
                };
                this.Add(labelRoom);
                labelRoom.StretchWidth(this, left: 4, right: 4);

                labelRoom.SetContentCompressionResistancePriority(900, UILayoutConstraintAxis.Vertical);

                this.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-2-[labelClass(>={minTextHeight})][labelTime][labelRoom]->=2-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary(
                    "labelClass", labelClass,
                    "labelTime", labelTime,
                    "labelRoom", labelRoom)));
            }
        }
    }
}