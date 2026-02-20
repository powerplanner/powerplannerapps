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
using PowerPlannerAndroid.Services;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.Widgets
{
    [BroadcastReceiver(Label = "Agenda", Exported = true)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/widgetagendainfo")]
    public class WidgetAgendaProvider : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            try
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                {
                    // On API 31+, use RemoteCollectionItems. Data loading is handled
                    // asynchronously in OnReceive via GoAsync(), so by the time OnUpdate
                    // is called the data is already loaded and stored in _pendingData.
                    UpdateWithRemoteCollectionItems(context, appWidgetManager, appWidgetIds);
                }
                else
                {
                    UpdateWithRemoteAdapter(context, appWidgetManager, appWidgetIds);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current.TrackException(ex);
            }

            TelemetryExtension.Current?.TrackEvent("Widget_Agenda_OnUpdate");
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
        }

        [ThreadStatic]
        private static WidgetAgendaService.WidgetAgendaData _pendingData;

        public override void OnReceive(Context context, Intent intent)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(31)
                && intent.Action == AppWidgetManager.ActionAppwidgetUpdate)
            {
                // Use GoAsync() so the broadcast receiver isn't killed while we load data
                var pendingResult = GoAsync();
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        _pendingData = await WidgetAgendaService.LoadAgendaDataAsync(context);
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                        _pendingData = null;
                    }

                    try
                    {
                        // Call base which will invoke OnUpdate on this thread
                        base.OnReceive(context, intent);
                    }
                    finally
                    {
                        _pendingData = null;
                        pendingResult.Finish();
                    }
                });
            }
            else
            {
                base.OnReceive(context, intent);
            }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("android31.0")]
        private static void UpdateWithRemoteCollectionItems(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var data = _pendingData;

            foreach (var appWidgetId in appWidgetIds)
            {
                var views = CreateBaseRemoteViews(context, appWidgetId);

                // Build the collection items (always set an adapter, even if empty, to clear "Loading..." state)
                var builder = new RemoteViews.RemoteCollectionItems.Builder();
                builder.SetHasStableIds(false);
                builder.SetViewTypeCount(3);

                if (data?.Items != null)
                {
                    for (int i = 0; i < data.Items.Count; i++)
                    {
                        var itemView = WidgetAgendaService.CreateRemoteViewForItem(context.PackageName, data.Items[i], data.Now, data.LocalAccountId);
                        builder.AddItem(i, itemView);
                    }
                }

                views.SetRemoteAdapter(Resource.Id.WidgetAgendaListView, builder.Build());

                appWidgetManager.UpdateAppWidget(appWidgetId, views);
                Android.Util.Log.Debug("WidgetAgendaProvider", "Updated Widget (RemoteCollectionItems)");
            }
        }

        private static void UpdateWithRemoteAdapter(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            foreach (var appWidgetId in appWidgetIds)
            {
                // Set up the intent that starts the widget agenda items service, which will provide the views for the collection
                Intent intent = new Intent(context, typeof(WidgetAgendaService));
                intent.SetPackage(context.PackageName);

                // Add the app widget ID to the intent extras
                intent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
                intent.SetData(Android.Net.Uri.Parse(intent.ToUri(Android.Content.IntentUriType.AndroidAppScheme)));

                var views = CreateBaseRemoteViews(context, appWidgetId);

                // Set up the list adapter to use our service that generates the items
#pragma warning disable CA1422 // Validate platform compatibility - needed for API < 31
                views.SetRemoteAdapter(Resource.Id.WidgetAgendaListView, intent);
#pragma warning restore CA1422

                // Update the widget
                appWidgetManager.UpdateAppWidget(appWidgetId, views);
                Android.Util.Log.Debug("WidgetAgendaProvider", "Updated Widget (RemoteAdapter)");
            }
        }

        private static RemoteViews CreateBaseRemoteViews(Context context, int appWidgetId)
        {
            // Instantiate the widget layout
            var views = new RemoteViews(context.PackageName, Resource.Layout.WidgetAgenda);

            // This is displayed while the list view loads.
            // My list view will always have items, so this will act as loading text.
            views.SetEmptyView(Resource.Id.WidgetAgendaListView, Resource.Id.WidgetAgendaEmptyView);

            // Wire the header click
            Intent openIntent = new Intent(context, typeof(MainActivity))
                .SetAction(Intent.ActionView);
            PendingIntent pendingOpenIntent = PendingIntent.GetActivity(context, 0, openIntent, PendingIntentFlags.Immutable);
            views.SetOnClickPendingIntent(Resource.Id.WidgetAgendaHeaderBranding, pendingOpenIntent);

            // Wire the "+" click
            Intent addIntent = new Intent(context, typeof(MainActivity))
                .SetAction(Intent.ActionView)
                .SetData(Android.Net.Uri.Parse("powerplanner:?" + new QuickAddToCurrentAccountArguments().SerializeToString()));
            PendingIntent pendingAddIntent = PendingIntent.GetActivity(context, 0, addIntent, PendingIntentFlags.Immutable);
            views.SetOnClickPendingIntent(Resource.Id.WidgetAgendaHeaderAddButton, pendingAddIntent);

            // Add click handlers to each individual item
            Intent itemClickIntent = new Intent(context, typeof(MainActivity));
            itemClickIntent.SetAction(Intent.ActionView);
            itemClickIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
            PendingIntent pendingItemClickIntent = PendingIntent.GetActivity(context, 0, itemClickIntent, PendingIntentFlags.Immutable);
            views.SetPendingIntentTemplate(Resource.Id.WidgetAgendaListView, pendingItemClickIntent);

            return views;
        }
    }
}