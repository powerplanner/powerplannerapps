using PowerPlannerAppDataLibrary.DataLayer.DataItems;
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
    public class ViewItemExam : BaseViewItemHomeworkExam, IComparable<ViewItemExam>, IComparable<ViewItemHomework>
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

                if (_class != null)
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
                    NotifySubtitleChanged();
                    break;
            }
        }

        internal ViewItemExam(DataItemMegaItem dataItem) : base(dataItem)
        {
            base.PropertyChanged += ViewItemExam_PropertyChanged;
        }

        private void ViewItemExam_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Date):
                    NotifySubtitleChanged();
                    break;
            }
        }

        /// <summary>
        /// Returns true if the exam is in the past
        /// </summary>
        public new bool IsComplete
        {
            get
            {
                if (Date < DateTime.Today)
                {
                    return true;
                }

                DateTime endTime;
                if (TryGetEndDateWithTime(out endTime))
                {
                    return endTime < DateTime.Now;
                }

                return false;
            }
        }

        public double PercentComplete
        {
            get { return IsComplete ? 1 : 0; }
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
                    return string.Format(PowerPlannerResources.GetString("String_OnX"), answer);
                }
                else
                {
                    // Need to make the rest lowercase
                    return " - " + PowerPlannerResources.GetStringAsLowercaseWithParameters("String_OnX", answer);
                }
            }
        }

        /// <summary>
        /// Gets only the time
        /// </summary>
        public string SubtitleDueTime
        {
            get
            {
                string answer = GetTimeString().TrimStart(',', ' ');

                if (Class == null || Class.IsNoClassClass)
                {
                    return answer.Substring(0, 1).ToUpper() + answer.Substring(1);
                }
                else
                {
                    return $" - {answer}";
                }
            }
        }

        private string GetTimeString()
        {
            switch (GetActualTimeOption())
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    return ", " + PowerPlannerResources.GetString("TimeOption_AllDay").ToLower();

                case DataItemMegaItem.TimeOptions.BeforeClass:
                    return ", " + PowerPlannerResources.GetString("TimeOption_BeforeClass").ToLower();

                case DataItemMegaItem.TimeOptions.Custom:
                    return $", " + string.Format(PowerPlannerResources.GetString("String_TimeToTime"), DateTimeFormatterExtension.Current.FormatAsShortTimeWithoutAmPm(Date), DateTimeFormatterExtension.Current.FormatAsShortTime(EndTime));

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

        public int CompareTo(ViewItemExam other)
        {
            return base.CompareTo(other);
        }

        public int CompareTo(ViewItemHomework other)
        {
            return base.CompareTo(other);
        }

        public override bool IsActive(DateTime today)
        {
            return !IsComplete;
        }
    }
}
