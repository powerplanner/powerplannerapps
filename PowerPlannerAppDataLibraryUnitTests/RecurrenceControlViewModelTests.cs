using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using System.Linq;

namespace PowerPlannerAppDataLibraryUnitTests
{
    [TestClass]
    public class RecurrenceControlViewModelTests
    {
        [TestMethod]
        public void TestWeeklyRecurrence()
        {
            var model = new RecurrenceControlViewModel();

            model.SelectedRepeatOption = RecurrenceControlViewModel.RepeatOptions.Weekly;
            model.RepeatIntervalAsString = "1";
            model.DayCheckBoxes.First(i => i.DayOfWeek == DayOfWeek.Monday).IsChecked = true;
            model.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Date;
            model.EndDate = new DateTime(2018, 3, 19);

            Verify(model, new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 19));

            model.EndDate = new DateTime(2018, 3, 20);

            Verify(model, new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 19));

            model.EndDate = new DateTime(2018, 3, 25);

            Verify(model, new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 19));

            model.EndDate = new DateTime(2018, 3, 18);

            Verify(model, new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 12));

            model.EndDate = new DateTime(2018, 3, 20);

            Verify(model, new DateTime(2018, 3, 11),
                new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 19));

            model.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Occurrences;
            model.EndOccurrencesAsString = "4";

            Verify(model, new DateTime(2018, 3, 11),
                new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 19),
                new DateTime(2018, 3, 26),
                new DateTime(2018, 4, 2));

            model.EndOccurrencesAsString = "2";

            Verify(model, new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 12),
                new DateTime(2018, 3, 19));

            model.DayCheckBoxes.First(i => i.DayOfWeek == DayOfWeek.Monday).IsChecked = false;
            model.DayCheckBoxes.First(i => i.DayOfWeek == DayOfWeek.Wednesday).IsChecked = true;
            model.DayCheckBoxes.First(i => i.DayOfWeek == DayOfWeek.Tuesday).IsChecked = true;
            model.EndOccurrencesAsString = "4";

            Verify(model, new DateTime(2018, 3, 19),
                new DateTime(2018, 3, 20),
                new DateTime(2018, 3, 21),
                new DateTime(2018, 3, 27),
                new DateTime(2018, 3, 28));

            model.RepeatIntervalAsString = "2";

            Verify(model, new DateTime(2018, 3, 19),
                new DateTime(2018, 3, 20),
                new DateTime(2018, 3, 21),
                new DateTime(2018, 4, 3),
                new DateTime(2018, 4, 4));

            model.DayCheckBoxes.First(i => i.DayOfWeek == DayOfWeek.Wednesday).IsChecked = false;
            model.DayCheckBoxes.First(i => i.DayOfWeek == DayOfWeek.Thursday).IsChecked = true;

            Verify(model, new DateTime(2018, 3, 19),
                new DateTime(2018, 3, 20),
                new DateTime(2018, 3, 22),
                new DateTime(2018, 4, 3),
                new DateTime(2018, 4, 5));
        }

        [TestMethod]
        public void TestDailyRecurrence()
        {
            var model = new RecurrenceControlViewModel();

            model.SelectedRepeatOption = RecurrenceControlViewModel.RepeatOptions.Daily;
            model.RepeatIntervalAsString = "1";
            model.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Date;
            model.EndDate = new DateTime(2018, 3, 3);

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 2),
                new DateTime(2018, 3, 3));

            model.RepeatIntervalAsString = "3";

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1));

            model.EndDate = new DateTime(2018, 3, 4);

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 4));

            model.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Occurrences;
            model.EndOccurrencesAsString = "3";

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 4),
                new DateTime(2018, 3, 7));

            model.RepeatIntervalAsString = "1";

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 2),
                new DateTime(2018, 3, 3));
        }

        [TestMethod]
        public void TestMonthlyRecurrence()
        {

            var model = new RecurrenceControlViewModel();

            model.SelectedRepeatOption = RecurrenceControlViewModel.RepeatOptions.Monthly;
            model.RepeatIntervalAsString = "1";
            model.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Date;
            model.EndDate = new DateTime(2018, 5, 1);

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 4, 1),
                new DateTime(2018, 5, 1));

            model.EndDate = new DateTime(2018, 5, 2);

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 4, 1),
                new DateTime(2018, 5, 1));

            model.RepeatIntervalAsString = "2";

            Verify(model, new DateTime(2018, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2018, 5, 1));

            model.RepeatIntervalAsString = "1";
            model.EndDate = new DateTime(2018, 5, 31);

            Verify(model, new DateTime(2018, 3, 31),
                new DateTime(2018, 3, 31),
                new DateTime(2018, 4, 30),
                new DateTime(2018, 5, 31));

            model.SelectedEndOption = RecurrenceControlViewModel.EndOptions.Occurrences;
            model.EndOccurrencesAsString = "4";

            Verify(model, new DateTime(2018, 3, 31),
                new DateTime(2018, 3, 31),
                new DateTime(2018, 4, 30),
                new DateTime(2018, 5, 31),
                new DateTime(2018, 6, 30));

            model.RepeatIntervalAsString = "3";

            Verify(model, new DateTime(2018, 3, 15),
                new DateTime(2018, 3, 15),
                new DateTime(2018, 6, 15),
                new DateTime(2018, 9, 15),
                new DateTime(2018, 12, 15));
        }

        private static void Verify(RecurrenceControlViewModel model, DateTime userSelectedStartDate, params DateTime[] expected)
        {
            DateTime[] actual = model.GetEnumerableDates(userSelectedStartDate).ToArray();

            Assert.AreEqual(expected.Length, actual.Length, "The number of occurrences didn't match");

            Assert.IsTrue(actual.SequenceEqual(expected), "The dates were different");
        }
    }
}
