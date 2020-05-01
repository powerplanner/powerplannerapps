// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using ToolsPortable;
using static PowerPlannerAppDataLibrary.NavigationManager;

namespace PowerPlannerAppDataLibrary.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        public static double GetDoubleOrDefault(this ISettings settings, string key, double defaultValue = default(double))
        {
            string val = settings.GetValueOrDefault(key, null);
            if (val != null && double.TryParse(val, out double answer))
            {
                return answer;
            }
            return defaultValue;
        }

        public static bool AddOrUpdateDouble(this ISettings settings, string key, double value)
        {
            string val = value.ToString();
            return settings.AddOrUpdateValue(key, val);
        }

        public static TEnum GetEnumOrDefault<TEnum>(this ISettings settings, string key, TEnum defaultValue = default(TEnum)) where TEnum : struct
        {
            // Try/catch since this used to be stored as an integer, and now we store it as a string
            string val;
            try
            {
                val = settings.GetValueOrDefault(key, null);
                if (val == null)
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }

            TEnum answer;
            if (Enum.TryParse<TEnum>(val, out answer))
            {
                return answer;
            }

            return defaultValue;
        }

        public static bool AddOrUpdateEnum<TEnum>(this ISettings settings, string key, TEnum value) where TEnum : struct
        {
            string val = value.ToString();
            return settings.AddOrUpdateValue(key, val);
        }

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Constants

        private const string HAS_ASKED_FOR_RATING = "HasAskedForRating";
        private const string HAS_REVIEWED_OR_EMAILED_DEV = "HasReviewedOrEmailedDev";
        private const string WAS_UPDATED_BY_BACKGROUND_TASK = "WasUpdatedByBackground";
        private const string LAST_LOGIN_LOCAL_ID = "LastLogin";
        private const string LAST_SELECTED_TIME_OPTION_FOR_TASK_WITHOUT_CLASS = "LastTimeOptionTaskNoClass";
        private const string LAST_SELECTED_TIME_OPTION_FOR_EVENT_WIHTOUT_CLASS = "LastTimeOptionEventNoClass";
        private const string LAST_SELECTED_DUE_TIME_FOR_TASK_WITHOUT_CLASS = "LastDueTimeTaskNoClass";
        private const string LAST_SELECTED_START_TIME_FOR_EVENT_WITHOUT_CLASS = "LastStartTimeEventNoClass";
        private const string LAST_SELECTED_DURATION_FOR_EVENT_WITHOUT_CLASS = "LastDurationEventNoClass";
        private const string AVERAGE_IMAGE_BLOB_SAVE_SPEED_IN_BYTES_PER_SECOND = "AverageImageBlobSaveSpeedInBytesPerSecond";
        private const string AVERAGE_IMAGE_UPLOAD_SPEED_IN_BYTES_PER_SECOND = "AverageImageUploadSpeedInBytesPerSecond";
        private const string HAS_SHOWN_PROMO_CONTRIBUTE = "HasShownPromoContribute";
        private const string OWNS_IN_APP_PURCHASE = "OwnsInAppPurchase";

        #endregion

        /// <summary>
        /// Only iOS uses this, on the rest of the platforms we query the platform APIs to retrieve if they've purchased
        /// </summary>
        public static bool OwnsInAppPurchase
        {
            get => AppSettings.GetValueOrDefault(OWNS_IN_APP_PURCHASE, false);
            set
            {
                if (value)
                {
                    AppSettings.AddOrUpdateValue(OWNS_IN_APP_PURCHASE, true);
                }
                else
                {
                    AppSettings.Remove(OWNS_IN_APP_PURCHASE);
                }
            }
        }

        public static double AverageImageBlobSaveSpeedInBytesPerSecond
        {
            // Apparently the GetValue methods that should work on doubles, don't... so had to use my own methods
            get => Math.Max(AppSettings.GetDoubleOrDefault(AVERAGE_IMAGE_BLOB_SAVE_SPEED_IN_BYTES_PER_SECOND, 5000000.0), 5); // 5 MB/sec by default
            set => AppSettings.AddOrUpdateDouble(AVERAGE_IMAGE_BLOB_SAVE_SPEED_IN_BYTES_PER_SECOND, value);
        }

        /// <summary>
        /// The upload speed of simply uploading the file (doesn't count the saving of the blob)
        /// </summary>
        public static double AverageImageUploadSpeedInBytesPerSecond
        {
            get => Math.Max(AppSettings.GetDoubleOrDefault(AVERAGE_IMAGE_UPLOAD_SPEED_IN_BYTES_PER_SECOND, 3000000), 5); // 3 MB/sec by default
            set => AppSettings.AddOrUpdateDouble(AVERAGE_IMAGE_UPLOAD_SPEED_IN_BYTES_PER_SECOND, value);
        }

        /// <summary>
        /// Android uses this (and in future iOS will too). Windows doesn't use this since we'll be 
        /// </summary>
        public static bool HasAskedForRating
        {
            get
            {
                return AppSettings.GetValueOrDefault(HAS_ASKED_FOR_RATING, false);
            }

            set
            {
                if (value)
                    AppSettings.AddOrUpdateValue(HAS_ASKED_FOR_RATING, true);
                else
                    AppSettings.Remove(HAS_ASKED_FOR_RATING);
            }
        }

        /// <summary>
        /// iOS uses this. If this isn't true and HasAskedForRating is true, that implies the user clicked Not Now, which in that case we'll try showing the less intrusive in-app rate dialog
        /// </summary>
        public static bool HasReviewedOrEmailedDev
        {
            get
            {
                return AppSettings.GetValueOrDefault(HAS_REVIEWED_OR_EMAILED_DEV, false);
            }

            set
            {
                if (value)
                {
                    AppSettings.AddOrUpdateValue(HAS_REVIEWED_OR_EMAILED_DEV, true);
                }
                else
                {
                    AppSettings.Remove(HAS_REVIEWED_OR_EMAILED_DEV);
                }
            }
        }

        public static bool HasShownPromoContribute
        {
            get => AppSettings.GetValueOrDefault(HAS_SHOWN_PROMO_CONTRIBUTE, false);
            set
            {
                if (value)
                {
                    AppSettings.AddOrUpdateValue(HAS_SHOWN_PROMO_CONTRIBUTE, true);
                }
                else
                {
                    AppSettings.Remove(HAS_SHOWN_PROMO_CONTRIBUTE);
                }
            }
        }

        public static bool WasUpdatedByBackgroundTask
        {
            get
            {
                return AppSettings.GetValueOrDefault(WAS_UPDATED_BY_BACKGROUND_TASK, false);
            }

            set
            {
                if (value)
                    AppSettings.AddOrUpdateValue(WAS_UPDATED_BY_BACKGROUND_TASK, true);
                else
                    AppSettings.Remove(WAS_UPDATED_BY_BACKGROUND_TASK);
            }
        }

        public static Guid LastLoginLocalId
        {
            get
            {
                return AppSettings.GetValueOrDefault(LAST_LOGIN_LOCAL_ID, Guid.Empty);
            }

            set
            {
                AppSettings.AddOrUpdateValue(LAST_LOGIN_LOCAL_ID, value);
            }
        }

        public static string LastSelectedTimeOptionForTaskWithoutClass
        {
            get => AppSettings.GetValueOrDefault(LAST_SELECTED_TIME_OPTION_FOR_TASK_WITHOUT_CLASS, null);
            set => AppSettings.AddOrUpdateValue(LAST_SELECTED_TIME_OPTION_FOR_TASK_WITHOUT_CLASS, value);
        }

        public static string LastSelectedTimeOptionForEventWithoutClass
        {
            get => AppSettings.GetValueOrDefault(LAST_SELECTED_TIME_OPTION_FOR_EVENT_WIHTOUT_CLASS, null);
            set => AppSettings.AddOrUpdateValue(LAST_SELECTED_TIME_OPTION_FOR_EVENT_WIHTOUT_CLASS, value);
        }

        public static TimeSpan? LastSelectedDueTimeForTaskWithoutClass
        {
            get => AppSettings.GetTimeSpanOrDefault(LAST_SELECTED_DUE_TIME_FOR_TASK_WITHOUT_CLASS, null);
            set => AppSettings.AddOrUpdateTimeSpan(LAST_SELECTED_DUE_TIME_FOR_TASK_WITHOUT_CLASS, value);
        }

        public static TimeSpan? LastSelectedStartTimeForEventWithoutClass
        {
            get => AppSettings.GetTimeSpanOrDefault(LAST_SELECTED_START_TIME_FOR_EVENT_WITHOUT_CLASS, null);
            set => AppSettings.AddOrUpdateTimeSpan(LAST_SELECTED_START_TIME_FOR_EVENT_WITHOUT_CLASS, value);
        }

        public static TimeSpan? LastSelectedDurationForEventWithoutClass
        {
            get => AppSettings.GetTimeSpanOrDefault(LAST_SELECTED_DURATION_FOR_EVENT_WITHOUT_CLASS, null);
            set => AppSettings.AddOrUpdateTimeSpan(LAST_SELECTED_DURATION_FOR_EVENT_WITHOUT_CLASS, value);
        }

        private static TimeSpan? GetTimeSpanOrDefault(this ISettings settings, string key, TimeSpan? defaultValue)
        {
            string val = AppSettings.GetValueOrDefault(key, null);
            if (val != null && TimeSpan.TryParse(val, out TimeSpan result))
            {
                return result;
            }
            return defaultValue;
        }

        private static bool AddOrUpdateTimeSpan(this ISettings settings, string key, TimeSpan? value)
        {
            if (value == null)
            {
                AppSettings.Remove(key);
                return true;
            }
            else
            {
                return AppSettings.AddOrUpdateValue(key, value.Value.ToString());
            }
        }

        public static class NavigationManagerSettings
        {
            private const string MAIN_MENU_SELECTION = "NavManager_MainMenuSelection";
            private const string CLASS_SELECTION = "NavManager_ClassSelection";

            static NavigationManagerSettings()
            {
            }
    
            public static MainMenuSelections MainMenuSelection
            {
                get { return AppSettings.GetEnumOrDefault<MainMenuSelections>(MAIN_MENU_SELECTION, MainMenuSelections.Calendar); }
                set
                {
                    // Never remember settings
                    if (value == MainMenuSelections.Settings)
                        return;

                    AppSettings.AddOrUpdateEnum(MAIN_MENU_SELECTION, value);
                }
            }

            public static Guid ClassSelection
            {
                get { return AppSettings.GetValueOrDefault(CLASS_SELECTION, Guid.Empty); }
                set { AppSettings.AddOrUpdateValue(CLASS_SELECTION, value); }
            }

            public static void Clear()
            {
                AppSettings.Remove(MAIN_MENU_SELECTION);
                AppSettings.Remove(CLASS_SELECTION);
            }
        }
    }
}