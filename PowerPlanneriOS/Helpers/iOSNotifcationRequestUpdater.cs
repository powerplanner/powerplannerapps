using HomeKit;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserNotifications;
using Vx.Helpers;

namespace PowerPlanneriOS.Helpers
{
    internal class iOSNotifcationRequestUpdater
    {
        public static async Task MakePendingNotificationRequestsLikeAsync(UNUserNotificationCenter notificationCenter, string[] identifierStartsWith, Dictionary<string, iOSNotificationRequest> requests)
        {
            try
            {
                var existingTask = notificationCenter.GetPendingNotificationRequestsAsync();
                var existing = existingTask == null ? null : await existingTask;
                existing = existing ?? [];

                Dictionary<string, iOSNotificationRequest> existingRequests = existing
                    .Where(i => identifierStartsWith.Any(start => i.Identifier.StartsWith(start)))
                    .ToDictionary(i => i.Identifier, i => new iOSNotificationRequest
                    {
                        Identifier = i.Identifier,
                        Title = i.Content.Title,
                        Subtitle = i.Content.Subtitle,
                        Body = i.Content.Body,
                        DeliveryTime = i.Trigger is UNCalendarNotificationTrigger calendarTrigger && calendarTrigger.NextTriggerDate != null ? (DateTime)calendarTrigger.NextTriggerDate : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
                    });

                ReconcileDictionary.Reconcile(
                    existingRequests,
                    requests,
                    out HashSet<string> toRemove,
                    out Dictionary<string, iOSNotificationRequest> toUpdate,
                    out Dictionary<string, iOSNotificationRequest> toAdd);

                if (toRemove.Count > 0)
                {
                    notificationCenter.RemovePendingNotificationRequests(toRemove.ToArray());
                }

                foreach (var update in toUpdate.Values)
                {
                    await notificationCenter.AddNotificationRequestAsync(update.ToRequest());
                }

                foreach (var add in toAdd.Values)
                {
                    double seconds = (add.DeliveryTime - DateTime.Now).TotalSeconds;
                    if (seconds < 5)
                    {
                        continue;
                    }

                    await notificationCenter.AddNotificationRequestAsync(add.ToRequest());
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }

    internal class iOSNotificationRequest : IEquatable<iOSNotificationRequest>
    {
        public string Identifier { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Body { get; set; }
        public DateTime DeliveryTime { get; set; }

        public UNNotificationContent ToContent()
        {
            var content = new UNMutableNotificationContent
            {
                Title = Title
            };

            if (Body != null)
            {
                content.Body = Body;
            }

            if (Subtitle != null)
            {
                content.Subtitle = Subtitle;
            }

            return content;
        }

        public UNTimeIntervalNotificationTrigger ToTrigger()
        {
            double seconds = (DeliveryTime - DateTime.Now).TotalSeconds;
            return UNTimeIntervalNotificationTrigger.CreateTrigger(seconds, false);
        }

        public UNNotificationRequest ToRequest()
        {
            return UNNotificationRequest.FromIdentifier(Identifier, ToContent(), ToTrigger());
        }

        public bool Equals(iOSNotificationRequest other)
        {
            return object.Equals(Identifier, other.Identifier) &&
                   object.Equals(Title, other.Title) &&
                   object.Equals(Subtitle, other.Subtitle) &&
                   object.Equals(Body, other.Body) &&
                   object.Equals(DeliveryTime.ToUniversalTime(), other.DeliveryTime.ToUniversalTime());
        }

        public override bool Equals(object obj)
        {
            if (obj is iOSNotificationRequest other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier, Title, Subtitle, Body, DeliveryTime.ToUniversalTime());
        }
    }
}
