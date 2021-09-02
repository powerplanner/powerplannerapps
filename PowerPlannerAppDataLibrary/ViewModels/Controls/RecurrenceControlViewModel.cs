using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.Controls
{
    public class RecurrenceControlViewModel : BindableBase
    {
        public uint GetRepeatIntervalAsNumber()
        {
            if (RepeatIntervalAsString != null && uint.TryParse(RepeatIntervalAsString, out uint answer))
            {
                return answer;
            }

            return 0;
        }

        private bool IsRepeatIntervalValid()
        {
            return GetRepeatIntervalAsNumber() > 0;
        }

        public uint? RepeatIntervalAsNumber
        {
            get => GetRepeatIntervalAsNumber();
            set => RepeatIntervalAsString = value.ToString();
        }

        private string _repeatIntervalAsString = "1";
        public string RepeatIntervalAsString
        {
            get { return _repeatIntervalAsString; }
            set { SetProperty(ref _repeatIntervalAsString, value, nameof(RepeatIntervalAsString), nameof(RepeatIntervalAsNumber)); }
        }

        public enum RepeatOptions
        {
            Daily,
            Weekly,
            Monthly
        }

        private RepeatOptions _selectedRepeatOption = RepeatOptions.Weekly;
        public RepeatOptions SelectedRepeatOption
        {
            get { return _selectedRepeatOption; }
            set { SetProperty(ref _selectedRepeatOption, value, nameof(SelectedRepeatOption), nameof(SelectedRepeatOptionAsString), nameof(AreDayCheckBoxesVisible)); }
        }

        /// <summary>
        /// The options, like "day", "week", and "month"
        /// </summary>
        public string[] RepeatOptionsAsStrings { get; private set; } = new string[]
        {
            PowerPlannerResources.GetString("RepeatingEntry_OptionDay"),
            PowerPlannerResources.GetString("RepeatingEntry_OptionWeek"),
            PowerPlannerResources.GetString("RepeatingEntry_OptionMonth")
        };

        /// <summary>
        /// The string-friendly repeat option
        /// </summary>
        public string SelectedRepeatOptionAsString
        {
            get { return RepeatOptionsAsStrings[(int)SelectedRepeatOption]; }
            set
            {
                int index = RepeatOptionsAsStrings.FindIndex(i => i == value);
                if (index == -1)
                {
                    index = 0;
                }

                SelectedRepeatOption = (RepeatOptions)index;
            }
        }

        public class DayCheckBox : BindableBase
        {
            public DayCheckBox(DayOfWeek dayOfWeek, bool isChecked)
            {
                DayOfWeek = dayOfWeek;
                DisplayName = DateTools.ToLocalizedString(dayOfWeek);
                _isChecked = isChecked;
            }

            public DayOfWeek DayOfWeek { get; private set; }

            public string DisplayName { get; private set; }

            private bool _isChecked;
            public bool IsChecked
            {
                get { return _isChecked; }
                set { SetProperty(ref _isChecked, value, nameof(IsChecked)); }
            }
        }

        public bool AreDayCheckBoxesVisible => SelectedRepeatOption == RepeatOptions.Weekly;

        public DayCheckBox[] DayCheckBoxes { get; private set; } = GenerateDayCheckBoxes();

        public DayCheckBox[] DayCheckBoxesLeftSide
        {
            get { return DayCheckBoxes.Take(4).ToArray(); }
        }

        public DayCheckBox[] DayCheckBoxesRightSide
        {
            get { return DayCheckBoxes.Skip(4).ToArray(); }
        }

        /// <summary>
        /// Gets the selected days of week to repeat on. Sorted.
        /// </summary>
        /// <returns></returns>
        public DayOfWeek[] GetSelectedDaysOfWeek()
        {
            return DayCheckBoxes.Where(i => i.IsChecked).Select(i => i.DayOfWeek).OrderBy(i => i).ToArray();
        }

        /// <summary>
        /// Gets the date that the series should actually start on. Might be different than user picked start date due to weekly occurrence allowing you to pick a day that isn't the start date.
        /// </summary>
        /// <param name="userPickedStartDate"></param>
        /// <returns></returns>
        public DateTime GetStartDate(DateTime userPickedStartDate)
        {
            switch (SelectedRepeatOption)
            {
                // For weekly, they might have selected a day of the week that starts later, so have to find the closest next date
                case RepeatOptions.Weekly:

                    DayOfWeek[] selectedDaysOfWeek = GetSelectedDaysOfWeek();
                    if (selectedDaysOfWeek.Length == 0) throw new InvalidOperationException("No days of week were selected");
                    DateTime closest = DateTime.MaxValue;

                    foreach (var day in selectedDaysOfWeek)
                    {
                        DateTime potentialClosest = DateTools.Next(day, userPickedStartDate);
                        if (potentialClosest < closest)
                        {
                            closest = potentialClosest;
                        }
                    }

                    return closest;

                default:
                    return userPickedStartDate.Date;
            }
        }

        public DateOccurrenceEnumerator GetEnumeratorForDates(DateTime userPickedStartDate)
        {
            return new DateOccurrenceEnumerator(this, userPickedStartDate);
        }

        public DateOccurrenceEnumerable GetEnumerableDates(DateTime userPickedStartDate)
        {
            return new DateOccurrenceEnumerable(this, userPickedStartDate);
        }

        public class DateOccurrenceEnumerable : IEnumerable<DateTime>
        {
            private RecurrenceControlViewModel _viewModel;
            private DateTime _userPickedStartDate;

            public DateOccurrenceEnumerable(RecurrenceControlViewModel viewModel, DateTime userPickedStartDate)
            {
                _viewModel = viewModel;
                _userPickedStartDate = userPickedStartDate;
            }

            public IEnumerator<DateTime> GetEnumerator()
            {
                return _viewModel.GetEnumeratorForDates(_userPickedStartDate);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class DateOccurrenceEnumerator : IEnumerator<DateTime>
        {
            private RecurrenceControlViewModel _viewModel;
            private DateTime _startDate;
            private int _countSoFar = 0;
            private uint _repeatInterval;
            private DayOfWeek[] _sortedRepeatDays;

            /// <summary>
            /// Assumes there's at least one week day selected if using weekly
            /// </summary>
            /// <param name="viewModel"></param>
            /// <param name="userPickedStartDate"></param>
            public DateOccurrenceEnumerator(RecurrenceControlViewModel viewModel, DateTime userPickedStartDate)
            {
                _viewModel = viewModel;
                _repeatInterval = viewModel.GetRepeatIntervalAsNumber();
                _sortedRepeatDays = viewModel.GetSelectedDaysOfWeek();

                // Get the actual start date
                _startDate = viewModel.GetStartDate(userPickedStartDate);
            }

            public DateTime Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // Nothing
            }

            public bool MoveNext()
            {
                // If limiting by occurrences and we've reached the limit
                if (_viewModel.IsEndOccurrencesChecked && _countSoFar >= _viewModel.GetEndOccurrencesAsNumber())
                {
                    return false;
                }

                DateTime prospectiveNextDate;
                
                // If first
                if (_countSoFar == 0)
                {
                    prospectiveNextDate = _startDate;
                }
                else
                {
                    switch (_viewModel.SelectedRepeatOption)
                    {
                        case RepeatOptions.Daily:
                            prospectiveNextDate = _startDate.AddDays(_repeatInterval * _countSoFar).Date;
                            break;

                        case RepeatOptions.Weekly:

                            // If only one day repeating on, that's easy
                            if (_sortedRepeatDays.Length == 1)
                            {
                                prospectiveNextDate = Current.AddDays(_repeatInterval * 7);
                            }
                            else
                            {
                                DateTime value;

                                // If not repeating every week and we're on the last day, jump ahead
                                // Note that this would be slightly wrong in countries where the first day isn't Sunday
                                if (_repeatInterval > 1 && Current.DayOfWeek == _sortedRepeatDays.Last())
                                {
                                    value = Current.AddDays(8);
                                }
                                else
                                {
                                    // Otherwise we just jump ahead 1 and then look for the next
                                    value = Current.AddDays(1);
                                }

                                prospectiveNextDate = _viewModel.GetStartDate(value);
                            }
                            break;

                        case RepeatOptions.Monthly:
                            prospectiveNextDate = _startDate.AddMonths((int)_repeatInterval * _countSoFar).Date;
                            break;

                        default:
                            throw new NotImplementedException("Unknown repeat option");
                    }
                }

                // If limiting by end date, check that
                if (_viewModel.IsEndDateChecked)
                {
                    if (prospectiveNextDate > _viewModel.EndDate)
                    {
                        return false;
                    }
                }

                // Otherwise we're good! Continue!
                _countSoFar++;
                Current = prospectiveNextDate;
                return true;
            }

            public void Reset()
            {
                _countSoFar = 0;
            }
        }

        private static DayCheckBox[] GenerateDayCheckBoxes()
        {
            // For now we're just always going to start with Monday, since for the US, Monday-Friday is the most relevant
            // even though we start the week on Sunday

            return new DayCheckBox[]
            {
                new DayCheckBox(DayOfWeek.Monday, false),
                new DayCheckBox(DayOfWeek.Tuesday, false),
                new DayCheckBox(DayOfWeek.Wednesday, false),
                new DayCheckBox(DayOfWeek.Thursday, false),
                new DayCheckBox(DayOfWeek.Friday, false),
                new DayCheckBox(DayOfWeek.Saturday, false),
                new DayCheckBox(DayOfWeek.Sunday, false)
            };
        }

        public enum EndOptions
        {
            // Note that iOS version is taking a dependency on the order of these enums
            Date,
            Occurrences
        }

        private EndOptions _endOptions = EndOptions.Date;
        public EndOptions SelectedEndOption
        {
            get { return _endOptions; }
            set { SetProperty(ref _endOptions, value, nameof(SelectedEndOption), nameof(IsEndDateChecked), nameof(IsEndOccurrencesChecked)); }
        }

        public bool IsEndDateChecked
        {
            get { return SelectedEndOption == EndOptions.Date; }
            set { SelectedEndOption = value ? EndOptions.Date : EndOptions.Occurrences; }
        }

        public bool IsEndOccurrencesChecked
        {
            get => !IsEndDateChecked;
            set => IsEndDateChecked = !value;
        }

        private DateTime _endDate = DateTime.Today.AddMonths(6);
        public DateTime EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value.Date, nameof(EndDate)); }
        }

        private string _endOccurrencesAsString = "5";
        public string EndOccurrencesAsString
        {
            get { return _endOccurrencesAsString; }
            set { SetProperty(ref _endOccurrencesAsString, value, nameof(EndOccurrencesAsString), nameof(EndOccurencesAsNumber)); }
        }

        public uint EndOccurencesAsNumber
        {
            get => GetEndOccurrencesAsNumber();
            set => EndOccurrencesAsString = value.ToString();
        }

        public uint GetEndOccurrencesAsNumber()
        {
            if (EndOccurrencesAsString != null && uint.TryParse(EndOccurrencesAsString, out uint answer))
            {
                return answer;
            }

            return 0;
        }

        private bool IsEndOccurrencesValid()
        {
            return GetEndOccurrencesAsNumber() > 1;
        }

        public bool ShowErrorIfInvalid()
        {
            string errorMessage = null;
            string telemSubtype = null;
            string userData = null;

            if (!IsRepeatIntervalValid())
            {
                errorMessage = "The repeat interval must be set to an integer value of 1 or greater. You entered: " + RepeatIntervalAsString;
                telemSubtype = "InvalidRepeatInterval";
                userData = RepeatIntervalAsString;
            }
            else if (SelectedRepeatOption == RepeatOptions.Weekly && GetSelectedDaysOfWeek().Length == 0)
            {
                errorMessage = "You must select at least one day of the week to repeat on if using weekly recurrence.";
                telemSubtype = "NoDayOfWeekSelected";
                userData = "";
            }
            else if (IsEndOccurrencesChecked && !IsEndOccurrencesValid())
            {
                errorMessage = "The occurrences value must be set to an integer of 2 or greater. You entered: " + EndOccurrencesAsString;
                telemSubtype = "InvalidOccurrences";
                userData = EndOccurrencesAsString;
            }
            else if (IsEndOccurrencesChecked && GetEndOccurrencesAsNumber() > 50)
            {
                errorMessage = "The occurrences value cannot be greater than 50. Please reduce the number of occurrences. You entered: " + EndOccurrencesAsString;

                TelemetryExtension.Current?.TrackEvent("UserError_TooManyOccurrences", new Dictionary<string, string>()
                {
                    { "Error", "OccurrenceValueOver50" },
                    { "UserData", EndOccurrencesAsString }
                });
            }

            if (errorMessage != null)
            {
                if (telemSubtype != null && userData != null)
                {
                    TelemetryExtension.Current?.TrackEvent("UserError_InvalidRecurrence", new Dictionary<string, string>()
                    {
                        { "Error", telemSubtype },
                        { "UserData", userData }
                    });
                }

                var dontWait = new PortableMessageDialog(errorMessage, "Repeating occurrence invalid").ShowAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns human friendly version of the data, like "Repeat every 1 week on Tu, Th for 13 occurrences"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string answer = $"Repeat every {GetRepeatIntervalAsNumber()} {SelectedRepeatOptionAsString} ";

            if (SelectedRepeatOption == RepeatOptions.Weekly)
            {
                answer += "on " + string.Join(", ", GetSelectedDaysOfWeek().Select(i => i.ToString().Substring(0, 2))) + " ";
            }

            if (IsEndOccurrencesChecked)
            {
                answer += $"for {GetEndOccurrencesAsNumber()} occurrences.";
            }
            else
            {
                answer += $"till {EndDate.ToString("d")}.";
            }

            return answer;
        }
    }
}
