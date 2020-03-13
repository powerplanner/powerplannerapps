using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemHomework : BaseViewItemHomework, IComparable<ViewItemHomework>, IComparable<ViewItemExam>
    {
        private ViewItemClass _class;
        private PropertyChangedEventHandler _classPropertyChangedHandler;
        public ViewItemClass Class
        {
            get { return _class; }
            set
            {
                if (_class != null && _classPropertyChangedHandler != null)
                {
                    _class.PropertyChanged -= _classPropertyChangedHandler;
                }

                SetProperty(ref _class, value, "Class");

                if (_class != null && !_class.IsNoClassClass)
                {
                    _classPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(_class_PropertyChanged).Handler;
                    _class.PropertyChanged += _classPropertyChangedHandler;
                }

                NotifySubtitleChanged();
            }
        }

        private void _class_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Class.Name):
                    OnPropertyChanged(nameof(Subtitle));
                    break;
            }
        }

        public string Subtitle
        {
            get
            {
                if (Class == null || Class.IsNoClassClass)
                {
                    return SubtitleDueDate;
                }
                return Class.Name + SubtitleDueDate;
            }
        }

        /// <summary>
        /// Includes both date and time
        /// </summary>
        public string SubtitleDueDate
        {
            get
            {
                string answer = DateHelpers.ToFriendlyShortDate(Date);

                string timeString = GetTimeString();

                answer += timeString;

                if (Class == null || Class.IsNoClassClass)
                {
                    return string.Format(PowerPlannerResources.GetString("String_DueX"), answer);
                }
                else
                {
                    // Need to make the rest lowercase
                    return $" - " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_DueX", answer);
                }
            }
        }

        /// <summary>
        /// Only includes the time, not date
        /// </summary>
        public string SubtitleDueTime
        {
            get
            {
                var answer = GetTimeString().TrimStart(',', ' ');

                if (Class == null || Class.IsNoClassClass)
                {
                    return string.Format(PowerPlannerResources.GetString("String_DueX"), answer);
                }
                else
                {
                    // Need to make the rest lowercase
                    return $" - " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_DueX", answer);
                }
            }
        }

        private string GetTimeString()
        {
            switch (GetActualTimeOption())
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    {
                        if (Account.IsInDifferentTimeZone)
                        {
                            return ", " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_AtX", DateTimeFormatterExtension.Current.FormatAsShortTime(Date));
                        }

                        return ", " + PowerPlannerResources.GetString("TimeOption_EndOfDay").ToLower();
                    }

                case DataItemMegaItem.TimeOptions.BeforeClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_BeforeClass").ToLower();

                case DataItemMegaItem.TimeOptions.Custom:
                    return " " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_AtX", DateTimeFormatterExtension.Current.FormatAsShortTime(Date));

                case DataItemMegaItem.TimeOptions.DuringClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_DuringClass").ToLower();

                case DataItemMegaItem.TimeOptions.EndOfClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_EndOfClass").ToLower();

                case DataItemMegaItem.TimeOptions.StartOfClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_StartOfClass").ToLower();
            }

            // Shouldn't get hit
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            return "";
        }

        private void NotifySubtitleChanged()
        {
            OnPropertyChanged(nameof(Subtitle));
        }

        internal ViewItemHomework(DataItemMegaItem dataItem) : base(dataItem)
        {
            base.PropertyChanged += ViewItemHomework_PropertyChanged;
        }

        private void ViewItemHomework_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Date):
                    NotifySubtitleChanged();
                    break;
            }
        }

        public int CompareTo(ViewItemExam other)
        {
            return base.CompareTo(other);
        }

        public int CompareTo(ViewItemHomework other)
        {
            return base.CompareTo(other);
        }
    }
}
