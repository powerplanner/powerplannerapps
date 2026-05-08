using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.Services
{
    public class AiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "https://powerplannerai-gjfgfzfecwhhapey.westus-01.azurewebsites.net/api/ai/generate-items";
        //private const string ApiUrl = "http://localhost:7074/api/ai/generate-items";

        public string DescriptionOfChanges { get; private set; }

        public const int LIMIT = 100;

        public async Task<List<AiProposedChange>> GenerateChangesAsync(
            string userInput,
            MyObservableList<ViewItemClass> classes,
            ViewItemSemester semester,
            SemesterItemsViewGroup semesterItems,
            DateOnly displayMonth,
            string accountToken)
        {
            if (userInput == "Move my overdue items to today")
            {
                var overdue = semesterItems.Items.OfType<ViewItemTaskOrEvent>()
                    .Where(i => i.DateInSchoolTime.Date < DateTime.Today && !i.IsComplete)
                    .ToList();

                await Task.Delay(50);

                return overdue.Select(i => new AiProposedChange
                {
                    Operation = AiChangeOperation.Edit,
                    ExistingItemId = i.Identifier,
                    Date = DateOnly.FromDateTime(DateTime.Today)
                }).ToList();
            }

            // Build ID mappings
            var guidToInt = new Dictionary<Guid, int>();
            var intToGuid = new Dictionary<int, Guid>();

            // 0 = semester (No Class)
            guidToInt[semester.Identifier] = 0;
            intToGuid[0] = semester.Identifier;

            int nextId = 1;

            // Map classes
            foreach (var c in classes)
            {
                if (c.IsNoClassClass)
                    continue;

                guidToInt[c.Identifier] = nextId;
                intToGuid[nextId] = c.Identifier;
                nextId++;
            }

            // Map existing items
            await semesterItems.LoadingTask;
            foreach (var item in semesterItems.Items)
            {
                if (!guidToInt.ContainsKey(item.Identifier))
                {
                    guidToInt[item.Identifier] = nextId;
                    intToGuid[nextId] = item.Identifier;
                    nextId++;
                }
            }

            // Build request
            var classInfos = new List<ClassInfo>();
            classInfos.Add(new ClassInfo { Id = 0, Name = "No Class", Schedules = new List<ScheduleInfo>() });

            foreach (var c in classes)
            {
                if (c.IsNoClassClass)
                    continue;

                var schedules = new List<ScheduleInfo>();
                if (c.Schedules != null)
                {
                    foreach (var s in c.Schedules)
                    {
                        schedules.Add(new ScheduleInfo
                        {
                            DayOfWeek = s.DayOfWeek,
                            StartTime = s.StartTimeInSchoolTime.TimeOfDay,
                            EndTime = s.EndTimeInSchoolTime.TimeOfDay
                        });
                    }
                }

                classInfos.Add(new ClassInfo
                {
                    Id = guidToInt[c.Identifier],
                    Name = c.Name,
                    Schedules = schedules
                });
            }

            // Build existing items (up to limit, sorted by date proximity to today)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var existingItemInfos = new List<ExistingItemInfo>();
            var existingCompletedItemInfos = new List<ExistingItemInfo>();

            foreach (var item in semesterItems.Items
                .OrderBy(i => Math.Abs((i.DateInSchoolTime.Date - DateTime.Today).Days))
                .Take(LIMIT))
            {
                var info = new ExistingItemInfo
                {
                    Id = guidToInt[item.Identifier],
                    Name = item.Name,
                    Date = DateOnly.FromDateTime(item.DateInSchoolTime.Date),
                    Details = item.Details
                };

                if (item is ViewItemTaskOrEvent taskOrEvent)
                {
                    info.Type = taskOrEvent.Type == TaskOrEventType.Task ? ItemType.Task : ItemType.Event;
                    info.ClassId = guidToInt.ContainsKey(taskOrEvent.Class.Identifier) ? guidToInt[taskOrEvent.Class.Identifier] : 0;

                    if (taskOrEvent.Type == TaskOrEventType.Task)
                    {
                        info.PercentComplete = taskOrEvent.PercentComplete;
                    }

                    if (taskOrEvent.IsComplete)
                    {
                        existingCompletedItemInfos.Add(info);
                    }
                    else
                    {
                        existingItemInfos.Add(info);
                    }
                }
                else if (item is ViewItemHoliday holiday)
                {
                    info.Type = ItemType.Holiday;
                    info.ClassId = 0;
                    info.EndDate = DateOnly.FromDateTime(holiday.EndTime.Date);

                    if (holiday.EndTime < DateTime.Today)
                    {
                        existingCompletedItemInfos.Add(info);
                    }
                    else
                    {
                        existingItemInfos.Add(info);
                    }
                }
            }

            var request = new GenerateItemsRequest
            {
                UserInput = userInput,
                Context = new RequestContext
                {
                    Today = today,
                    TodayDayOfWeek = DateTime.Today.DayOfWeek,
                    DisplayMonth = displayMonth,
                    Classes = classInfos,
                    ExistingItems = existingItemInfos,
                    ExistingCompletedItems = existingCompletedItemInfos
                }
            };

            // Make HTTP call
            var json = JsonSerializer.Serialize(request, AiServiceJsonContext.Default.GenerateItemsRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            httpRequest.Content = content;
            if (!string.IsNullOrEmpty(accountToken))
            {
                httpRequest.Headers.Add("Authorization", $"Bearer {accountToken}");
            }

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize(responseJson, AiServiceJsonContext.Default.GenerateItemsResponse);

            // Map response
            DescriptionOfChanges = result.DescriptionOfChanges;

            var proposedChanges = new List<AiProposedChange>();
            if (result.Changes != null)
            {
                foreach (var change in result.Changes)
                {
                    var proposed = new AiProposedChange
                    {
                        Name = change.Name,
                        Time = change.Time,
                        EndTime = change.EndTime,
                        Details = change.Details,
                        PercentComplete = change.PercentComplete,
                        IsSelected = true
                    };

                    // Map operation
                    proposed.Operation = change.Operation switch
                    {
                        OperationType.Edit => AiChangeOperation.Edit,
                        OperationType.Delete => AiChangeOperation.Delete,
                        _ => AiChangeOperation.Add
                    };

                    // Map type
                    if (change.Type.HasValue)
                    {
                        proposed.Type = change.Type.Value switch
                        {
                            ItemType.Task => AiItemType.Task,
                            ItemType.Event => AiItemType.Event,
                            ItemType.Holiday => AiItemType.Holiday,
                            _ => null
                        };
                    }

                    // Map dates
                    if (change.Date.HasValue)
                    {
                        proposed.Date = change.Date.Value;
                    }
                    if (change.EndDate.HasValue)
                    {
                        proposed.EndDate = change.EndDate.Value;
                    }

                    // Map existingItemId
                    if (change.ExistingItemId.HasValue && intToGuid.TryGetValue(change.ExistingItemId.Value, out var existingGuid))
                    {
                        proposed.ExistingItemId = existingGuid;
                    }

                    // Map classId
                    if (change.ClassId.HasValue && intToGuid.TryGetValue(change.ClassId.Value, out var classGuid))
                    {
                        proposed.ClassId = classGuid;
                    }

                    proposedChanges.Add(proposed);
                }
            }

            return proposedChanges;
        }

        #region Request/Response DTOs

        internal enum ItemType { Task, Event, Holiday }

        internal enum OperationType { Add, Edit, Delete }

        internal class GenerateItemsRequest
        {
            [JsonPropertyName("userInput")]
            public string UserInput { get; set; } = "";

            [JsonPropertyName("context")]
            public RequestContext Context { get; set; }
        }

        internal class RequestContext
        {
            [JsonPropertyName("today")]
            public DateOnly? Today { get; set; }

            [JsonPropertyName("todayDayOfWeek")]
            public DayOfWeek? TodayDayOfWeek { get; set; }

            [JsonPropertyName("displayMonth")]
            public DateOnly? DisplayMonth { get; set; }

            [JsonPropertyName("classes")]
            public List<ClassInfo> Classes { get; set; } = new List<ClassInfo>();

            [JsonPropertyName("existingItems")]
            public List<ExistingItemInfo> ExistingItems { get; set; }

            [JsonPropertyName("existingCompletedItems")]
            public List<ExistingItemInfo> ExistingCompletedItems { get; set; }
        }

        internal class ClassInfo
        {
            [JsonPropertyName("id")]
            public int? Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("schedules")]
            public List<ScheduleInfo> Schedules { get; set; }
        }

        internal class ScheduleInfo
        {
            [JsonPropertyName("dayOfWeek")]
            public DayOfWeek DayOfWeek { get; set; }

            [JsonPropertyName("startTime")]
            public TimeSpan? StartTime { get; set; }

            [JsonPropertyName("endTime")]
            public TimeSpan? EndTime { get; set; }
        }

        internal class ItemInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public ItemType? Type { get; set; }

            [JsonPropertyName("date")]
            public DateOnly? Date { get; set; }

            [JsonPropertyName("endDate")]
            public DateOnly? EndDate { get; set; }

            [JsonPropertyName("time")]
            public string Time { get; set; }

            [JsonPropertyName("endTime")]
            public string EndTime { get; set; }

            [JsonPropertyName("percentComplete")]
            public double? PercentComplete { get; set; }

            [JsonPropertyName("classId")]
            public int? ClassId { get; set; }

            [JsonPropertyName("details")]
            public string Details { get; set; }
        }

        internal class ExistingItemInfo : ItemInfo
        {
            [JsonPropertyName("id")]
            public int? Id { get; set; }
        }

        internal class GenerateItemsResponse
        {
            [JsonPropertyName("changes")]
            public List<ProposedChange> Changes { get; set; } = new();

            [JsonPropertyName("descriptionOfChanges")]
            public string DescriptionOfChanges { get; set; } = "";
        }

        internal class ProposedChange : ItemInfo
        {
            [JsonPropertyName("operation")]
            public OperationType Operation { get; set; }

            [JsonPropertyName("existingItemId")]
            public int? ExistingItemId { get; set; }
        }

        #endregion
    }

    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(AiService.GenerateItemsRequest))]
    [JsonSerializable(typeof(AiService.GenerateItemsResponse))]
    internal partial class AiServiceJsonContext : JsonSerializerContext
    {
    }
}
