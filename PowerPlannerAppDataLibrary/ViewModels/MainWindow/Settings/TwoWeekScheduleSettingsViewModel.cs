using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerSending;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class TwoWeekScheduleSettingsViewModel : BaseSettingsViewModelWithAccount
    {
        public TwoWeekScheduleSettingsViewModel(BaseViewModel parent) : base(parent)
        {
            _currentWeek = Account.CurrentWeek;
            _weekChangesOn = Account.WeekChangesOn;
        }

        private string[] _availableCurrentWeekStrings;
        /// <summary>
        /// The localized strings
        /// </summary>
        public string[] AvailableCurrentWeekStrings
        {
            get
            {
                if (_availableCurrentWeekStrings == null)
                {
                    _availableCurrentWeekStrings = new string[]
                    {
                        PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.WeekOne),
                        PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.WeekTwo)
                    };
                }

                return _availableCurrentWeekStrings;
            }
        }

        public string CurrentWeekString
        {
            get
            {
                switch (CurrentWeek)
                {
                    case PowerPlannerSending.Schedule.Week.WeekOne:
                        return AvailableCurrentWeekStrings[0];

                    case PowerPlannerSending.Schedule.Week.WeekTwo:
                        return AvailableCurrentWeekStrings[1];

                    default:
                        throw new NotImplementedException();
                }
            }

            set
            {
                switch (AvailableCurrentWeekStrings.FindIndex(i => i == value))
                {
                    case 0:
                        CurrentWeek = PowerPlannerSending.Schedule.Week.WeekOne;
                        break;

                    case 1:
                        CurrentWeek = PowerPlannerSending.Schedule.Week.WeekTwo;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private Schedule.Week _currentWeek;
        public Schedule.Week CurrentWeek
        {
            get { return _currentWeek; }
            set
            {
                if (_currentWeek != value)
                {
                    _currentWeek = value;
                    OnPropertyChanged(nameof(CurrentWeek));
                }

                if (Account.CurrentWeek != value)
                {
                    var dontWait = Account.SetWeek(Account.WeekChangesOn, value);
                }
            }
        }

        private string[] _availableWeekChangesOnStrings;
        /// <summary>
        /// The localized strings
        /// </summary>
        public string[] AvailableWeekChangesOnStrings
        {
            get
            {
                if (_availableWeekChangesOnStrings == null)
                {
                    _availableWeekChangesOnStrings = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(i => LocalizedDateTimeStrings.GetDayName(i)).ToArray();
                }

                return _availableWeekChangesOnStrings;
            }
        }

        public string WeekChangesOnString
        {
            get { return AvailableWeekChangesOnStrings[(int)WeekChangesOn]; }
            set
            {
                WeekChangesOn = (DayOfWeek)AvailableWeekChangesOnStrings.FindIndex(i => i == value);
            }
        }

        private DayOfWeek _weekChangesOn;
        public DayOfWeek WeekChangesOn
        {
            get { return _weekChangesOn; }
            set
            {
                if (_weekChangesOn != value)
                {
                    _weekChangesOn = value;
                    OnPropertyChanged(nameof(WeekChangesOn));
                }

                if (Account.WeekChangesOn != value)
                {
                    var dontWait = Account.SetWeek(value, Account.CurrentWeek);
                }
            }
        }
    }
}
