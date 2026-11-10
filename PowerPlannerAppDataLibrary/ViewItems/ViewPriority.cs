using System;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewPriority : IComparable<ViewPriority>
    {
        public static readonly ViewPriority High = new ViewPriority(PowerPlannerSending.Priority.High);
        public static readonly ViewPriority Normal = new ViewPriority(PowerPlannerSending.Priority.Normal);
        public static readonly ViewPriority Low = new ViewPriority(PowerPlannerSending.Priority.Low);

        public readonly PowerPlannerSending.Priority Value;
        private ViewPriority(PowerPlannerSending.Priority priority)
        {
            Value = priority;
        }

        /// <summary>
        /// Returns localized display string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return R.S("Priority_" + Value.ToString());
        }

        private const string DISPLAY_PREFIX_HIGH = "❗ ";
        private const string DISPLAY_PREFIX_LOW = "⬇️ ";

        public string DisplayPrefix()
        {
            switch (Value)
            {
                case PowerPlannerSending.Priority.High:
                    return DISPLAY_PREFIX_HIGH;

                case PowerPlannerSending.Priority.Low:
                    return DISPLAY_PREFIX_LOW;

                default:
                    return string.Empty;
            }
        }

        public static ViewPriority Get(PowerPlannerSending.Priority priority)
        {
            switch (priority)
            {
                case PowerPlannerSending.Priority.High:
                    return High;

                case PowerPlannerSending.Priority.Low:
                    return Low;

                default:
                    return Normal;
            }
        }

        public int CompareTo(ViewPriority other)
        {
            if (other == null)
            {
                return 0;
            }

            return ((int)Value).CompareTo((int)other.Value);
        }
    }
}
