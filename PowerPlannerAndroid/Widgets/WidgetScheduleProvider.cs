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
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAndroid.Services;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Widgets
{
    [BroadcastReceiver(Label = "Schedule", Exported = true)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/widgetscheduleinfo")]
    public class WidgetScheduleProvider : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            try
            {
                foreach (var appWidgetId in appWidgetIds)
                {
                    // Set up the intent that starts the widget schedule items service, which will provide the views for the collection
                    Intent intent = new Intent(context, typeof(WidgetScheduleService));
                    intent.SetPackage(context.PackageName);

                    // Add the app widget ID to the intent extras
                    intent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
                    intent.SetData(Android.Net.Uri.Parse(intent.ToUri(Android.Content.IntentUriType.AndroidAppScheme)));

                    // Instantiate the widget layout
                    var views = new RemoteViews(context.PackageName, Resource.Layout.WidgetSchedule);

                    // Localize header text
                    views.SetTextViewText(Resource.Id.WidgetScheduleHeaderText, PowerPlannerResources.GetString("MainMenuItem_Schedule"));

                    // This is displayed while the list view loads.
                    // My list view will always have items, so this will act as loading text.
                    views.SetEmptyView(Resource.Id.WidgetScheduleListView, Resource.Id.WidgetScheduleEmptyView);

                    // Wire the header click
                    Intent openIntent = new Intent(context, typeof(MainActivity))
                        .SetAction(Intent.ActionView)
                        .SetData(Android.Net.Uri.Parse("powerplanner:?" + new ViewScheduleArguments()
                        {
                            LaunchSurface = LaunchSurface.SecondaryTile
                        }.SerializeToString()));
                    PendingIntent pendingOpenIntent = PendingIntent.GetActivity(context, 0, openIntent, 0);
                    views.SetOnClickPendingIntent(Resource.Id.WidgetScheduleHeader, pendingOpenIntent);

                    // Set up the list adapter to use our service that generates the items
                    views.SetRemoteAdapter(Resource.Id.WidgetScheduleListView, intent);

                    // Add click handlers to each individual item
                    Intent itemClickIntent = new Intent(context, typeof(MainActivity));
                    itemClickIntent.SetAction(Intent.ActionView);
                    itemClickIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
                    PendingIntent pendingItemClickIntent = PendingIntent.GetActivity(context, 0, itemClickIntent, 0);
                    views.SetPendingIntentTemplate(Resource.Id.WidgetScheduleListView, pendingItemClickIntent);

                    // Update the widget
                    appWidgetManager.UpdateAppWidget(appWidgetId, views);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current.TrackException(ex);
            }

            TelemetryExtension.Current?.TrackEvent("Widget_Schedule_OnUpdate");
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
        }
    }
}