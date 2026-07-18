using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary;
using InterfacesUWP;

namespace PowerPlannerUWP.Extensions
{
    public class UWPTelemetryExtension : TelemetryExtension
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentQueue<string> _pendingTelemetry = new ConcurrentQueue<string>();
        private static readonly string _instrumentationKey;
        private static readonly Uri _ingestionEndpoint;
        private static int _isSending;

        private readonly string _sessionId = Guid.NewGuid().ToString();
        private string _localUserId;
        private string _authenticatedUserId;

        static UWPTelemetryExtension()
        {
            ParseConnectionString(Secrets.AppCenterAppSecret, out _instrumentationKey, out _ingestionEndpoint);
        }

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            try
            {
                _developerLogs.Add(EventToString(eventName, properties));
                QueueTelemetry("Event", new EventTelemetryData
                {
                    BaseType = "EventData",
                    BaseData = new EventBaseData
                    {
                        Name = eventName,
                        Properties = CopyProperties(properties)
                    }
                }, UWPTelemetryJsonContext.Default.TelemetryEnvelopeEventTelemetryData);
            }
            catch { }
        }

        public override void TrackMetric(string metricId, double metricValue)
        {
            try
            {
                _developerLogs.Add("Metric: " + metricId + ":" + metricValue);
                TrackMetricCore(metricId, metricValue, null);
            }
            catch { }
        }

        public override void TrackMetric(string metricId, double metricValue, string dim1Label, string dim1Value)
        {
            string str = "Metric: " + metricId + ":" + metricValue + ":" + dim1Label + ":" + dim1Value;
            _developerLogs.Add(str);
            TrackMetricCore(metricId, metricValue, new Dictionary<string, string> { { dim1Label, dim1Value } });
        }

        public override void TrackMetric(string metricId, double metricValue, string dim1Label, string dim1Value, string dim2Label, string dim2Value)
        {
            string str = "Metric: " + metricId + ":" + metricValue + ":" + dim1Label + ":" + dim1Value + ":" + dim2Label + ":" + dim2Value;
            _developerLogs.Add(str);
            TrackMetricCore(metricId, metricValue, new Dictionary<string, string>
            {
                { dim1Label, dim1Value },
                { dim2Label, dim2Value }
            });
        }

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null, IDictionary<string, string> properties = null)
        {
            try
            {
                _developerLogs.Add(ExceptionToString(ex, exceptionName, properties));
                var exceptionProperties = CopyProperties(properties);
                if (exceptionName != null)
                {
                    exceptionProperties["ExceptionName"] = exceptionName;
                }

                QueueTelemetry("Exception", new ExceptionTelemetryData
                {
                    BaseType = "ExceptionData",
                    BaseData = new ExceptionBaseData
                    {
                        Exceptions = new[]
                        {
                            new ExceptionDetails
                            {
                                TypeName = ex.GetType().FullName,
                                Message = ex.Message,
                                Stack = ex.ToString()
                            }
                        },
                        Properties = exceptionProperties
                    }
                }, UWPTelemetryJsonContext.Default.TelemetryEnvelopeExceptionTelemetryData);
            }
            catch { }
        }

        private bool _hasStartedSession;
        public override void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);

            // Represents a local user identity. If they use two local accounts, that means there's two users, which is typically correct.
            _localUserId = account != null ? account.LocalAccountId.ToString() : null;

            // Represents an online user identity.
            _authenticatedUserId = CurrentAccountId == 0 ? null : CurrentAccountId.ToString();

            // Do NOT use User.AccountId, that's meant for things like which tenant.

            if (!_hasStartedSession)
            {
                _hasStartedSession = true;
                TrackEvent("StartSessionLog");
            }
        }

        public override void SuspendingApp()
        {
            try
            {
                _ = SendQueuedTelemetryAsync();
            }
            catch { }
        }

        private void TrackMetricCore(string metricId, double metricValue, Dictionary<string, string> properties)
        {
            QueueTelemetry("Metric", new MetricTelemetryData
            {
                BaseType = "MetricData",
                BaseData = new MetricBaseData
                {
                    Metrics = new[] { new DataPoint { Name = metricId, Value = metricValue } },
                    Properties = properties ?? new Dictionary<string, string>()
                }
            }, UWPTelemetryJsonContext.Default.TelemetryEnvelopeMetricTelemetryData);
        }

        private void QueueTelemetry<T>(string telemetryType, T data, System.Text.Json.Serialization.Metadata.JsonTypeInfo<TelemetryEnvelope<T>> jsonTypeInfo)
        {
            if (string.IsNullOrWhiteSpace(_instrumentationKey) || _ingestionEndpoint == null)
            {
                return;
            }

            var envelope = new TelemetryEnvelope<T>
            {
                Name = "Microsoft.ApplicationInsights." + _instrumentationKey.Replace("-", string.Empty) + "." + telemetryType,
                Time = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                InstrumentationKey = _instrumentationKey,
                Tags = CreateTags(),
                Data = data
            };

            _pendingTelemetry.Enqueue(JsonSerializer.Serialize(envelope, jsonTypeInfo));
            _ = SendQueuedTelemetryAsync();
        }

        private Dictionary<string, string> CreateTags()
        {
            var tags = new Dictionary<string, string>
            {
                { "ai.application.ver", Variables.VERSION.ToString() },
                { "ai.device.os", "Windows" },
                { "ai.device.osVersion", "10.0." + DeviceInfo.BuildNumber },
                { "ai.session.id", _sessionId }
            };

            if (_localUserId != null)
            {
                tags["ai.user.id"] = _localUserId;
            }

            if (_authenticatedUserId != null)
            {
                tags["ai.user.authUserId"] = _authenticatedUserId;
            }

            return tags;
        }

        private static async Task SendQueuedTelemetryAsync()
        {
            if (Interlocked.CompareExchange(ref _isSending, 1, 0) != 0)
            {
                return;
            }

            try
            {
                while (_pendingTelemetry.TryPeek(out string payload))
                {
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    using (var response = await _httpClient.PostAsync(_ingestionEndpoint, content).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            return;
                        }
                    }

                    _pendingTelemetry.TryDequeue(out _);
                }
            }
            catch
            {
            }
            finally
            {
                Interlocked.Exchange(ref _isSending, 0);
            }
        }

        private static Dictionary<string, string> CopyProperties(IDictionary<string, string> properties)
        {
            return properties == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(properties);
        }

        private static void ParseConnectionString(string connectionString, out string instrumentationKey, out Uri ingestionEndpoint)
        {
            instrumentationKey = null;
            string endpoint = null;

            foreach (string part in (connectionString ?? string.Empty).Split(';'))
            {
                int separatorIndex = part.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = part.Substring(0, separatorIndex).Trim();
                string value = part.Substring(separatorIndex + 1).Trim();
                if (key.Equals("InstrumentationKey", StringComparison.OrdinalIgnoreCase))
                {
                    instrumentationKey = value;
                }
                else if (key.Equals("IngestionEndpoint", StringComparison.OrdinalIgnoreCase))
                {
                    endpoint = value;
                }
            }

            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                ingestionEndpoint = null;
                return;
            }

            endpoint = string.IsNullOrWhiteSpace(endpoint) ? "https://dc.services.visualstudio.com/" : endpoint;
            ingestionEndpoint = new Uri(new Uri(endpoint.EndsWith("/") ? endpoint : endpoint + "/"), "v2/track");
        }

        private List<string> _developerLogs = new List<string>();
        public override string GetDeveloperLogs()
        {
            return string.Join("\n", _developerLogs);
        }
    }

    internal sealed class TelemetryEnvelope<T>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("iKey")]
        public string InstrumentationKey { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string> Tags { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    internal sealed class EventTelemetryData
    {
        [JsonPropertyName("baseType")]
        public string BaseType { get; set; }

        [JsonPropertyName("baseData")]
        public EventBaseData BaseData { get; set; }
    }

    internal sealed class EventBaseData
    {
        [JsonPropertyName("ver")]
        public int Version { get; set; } = 2;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; }
    }

    internal sealed class ExceptionTelemetryData
    {
        [JsonPropertyName("baseType")]
        public string BaseType { get; set; }

        [JsonPropertyName("baseData")]
        public ExceptionBaseData BaseData { get; set; }
    }

    internal sealed class ExceptionBaseData
    {
        [JsonPropertyName("ver")]
        public int Version { get; set; } = 2;

        [JsonPropertyName("exceptions")]
        public ExceptionDetails[] Exceptions { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; }
    }

    internal sealed class ExceptionDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 1;

        [JsonPropertyName("outerId")]
        public int OuterId { get; set; }

        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("hasFullStack")]
        public bool HasFullStack { get; set; } = true;

        [JsonPropertyName("stack")]
        public string Stack { get; set; }
    }

    internal sealed class MetricTelemetryData
    {
        [JsonPropertyName("baseType")]
        public string BaseType { get; set; }

        [JsonPropertyName("baseData")]
        public MetricBaseData BaseData { get; set; }
    }

    internal sealed class MetricBaseData
    {
        [JsonPropertyName("ver")]
        public int Version { get; set; } = 2;

        [JsonPropertyName("metrics")]
        public DataPoint[] Metrics { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; }
    }

    internal sealed class DataPoint
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("kind")]
        public int Kind { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(TelemetryEnvelope<EventTelemetryData>))]
    [JsonSerializable(typeof(TelemetryEnvelope<ExceptionTelemetryData>))]
    [JsonSerializable(typeof(TelemetryEnvelope<MetricTelemetryData>))]
    internal partial class UWPTelemetryJsonContext : JsonSerializerContext
    {
    }
}
