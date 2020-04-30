using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerUWP.ViewModel.MainWindow.MainScreen.Calendar
{
    public class PastItemsSurveyViewModel : BaseViewModel
    {
        public PastItemsSurveyViewModel(BaseViewModel parent) : base(parent)
        {
        }

        private static bool _hasShown = false;
        public static void ShowIfNeeded(CalendarViewModel calendarViewModel)
        {
            // Don't show if already shown once while running
            try
            {
                if (_hasShown)
                {
                    return;
                }

                // Or if user is a new user
                if (PowerPlannerAppDataLibrary.Helpers.Settings.TimesShowedCalendarPastCompletedSurvey == -1)
                {
                    _hasShown = true;
                    return;
                }

                // Or if shown to the user 3 times and they didn't respond (or also if they already responded)
                if (PowerPlannerAppDataLibrary.Helpers.Settings.TimesShowedCalendarPastCompletedSurvey >= 3)
                {
                    _hasShown = true;
                    return;
                }

                //  Or if they're not using English language
                if (new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToString()).TwoLetterISOLanguageName != "en")
                {
                    _hasShown = true;
                    return;
                }

                // Show the survey
                _hasShown = true;
                calendarViewModel.MainScreenViewModel.ShowPopup(new PastItemsSurveyViewModel(calendarViewModel.MainScreenViewModel));

                PowerPlannerAppDataLibrary.Helpers.Settings.TimesShowedCalendarPastCompletedSurvey++;
            }
            catch { }
        }

        public void FinishedSubmitting()
        {
            // Set it so we don't show again
            PowerPlannerAppDataLibrary.Helpers.Settings.TimesShowedCalendarPastCompletedSurvey = 99;

            // And close
            RemoveViewModel();
        }
    }
}
