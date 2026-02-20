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

                if (widgetIds.Length == 0)
                {
                    return;
                }

                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                {
                    // On API 31+, we use RemoteCollectionItems instead of RemoteViewsService,
                    // so trigger a full widget update via broadcast which will reload data
                    // in OnUpdate and build RemoteCollectionItems.
                    SendUpdateBroadcast<WidgetAgendaProvider>(widgetIds);
                }
                else
                {
                    // On older APIs, notify the RemoteViewsService to reload data via OnDataSetChanged
                    foreach (var id in widgetIds)
                    {
                        AppWidgetManager.NotifyAppWidgetViewDataChanged(id, Resource.Id.WidgetAgendaListView);
                    }
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

                if (widgetIds.Length == 0)
                {
                    return;
                }

                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                {
                    SendUpdateBroadcast<WidgetScheduleProvider>(widgetIds);
                }
                else
                {
                    foreach (var id in widgetIds)
                    {
                        AppWidgetManager.NotifyAppWidgetViewDataChanged(id, Resource.Id.WidgetScheduleListView);
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void SendUpdateBroadcast<TProvider>(int[] widgetIds) where TProvider : AppWidgetProvider
        {
            var context = Application.Context;
            var intent = new Intent(context, typeof(TProvider));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, widgetIds);
            context.SendBroadcast(intent);
        }
    }
}