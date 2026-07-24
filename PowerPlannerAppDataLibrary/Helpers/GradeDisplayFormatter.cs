using System;
using System.Globalization;
using PowerPlannerSending;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class GradeDisplayFormatter
    {
        public static string FormatGradeValue(double value, int decimalPlaces = 2)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return value.ToString(CultureInfo.InvariantCulture);

            if (value == Grade.UNGRADED)
                return "--";

            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);
        }

        public static double RoundGradeValue(double value, int decimalPlaces = 2)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value == Grade.UNGRADED)
                return value;

            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        public static string FormatGradePercent(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return value.ToString(CultureInfo.InvariantCulture);

            return Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.##%", CultureInfo.InvariantCulture);
        }
    }
}
