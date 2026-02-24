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
        public override async void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            try
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                {
                    // On API 31+, use RemoteCollectionItems (the modern API).
                    // Use GoAsync() so the broadcast receiver isn't killed while we load data.
                    var pendingResult = GoAsync();
                    try
                    {
                        var data = await WidgetAgendaService.LoadDataAsync(context);
                        UpdateWithRemoteCollectionItems(context, appWidgetManager, appWidgetIds, data);
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                    finally
                    {
                        try
                        {
                            // Sometimes throws ObjectDisposedException
                            pendingResult.Finish();
                        }
                        catch (ObjectDisposedException) { }
                    }
                }
                else
                {
                    UpdateWithRemoteAdapter(context, appWidgetManager, appWidgetIds);
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            TelemetryExtension.Current?.TrackEvent("Widget_Agenda_OnUpdate");
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
        }

        [System.Runtime.Versioning.SupportedOSPlatform("android31.0")]
        private void UpdateWithRemoteCollectionItems(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds, WidgetAgendaService.WidgetAgendaData data)
        {
            foreach (var appWidgetId in appWidgetIds)
            {
                var views = CreateBaseRemoteViews(context, appWidgetId);

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
            }
        }

        [System.Runtime.Versioning.ObsoletedOSPlatform("android35.0")]
        private static void UpdateWithRemoteAdapter(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            foreach (var appWidgetId in appWidgetIds)
            {
                Intent intent = new Intent(context, typeof(WidgetAgendaService));
                intent.SetPackage(context.PackageName);
                intent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
                intent.SetData(Android.Net.Uri.Parse(intent.ToUri(Android.Content.IntentUriType.AndroidAppScheme)));

                var views = CreateBaseRemoteViews(context, appWidgetId);

                views.SetRemoteAdapter(Resource.Id.WidgetAgendaListView, intent);

                appWidgetManager.UpdateAppWidget(appWidgetId, views);
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