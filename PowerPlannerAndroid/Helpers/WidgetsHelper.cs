using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Appwidget;
using PowerPlannerAndroid.Widgets;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.Helpers
{
    public static class WidgetsHelper
    {
        private static AppWidgetManager _appWidgetManager;
        private static AppWidgetManager AppWidgetManager
        {
            get
            {
                if (_appWidgetManager == null)
                {
                    _appWidgetManager = AppWidgetManager.GetInstance(Application.Context);
                }

                return _appWidgetManager;
            }
        }

        public static void UpdateAllWidgets()
        {
            try
            {
                UpdateAgendaWidget();
                UpdateScheduleWidget();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static void UpdateAgendaWidget()
        {
            try
            {
                int[] widgetIds = AppWidgetManager.GetAppWidgetIds(new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(WidgetAgendaProvider)).Name));

                foreach (var id in widgetIds)
                {
                    AppWidgetManager.NotifyAppWidgetViewDataChanged(id, Resource.Id.WidgetAgendaListView);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static void UpdateScheduleWidget()
        {
            try
            {
                int[] widgetIds = AppWidgetManager.GetAppWidgetIds(new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(WidgetScheduleProvider)).Name));

                foreach (var id in widgetIds)
                {
                    AppWidgetManager.NotifyAppWidgetViewDataChanged(id, Resource.Id.WidgetScheduleListView);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}