using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Vx.Helpers;

namespace Vx.Test
{
    [TestClass]
    public class CustomTimePickerHelpersTest
    {
        [TestMethod]
        public void Test12HourWithoutAmPm()
        {
            // Assume AM if between 6-11
            Assert12Hour("9:00", new TimeSpan(9, 0, 0));
            Assert12Hour("8:30", new TimeSpan(8, 30, 0));
            Assert12Hour("7:12", new TimeSpan(7, 12, 0));
            Assert12Hour("6:00", new TimeSpan(6, 0, 0));
            Assert12Hour("6:13", new TimeSpan(6, 13, 0));
            Assert12Hour("10:40", new TimeSpan(10, 40, 0));
            Assert12Hour("11:59", new TimeSpan(11, 59, 0));

            // Assume PM for 12, 1, 2, 3, 4, 5
            Assert12Hour("12:00", new TimeSpan(12, 0, 0));
            Assert12Hour("12:45", new TimeSpan(12, 45, 0));
            Assert12Hour("1:55", new TimeSpan(13, 55, 0));
            Assert12Hour("2:35", new TimeSpan(14, 35, 0));
            Assert12Hour("3:00", new TimeSpan(15, 0, 0));
            Assert12Hour("4:55", new TimeSpan(16, 55, 0));
            Assert12Hour("5:59", new TimeSpan(17, 59, 0));
        }

        [TestMethod]
        public void TestJustHour12HourWithoutAmPm()
        {
            Assert12Hour("6", new TimeSpan(6, 0, 0));
            Assert12Hour("7", new TimeSpan(7, 0, 0));
            Assert12Hour("8", new TimeSpan(8, 0, 0));
            Assert12Hour("9", new TimeSpan(9, 0, 0));
            Assert12Hour("10", new TimeSpan(10, 0, 0));
            Assert12Hour("11", new TimeSpan(11, 0, 0));

            Assert12Hour("12", new TimeSpan(12, 0, 0));
            Assert12Hour("1", new TimeSpan(13, 0, 0));
            Assert12Hour("2", new TimeSpan(14, 0, 0));
            Assert12Hour("3", new TimeSpan(15, 0, 0));
            Assert12Hour("4", new TimeSpan(16, 0, 0));
            Assert12Hour("5", new TimeSpan(17, 0, 0));
        }

        [TestMethod]
        public void TestJustHour24HourWithoutAmPm()
        {
            Assert24Hour("0", new TimeSpan(0, 0, 0));
            Assert24Hour("1", new TimeSpan(1, 0, 0));
            Assert24Hour("2", new TimeSpan(2, 0, 0));
            Assert24Hour("3", new TimeSpan(3, 0, 0));
            Assert24Hour("6", new TimeSpan(6, 0, 0));
            Assert24Hour("7", new TimeSpan(7, 0, 0));
            Assert24Hour("8", new TimeSpan(8, 0, 0));

            Assert24Hour("00", new TimeSpan(0, 0, 0));
            Assert24Hour("01", new TimeSpan(1, 0, 0));
            Assert24Hour("02", new TimeSpan(2, 0, 0));
            Assert24Hour("03", new TimeSpan(3, 0, 0));
            Assert24Hour("06", new TimeSpan(6, 0, 0));
            Assert24Hour("07", new TimeSpan(7, 0, 0));
            Assert24Hour("08", new TimeSpan(8, 0, 0));
            Assert24Hour("09", new TimeSpan(9, 0, 0));
            Assert24Hour("10", new TimeSpan(10, 0, 0));
            Assert24Hour("11", new TimeSpan(11, 0, 0));

            Assert24Hour("12", new TimeSpan(12, 0, 0));
            Assert24Hour("13", new TimeSpan(13, 0, 0));
            Assert24Hour("14", new TimeSpan(14, 0, 0));
            Assert24Hour("15", new TimeSpan(15, 0, 0));
            Assert24Hour("16", new TimeSpan(16, 0, 0));
            Assert24Hour("17", new TimeSpan(17, 0, 0));
        }

        [TestMethod]
        public void TestHourPlusSingleMinute12Hour()
        {
            // Auto-complete to 
            Assert12Hour("6:2", new TimeSpan(6, 20, 0));
            Assert12Hour("11:4", new TimeSpan(11, 40, 0));

            // Assumes PM since 3 is usually PM
            Assert12Hour("3:1", new TimeSpan(15, 10, 0));
        }

        [TestMethod]
        public void TestHourPlusSingleMinute24Hour()
        {
            // Auto-complete to 
            Assert24Hour("6:2", new TimeSpan(6, 20, 0));
            Assert24Hour("06:2", new TimeSpan(6, 20, 0));
            Assert24Hour("11:4", new TimeSpan(11, 40, 0));

            Assert24Hour("3:1", new TimeSpan(3, 10, 0));
            Assert24Hour("03:1", new TimeSpan(3, 10, 0));
            Assert24Hour("15:1", new TimeSpan(15, 10, 0));
        }

        [TestMethod]
        public void TestJustHour12Hour()
        {
            Assert12Hour("6am", new TimeSpan(6, 0, 0));
            Assert12Hour("7 am", new TimeSpan(7, 0, 0));
            Assert12Hour("8a", new TimeSpan(8, 0, 0));
            Assert12Hour("9PM", new TimeSpan(21, 0, 0));
            Assert12Hour("10 PM", new TimeSpan(22, 0, 0));
            Assert12Hour("11P", new TimeSpan(23, 0, 0));

            Assert12Hour("12p", new TimeSpan(12, 0, 0));
            Assert12Hour("1 pm", new TimeSpan(13, 0, 0));
            Assert12Hour("2pm", new TimeSpan(14, 0, 0));
            Assert12Hour("3A", new TimeSpan(3, 0, 0));
            Assert12Hour("4AM", new TimeSpan(4, 0, 0));
            Assert12Hour("5 AM", new TimeSpan(5, 0, 0));
        }

        [TestMethod]
        public void Test12HourWithAm()
        {
            Assert12Hour("9:00 am", new TimeSpan(9, 0, 0));
            Assert12Hour("8:30 am", new TimeSpan(8, 30, 0));
            Assert12Hour("7:12 am", new TimeSpan(7, 12, 0));
            Assert12Hour("12:45 am", new TimeSpan(0, 45, 0));
            Assert12Hour("1:55 am", new TimeSpan(1, 55, 0));
        }

        [TestMethod]
        public void Test24HourStandard()
        {
            Assert24Hour("09:00", new TimeSpan(9, 0, 0));
            Assert24Hour("8:30", new TimeSpan(8, 30, 0));
            Assert24Hour("07:12", new TimeSpan(7, 12, 0));
            Assert24Hour("12:45", new TimeSpan(12, 45, 0));
            Assert24Hour("1:55", new TimeSpan(1, 55, 0));
            Assert24Hour("13:55", new TimeSpan(13, 55, 0));
            Assert24Hour("00:00", new TimeSpan(0, 0, 0));
            Assert24Hour("23:59", new TimeSpan(23, 59, 0));
        }

        [TestMethod]
        public void Test12HourAmVariations()
        {
            Assert12Hour("8:30am", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30a", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30AM", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30A", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30 am", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30 a", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30 A", new TimeSpan(8, 30, 0));
            Assert12Hour("8:30 AM", new TimeSpan(8, 30, 0));
        }

        [TestMethod]
        public void Test12HourPmVariations()
        {
            Assert12Hour("8:30pm", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30p", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30PM", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30P", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30 pm", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30 p", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30 P", new TimeSpan(20, 30, 0));
            Assert12Hour("8:30 PM", new TimeSpan(20, 30, 0));
        }

        [TestMethod]
        public void HandleWhitespace()
        {
            Assert12Hour("  7:15 ", new TimeSpan(7, 15, 0));
        }

        [TestMethod]
        public void TestFailuresNoContent()
        {
            AssertFailsToParse12Hour("");
            AssertFailsToParse24Hour("");
            AssertFailsToParse12Hour("  ");
            AssertFailsToParse24Hour("  ");
        }

        [TestMethod]
        public void TestFailuresInvalidRanges()
        {
            AssertFailsToParse12Hour("25");
            AssertFailsToParse12Hour("28:30");
            AssertFailsToParse12Hour("12:60");
            AssertFailsToParse12Hour("7:7");
        }

        [TestMethod]
        public void TestBoth24AndAmPmUsed()
        {
            // This should still work... we allow leading 0's
            Assert12Hour("03:50 PM", new TimeSpan(15, 50, 0));
            Assert24Hour("03:50 PM", new TimeSpan(15, 50, 0));

            // This should NOT work
            AssertFailsToParse12Hour("15:50 PM");
            AssertFailsToParse24Hour("15:50 PM");

            // This similarly shouldn't work
            AssertFailsToParse12Hour("0:50 PM");
            AssertFailsToParse24Hour("0:50 PM");
        }

        [TestMethod]
        public void TestUsing24HourIn12HourMode()
        {
            // This should go to 3 AM even though normally we use 3 PM
            Assert12Hour("03", new TimeSpan(3, 0, 0));

            // This should go to 5 PM
            Assert12Hour("17:00", new TimeSpan(17, 0, 0));
        }

        [TestMethod]
        public void TestUsing12HourIn24HourMode()
        {
            Assert24Hour("3am", new TimeSpan(3, 0, 0));
            Assert24Hour("3pm", new TimeSpan(15, 0, 0));
        }

        [TestMethod]
        public void TestFailuresDueToStartTime()
        {
            AssertFailsToParse12Hour("7:00 AM", new TimeSpan(9, 0, 0));
            AssertFailsToParse12Hour("3:00 PM", new TimeSpan(15, 0, 0));

            AssertFailsToParse24Hour("7:00", new TimeSpan(9, 0, 0));
            AssertFailsToParse12Hour("15:00", new TimeSpan(15, 0, 0));
        }

        [TestMethod]
        public void TestAdjustingDueToStartTime()
        {
            // Normally would parse as 8 AM, but since start is 9 AM, assumes 8 PM
            Assert12Hour(new TimeSpan(9, 0, 0), "8", new TimeSpan(20, 0, 0));

            // Normally would parse as 6:30 AM, but since start is 5 PM, assumes 6:30 PM
            Assert12Hour(new TimeSpan(17, 0, 0), "6:30", new TimeSpan(18, 30, 0));

            // If start time is 3:00 AM and they enter 4:00, assume AM since that's closer than 4PM (even though normally we'd assume 4PM)
            Assert12Hour(new TimeSpan(3, 0, 0), "4:00", new TimeSpan(4, 0, 0));
        }

        [TestMethod]
        public void TestAddingRelativeTime()
        {
            Assert12Hour(new TimeSpan(9, 0, 0), "+1", new TimeSpan(10, 0, 0));
            Assert12Hour(new TimeSpan(9, 0, 0), "+3", new TimeSpan(12, 0, 0));
            Assert12Hour(new TimeSpan(9, 0, 0), "+2h25m", new TimeSpan(11, 25, 0));

            // Note that the trailing "m" is required
            AssertFailsToParse12Hour("+2h25");

            // Maxing out
            Assert12Hour(new TimeSpan(9, 0, 0), "+22", new TimeSpan(23, 59, 0));
            Assert12Hour(new TimeSpan(9, 0, 0), "+20:25", new TimeSpan(23, 59, 0));

            Assert12Hour(new TimeSpan(9, 0, 0), "+2:25", new TimeSpan(11, 25, 0));

            // Should work same in 24hr
            Assert24Hour(new TimeSpan(9, 0, 0), "+2h25m", new TimeSpan(11, 25, 0));
        }

        private void Assert12Hour(TimeSpan startTime, string input, TimeSpan expected)
        {
            AssertParse(input, startTime, false, expected);
        }

        private void Assert24Hour(string input, TimeSpan expected)
        {
            AssertParse(input, default(TimeSpan), true, expected);
        }

        private void Assert24Hour(TimeSpan startTime, string input, TimeSpan expected)
        {
            AssertParse(input, startTime, true, expected);
        }

        private void Assert12Hour(string input, TimeSpan expected)
        {
            AssertParse(input, default(TimeSpan), false, expected);
        }

        private void AssertParse(string input, TimeSpan startTime, bool is24hour, TimeSpan expected)
        {
            if (CustomTimePickerHelpers.ParseComboBoxItem(startTime, input, is24hour, out TimeSpan answer))
            {
                Assert.AreEqual(expected, answer);
            }
            else
            {
                Assert.Fail("Failed to parse " + input);
            }
        }

        private void AssertFailsToParse12Hour(string input, TimeSpan startTime = default(TimeSpan))
        {
            AssertFailsToParse(input, startTime, false);
        }

        private void AssertFailsToParse24Hour(string input, TimeSpan startTime = default(TimeSpan))
        {
            AssertFailsToParse(input, startTime, true);
        }

        private void AssertFailsToParse(string input, TimeSpan startTime, bool is24hour)
        {
            if (CustomTimePickerHelpers.ParseComboBoxItem(startTime, input, is24hour, out _))
            {
                Assert.Fail("Should have failed to parse " + input + " but succeeded.");
            }
            else
            {
                // Passed!
            }
        }
    }
}
