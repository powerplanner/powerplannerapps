using Microsoft.Extensions.AI;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ToolsPortable;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModelState;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat
{
    public class ChatViewModelState : BindableBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string TurnUrl = "https://powerplannerai-gjfgfzfecwhhapey.westus-01.azurewebsites.net/api/v1/ai/chat/turn";
        //private const string TurnUrl = "http://localhost:7074/api/v1/ai/chat/turn";
        private readonly MainScreenViewModel _mainScreenViewModel;

        public ObservableCollection<ChatMessage> Messages { get; private init; } = new([new ChatMessage(ChatRole.Assistant, "How can I help you today?")]);

        public string CurrentInput { get; private set; }

        public void SetCurrentInputSilently(string value)
        {
            CurrentInput = value;
        }

        public void SetCurrentInput(string value)
        {
            if (CurrentInput != value)
            {
                CurrentInput = value;
                OnPropertyChanged(nameof(CurrentInput));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value, nameof(IsLoading));
        }

        public ChatViewModelState(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;

            LocalTools =
            [
                new NavigateToPageTool(_mainScreenViewModel),
                new GetClassesTool(_mainScreenViewModel),
                new GetTasksAndEventsTool(_mainScreenViewModel),
                new GetHolidaysTool(_mainScreenViewModel),
                new GetGradesTool(_mainScreenViewModel),

                // Dislay items
                new DisplayItemTool(_mainScreenViewModel)
                {
                    ItemType = PowerPlannerSending.ItemType.Class,
                    ItemName = "class",
                    DisplayName = "Open class"
                },

                new DisplayItemTool(_mainScreenViewModel)
                {
                    ItemType = PowerPlannerSending.ItemType.MegaItem,
                    ItemName = "task_or_event",
                    DisplayName = "Open task or event"
                },

                new DisplayItemTool(_mainScreenViewModel)
                {
                    ItemType = PowerPlannerSending.ItemType.MegaItem,
                    ItemName = "holiday",
                    DisplayName = "Open holiday"
                },

                // Add classes
                new AddClassesTool(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add classes"
                },

                // Edit classes
                new AddOrEditItemsTool<AddOrEditClassDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit classes"
                },

                // Add tasks
                new AddOrEditItemsTool<AddOrEditTaskDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add tasks"
                },

                // Edit tasks
                new AddOrEditItemsTool<AddOrEditTaskDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit tasks"
                },

                // Add events
                new AddOrEditItemsTool<AddOrEditEventDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add events"
                },

                // Edit events
                new AddOrEditItemsTool<AddOrEditEventDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit events"
                },

                // Add holidays
                new AddOrEditItemsTool<AddOrEditHolidayDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add holidays"
                },

                // Edit holidays
                new AddOrEditItemsTool<AddOrEditHolidayDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit holidays"
                },

                // Add weight categories
                new AddOrEditItemsTool<AddOrEditWeightCategoryDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add weight categories"
                },

                // Edit weight categories
                new AddOrEditItemsTool<AddOrEditWeightCategoryDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit weight categories"
                },

                // Add schedules
                new AddOrEditItemsTool<AddOrEditScheduleDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add schedules"
                },

                // Edit schedules
                new AddOrEditItemsTool<AddOrEditScheduleDataItem>(_mainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit schedules"
                },

                // Delete items
                new DeleteItemsTool(_mainScreenViewModel)
                {
                    PluralItemTypeName = "classes",
                    DisplayName = "Delete classes"
                },

                new DeleteItemsTool(_mainScreenViewModel)
                {
                    PluralItemTypeName = "tasks_and_events",
                    DisplayName = "Delete tasks and events"
                },

                new DeleteItemsTool(_mainScreenViewModel)
                {
                    PluralItemTypeName = "holidays",
                    DisplayName = "Delete holidays"
                },

                new DeleteItemsTool(_mainScreenViewModel)
                {
                    PluralItemTypeName = "weight_categories",
                    DisplayName = "Delete weight categories"
                },

                new DeleteItemsTool(_mainScreenViewModel)
                {
                    PluralItemTypeName = "grades",
                    DisplayName = "Delete grades"
                }
            ];
        }

        internal LocalTool GetLocalTool(string toolName)
        {
            return LocalTools.FirstOrDefault(i => i.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        }

        public object GetToolResult(FunctionCallContent functionCall)
        {
            return Messages.SelectMany(m => m.Contents).OfType<FunctionResultContent>().FirstOrDefault(i => i.CallId == functionCall.CallId);
        }

        private FunctionCallContent GetToolCall(string toolCallId)
        {
            return Messages.SelectMany(m => m.Contents).OfType<FunctionCallContent>().FirstOrDefault(i => i.CallId == toolCallId);
        }

        public async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentInput))
            {
                new PortableMessageDialog("Please enter a message before sending.", "Error").Show();
                return;
            }

            Messages.Add(new ChatMessage(ChatRole.User, CurrentInput));
            SetCurrentInput(string.Empty);
            IsLoading = true;

            try
            {
                await SendMessageAndHandleResponseAsync();
            }
            catch (Exception ex)
            {
                new PortableMessageDialog($"Error sending message: {ex.Message}", "Error").Show();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private LocalTool[] LocalTools;

        private async Task SendMessageAndHandleResponseAsync()
        {
            var response = await GetResponseAsync();
            await HandleResponseAsync(response);
        }

        private async Task HandleResponseAsync(ChatTurnResponse response)
        {
            if (response.Messages != null)
            {
                var toolCalls = response.Messages.SelectMany(i => i.Contents).OfType<FunctionCallContent>().ToArray();

                foreach (var m in response.Messages)
                {
                    Messages.Add(m);
                }

                await HandleToolCallsAsync(toolCalls);

                // If we had tool calls, we need to repeat the cycle now that we've responded to them
                if (toolCalls.Any())
                {
                    await SendMessageAndHandleResponseAsync();
                }
            }
        }

        /// <summary>
        /// Processes a batch of tool calls, executing calls that do not require approval and queuing approval-required
        /// calls for later handling.
        /// </summary>
        /// <remarks>Calls mapped to local tools that require approval are tracked as pending approval and
        /// are not executed in this operation.</remarks>
        /// <param name="functionCalls">The function calls to evaluate for local approval requirements and immediate execution.</param>
        /// <returns><see langword="true"/> when all calls were handled immediately; <see langword="false"/> when one or more
        /// calls were deferred for approval.</returns>
        private async Task HandleToolCallsAsync(FunctionCallContent[] functionCalls)
        {
            foreach (var call in functionCalls)
            {
                var localTool = GetLocalTool(call.Name);
                if (localTool != null && localTool.RequiresApproval)
                {
                    if (!await RequestApprovalAsync(call))
                    {
                        AddToolCallResponse(call.CallId, "Denied by the user.");
                        return;
                    }
                }

                await HandleToolCallAsync(call);
            }
        }

        private Dictionary<string, TaskCompletionSource<bool>> _pendingApprovalRequests = new Dictionary<string, TaskCompletionSource<bool>>();

        private Task<bool> RequestApprovalAsync(FunctionCallContent functionCall)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            _pendingApprovalRequests[functionCall.CallId] = taskCompletionSource;
            OnPropertyChanged("PendingApprovalRequests");
            return taskCompletionSource.Task;
        }

        public bool HasBeenApproved(string toolCallId)
        {
            return !_pendingApprovalRequests.ContainsKey(toolCallId);
        }

        public void ApproveToolCall(string toolCallId, bool approved)
        {
            if (_pendingApprovalRequests.TryGetValue(toolCallId, out var tcs))
            {
                tcs.SetResult(approved);
                _pendingApprovalRequests.Remove(toolCallId);
                OnPropertyChanged("PendingApprovalRequests");
            }
        }

        private async Task HandleToolCallAsync(FunctionCallContent functionCall)
        {
            var localTool = GetLocalTool(functionCall.Name);
            if (localTool != null)
            {
                try
                {
                    object result = await localTool.InvokeWithResultAsync(functionCall.Arguments);
                    AddToolCallResponse(functionCall.CallId, result);
                }
                catch (Exception ex)
                {
                    AddToolCallResponse(functionCall.CallId, "Error: " + ex.Message);
                }
            }
            else
            {
                AddToolCallResponse(functionCall.CallId, "Error: Unknown tool: " + functionCall.Name);
            }
        }

        private void AddToolCallResponse(string callId, object result)
        {
            AddChatMessage(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(callId, result)]));
        }

        private void AddChatMessage(ChatMessage chatMessage)
        {
            Messages.Add(chatMessage);
        }

        private async Task<ChatTurnResponse> GetResponseAsync()
        {
            var systemContextMessage = new ChatMessage(ChatRole.System, $"""
                You are Power Planner's AI assistant.
                - Power Planner is the ultimate homework planner for students.
                - The user has our local app open.
                - You have access to the user's class schedule and semester information. Use this information to provide helpful responses to the user's queries about their classes, assignments, and schedule.
                - Be concise and clear in your responses. Stay focused on the user's schedule and avoid providing unrelated information.
                - Always be polite and professional in your responses.
                - If the tools aren't available to help with the user's request, respond with a clear message indicating that you cannot assist with that request.
                - Power Planner has the following data structure... Years -> Semesters -> Classes. Then there are...
                  - Tasks: Homework / assignments, etc... things that can be marked complete and are due by a specific date. These can be under a class, or under a semester for "no class" tasks.
                  - Events: Exams / quizzes, etc... things that can't be marked complete and are events that simply happen and are done after they've passed. These can also be under a class, or under a semester for "no class" events. Class schedules should NOT be created using events.
                  - Schedules: Weekly schedule entries for a specific class.
                  - Grades: Grades under a specific class.
                  - Holidays: Holidays under a specific semester.
                - For any "how to" questions about using the app that you can't help with using the existing tools, refer the user to https://support.powerplanner.net, or ask them to email support@powerplanner.net.
                - If the user asks what you can do or what actions you can perform, mention the editing/adding tools before others, since users will primarily use this to bulk edit or add data.
                - Respond to the user in plain text (but feel free to format with bullets or use emojis!). We do not support Markdown rendering.
                """);

            var contextMessage = "Current app state:\n" + string.Join("\n", GetCurrentAppState());

            var currentContextMessage = new ChatMessage(ChatRole.System, contextMessage);

            var messages = Messages.ToList();
            messages.Insert(0, systemContextMessage);
            messages.Add(currentContextMessage);

            var request = new ChatTurnRequest
            {
                Messages = messages.ToArray(),
                Tools = LocalTools.Select(i => i.ToChatTurnTool()).ToArray()
            };

            var requestJson = JsonSerializer.Serialize(request, ChatTurnJsonContext.Default.ChatTurnRequest);
            var content = new StringContent(requestJson, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, TurnUrl);
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize(responseJson, ChatTurnJsonContext.Default.ChatTurnResponse);
            return result;
        }

        private IEnumerable<string> GetCurrentAppState()
        {
            yield return $"- CurrentDateTime: {DateTime.Now.ToString("yyyy-MM-ddTHH:mm")}";

            if (_mainScreenViewModel.CurrentSemester != null)
            {
                yield return $"- CurrentSemester: \"{_mainScreenViewModel.CurrentSemester.Name}\", Identifier: {_mainScreenViewModel.CurrentSemester.Identifier}";
            }
            else
            {
                yield return "- CurrentSemester: None";
            }

            yield return $"- CurrentPage: {_mainScreenViewModel.SelectedItem}";

            if (_mainScreenViewModel.SelectedItem == NavigationManager.MainMenuSelections.Calendar)
            {
                yield return $"- CurrentOpenMonth: {NavigationManager.GetDisplayMonth().ToString("yyyy-MM")}";
            }

            yield return $"- CurrentWeek: {_mainScreenViewModel.CurrentAccount.CurrentWeek} (a.k.a. week A/B)";

            if (_mainScreenViewModel.Classes.Count == 0)
            {
                yield return "- Current classes in the semester: None";
            }
            else
            {
                yield return $"- Current classes in the semester ({_mainScreenViewModel.Classes.Count}):";

                foreach (var c in _mainScreenViewModel.Classes)
                {
                    yield return $"  - Class: \"{c.Name}\", Identifier: {c.Identifier}, Color: {ColorHelper.ToString(c.Color)}";
                }
            }
        }

        public class ChatTurnTool
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            [JsonPropertyName("name")]
            public required string Name { get; set; }
            [JsonPropertyName("description")]
            public required string Description { get; set; }
            [JsonPropertyName("parametersSchema")]
            public required JsonElement ParametersSchema { get; set; }
            [JsonPropertyName("returnSchema")]
            public JsonElement? ReturnSchema { get; set; }
        }

        public enum ChatPageType
        {
            Calendar,
            Agenda,
            Schedule,
            Classes,
            Years,
            Settings
        }
    }

    [JsonSourceGenerationOptions(
            PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(ChatTurnRequest))]
    [JsonSerializable(typeof(ChatTurnResponse))]
    internal partial class ChatTurnJsonContext : JsonSerializerContext
    {
    }

    internal class ChatTurnRequest
    {
        [JsonPropertyName("messages")]
        public required ChatMessage[] Messages { get; set; }

        [JsonPropertyName("tools")]
        public required ChatTurnTool[] Tools { get; set; }
    }

    internal class ChatTurnResponse
    {
        [JsonPropertyName("finishReason")]
        public ChatFinishReason? FinishReason { get; set; }

        [JsonPropertyName("messages")]
        public required IList<ChatMessage> Messages { get; set; }
    }
}
