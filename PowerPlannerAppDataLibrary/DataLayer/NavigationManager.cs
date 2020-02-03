using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary
{
    public static class NavigationManager
    {
        public enum MainMenuSelections
        {
            Calendar,
            Day,
            Agenda,
            Schedule,
            Classes,
            Years,
            Settings
        }
        
        public static MainMenuSelections MainMenuSelection
        {
            get { return Settings.NavigationManagerSettings.MainMenuSelection; }
            set { Settings.NavigationManagerSettings.MainMenuSelection = value; }
        }
        
        public static Guid ClassSelection
        {
            get { return Settings.NavigationManagerSettings.ClassSelection; }

            set
            {
                Settings.NavigationManagerSettings.ClassSelection = value;

                ClassSelectedOn = DateTime.Now;
            }
        }
        
        /// <summary>
        /// This property is NOT stored. Only kept in runtime.
        /// </summary>
        public static DateTime ClassSelectedOn
        {
            get; private set;
        }

        private static DateTime _viewDatesSetOn;
        private static DateTime? _displayMonth;
        private static DateTime? _selectedDate;

        public static DateTime GetSelectedDate()
        {
            if (_viewDatesSetOn < DateTime.Now.AddMinutes(-5))
            {
                _selectedDate = null;
            }

            return _selectedDate.GetValueOrDefault(DateTime.Today);
        }

        public static DateTime GetDisplayMonth()
        {
            if (_viewDatesSetOn < DateTime.Now.AddMinutes(-5))
            {
                _displayMonth = null;
            }

            return _displayMonth.GetValueOrDefault(DateTools.GetMonth(DateTime.Today));
        }

        public static void SetSelectedDate(DateTime value, bool preserveForever = false)
        {
            _selectedDate = value.Date;
            if (_viewDatesSetOn != DateTime.MaxValue)
                _viewDatesSetOn = preserveForever ? DateTime.MaxValue : DateTime.Now;
        }

        public static void SetDisplayMonth(DateTime value, bool preserveForever = false)
        {
            _displayMonth = DateTools.GetMonth(value);
            if (_viewDatesSetOn != DateTime.MaxValue)
                _viewDatesSetOn = preserveForever ? DateTime.MaxValue : DateTime.Now;
        }

        private static DateTime? _previousAddItemDate;
        private static DateTime _previousAddItemDateSetOn;
        public static DateTime? GetPreviousAddItemDate()
        {
            // If last time has been set is 3 minutes or more, expire it
            if (_previousAddItemDateSetOn < DateTime.Now.AddMinutes(-3))
            {
                return null;
            }

            return _previousAddItemDate;
        }

        public static void SetPreviousAddItemDate(DateTime value)
        {
            _previousAddItemDate = value.Date;
            _previousAddItemDateSetOn = DateTime.Now;
        }


        private static Guid? _previousAddItemClass;
        private static DateTime _previousAddItemClassSetOn;

        /// <summary>
        /// This setting isn't saved
        /// </summary>
        public static Guid? GetPreviousAddItemClass()
        {
            // If last time has been set is 3 minutes or more, expire it
            if (_previousAddItemClassSetOn < DateTime.Now.AddMinutes(-3))
            {
                return null;
            }

            return _previousAddItemClass;
        }

        public static void SetPreviousAddItemClass(Guid identifier)
        {
            _previousAddItemClass = identifier;
            _previousAddItemClassSetOn = DateTime.Now;
        }

        /// <summary>
        /// This setting isn't stored
        /// </summary>
        public static Guid SelectedWeightCategoryIdentifier { get; set; }

        /// <summary>
        /// Clears things like SelectedDate and DisplayMonth
        /// </summary>
        public static void RestoreDefaultMemoryItems()
        {
            _previousAddItemDate = null;
            _previousAddItemDateSetOn = DateTime.MinValue;
            SelectedWeightCategoryIdentifier = Guid.Empty;
            _previousAddItemClass = null;
            _previousAddItemClassSetOn = DateTime.MinValue;
            _selectedDate = null;
            _displayMonth = null;
            _viewDatesSetOn = DateTime.MinValue;
        }
    }
}
