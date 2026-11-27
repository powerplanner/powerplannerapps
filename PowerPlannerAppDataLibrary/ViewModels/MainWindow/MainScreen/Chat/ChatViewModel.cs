using BareMvvm.Core.ViewModels;
using Microsoft.Extensions.AI;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Services;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat
{
    public partial class ChatViewModel : PopupComponentViewModel
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string TurnUrl = "http://localhost:7074/api/v1/ai/chat/turn";
        private readonly MyObservableList<ViewItemClass> _classes;
        private readonly ViewItemSemester _semester;
        private readonly SemesterItemsViewGroup _semesterItems;
        private readonly DateOnly _displayMonth;

        private VxState<List<ChatMessage>> _messages = new([]);
        private VxState<string> _currentInput = new("");
        private VxState<bool> _isLoading = new(false);

        public ChatViewModel(
            BaseViewModel parent,
            MyObservableList<ViewItemClass> classes,
            ViewItemSemester semester,
            SemesterItemsViewGroup semesterItems,
            DateOnly displayMonth) : base(parent)
        {
            _classes = classes;
            _semester = semester;
            _semesterItems = semesterItems;
            _displayMonth = displayMonth;

            Title = "AI Planner";
            UseCancelForBack();

            LocalTools =
            [
                new NavigateToPageTool(MainScreenViewModel),
                new GetClassesTool(MainScreenViewModel),
                new GetTasksAndEventsTool(MainScreenViewModel),
                new GetHolidaysTool(MainScreenViewModel),

                // Dislay items
                new DisplayItemTool(MainScreenViewModel)
                {
                    ItemType = PowerPlannerSending.ItemType.Class,
                    ItemName = "class",
                    DisplayName = "Open class"
                },

                new DisplayItemTool(MainScreenViewModel)
                {
                    ItemType = PowerPlannerSending.ItemType.MegaItem,
                    ItemName = "task_or_event",
                    DisplayName = "Open task or event"
                },

                new DisplayItemTool(MainScreenViewModel)
                {
                    ItemType = PowerPlannerSending.ItemType.MegaItem,
                    ItemName = "holiday",
                    DisplayName = "Open holiday"
                },

                // Add classes
                new AddClassesTool(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add classes"
                },

                // Edit classes
                new AddOrEditItemsTool<AddOrEditClassDataItem>(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit classes"
                },

                // Add tasks
                new AddOrEditItemsTool<AddOrEditTaskDataItem>(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add tasks"
                },

                // Edit tasks
                new AddOrEditItemsTool<AddOrEditTaskDataItem>(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit tasks"
                },

                // Add events
                new AddOrEditItemsTool<AddOrEditEventDataItem>(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add,
                    DisplayName = "Add events"
                },

                // Edit events
                new AddOrEditItemsTool<AddOrEditEventDataItem>(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit,
                    DisplayName = "Edit events"
                }
            ];
        }

        private LocalTool GetLocalTool(string toolName)
        {
            return LocalTools.FirstOrDefault(i => i.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        }

        private object GetToolResult(FunctionCallContent functionCall)
        {
            return _messages.Value.SelectMany(m => m.Contents).OfType<FunctionResultContent>().FirstOrDefault(i => i.CallId == functionCall.CallId);
        }

        private FunctionCallContent GetToolCall(string toolCallId)
        {
            return _messages.Value.SelectMany(m => m.Contents).OfType<FunctionCallContent>().FirstOrDefault(i => i.CallId == toolCallId);
        }

        protected override View Render()
        {
            var renderedMessages = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
            };

            bool needsToolCallApproval = false;

            foreach (var m in _messages.Value)
            {
                if (m.Role == ChatRole.Assistant)
                {
                    foreach (var toolCall in m.Contents.OfType<FunctionCallContent>())
                    {
                        var localTool = GetLocalTool(toolCall.Name);
                        if (localTool != null)
                        {
                            if (localTool.RequiresApproval && !HasBeenApproved(toolCall.CallId))
                            {
                                var renderedApproval = RenderApproval(toolCall, localTool);
                                renderedApproval.Margin = new Thickness(24, 0, 0, 12);
                                renderedMessages.Children.Add(renderedApproval);

                                needsToolCallApproval = true;
                            }
                            else
                            {
                                renderedMessages.Children.Add(new TextBlock
                                {
                                    Text = $"Action: {localTool.DisplayName ?? localTool.Name}",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    Margin = new Thickness(24, 0, 0, 12)
                                });
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(m.Text))
                    {
                        renderedMessages.Children.Add(new Border
                        {
                            BackgroundColor = Theme.Current.AccentColor,
                            CornerRadius = 6,
                            Padding = new Thickness(6),
                            Content = new TextBlock
                            {
                                Text = m.Text,
                                WrapText = true,
                                TextColor = System.Drawing.Color.White
                            },
                            Margin = new Thickness(0, 0, 24, 12)
                        });
                    }
                }
                else if (m.Role == ChatRole.User)
                {
                    renderedMessages.Children.Add(new Border
                    {
                        BackgroundColor = Theme.Current.BackgroundAlt1Color,
                        CornerRadius = 6,
                        Padding = new Thickness(6),
                        Content = new TextBlock
                        {
                            Text = m.Text,
                            WrapText = true,
                            TextColor = Theme.Current.ForegroundColor
                        },
                        Margin = new Thickness(24, 0, 0, 12)
                    });
                }
            }

            if (_isLoading.Value && !needsToolCallApproval)
            {
                renderedMessages.Children.Add(new Border
                {
                    BackgroundColor = Theme.Current.AccentColor,
                    CornerRadius = 6,
                    Padding = new Thickness(6),
                    Content = new ProgressBar
                    {
                        IsIndeterminate = true,
                        Color = System.Drawing.Color.White
                    },
                    Margin = new Thickness(0, 0, 24, 12)
                });
            }

            return new LinearLayout
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundColor,
                Children =
                {
                    new ScrollView
                    {
                        Content = renderedMessages
                    },

                    new TextBox
                    {
                        PlaceholderText = "Type your message...",
                        Margin = new Thickness(Theme.Current.PageMargin, 12, Theme.Current.PageMargin, Theme.Current.PageMargin),
                        Text = new VxValue<string>(_currentInput.Value, v => _currentInput.SetValueSilently(v)),
                        OnSubmit = SendMessage,
                        AutoFocus = true,
                        IsEnabled = _isLoading.Value == false,
                    }
                }
            };
        }

        private View RenderApproval(FunctionCallContent toolCall, LocalTool localTool)
        {
            string toolArguments;
            try
            {
                toolArguments = LocalTool.ConvertToolCallArgumentsToJsonElement(toolCall.Arguments).ToString();
            }
            catch
            {
                toolArguments = "Failed to display arguments";
            }

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundAlt1Color,
                CornerRadius = 6,
                Padding = new Thickness(6),
                Content = new LinearLayout
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Approval needed: {localTool.DisplayName ?? localTool.Name}",
                            Margin = new Thickness(0, 0, 0, 6),
                        },
                        new ScrollView
                        {
                            Height = 75,
                            Content = new Border
                            {
                                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                                CornerRadius = 4,
                                Padding = new Thickness(6),
                                Content = new TextBlock
                                {
                                    Text = toolArguments,
                                    IsTextSelectionEnabled = true,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    FontSize = Theme.Current.CaptionFontSize
                                }
                            },
                            Margin = new Thickness(0, 0, 0, 6)
                        },
                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new AccentButton
                                {
                                    Text = "Approve",
                                    Margin = new Thickness(0, 0, 3, 0),
                                    Click = () => ApproveToolCall(toolCall.CallId, true)
                                },

                                new Button
                                {
                                    Text = "Deny",
                                    Margin = new Thickness(3, 0, 0, 0),
                                    Click = () => ApproveToolCall(toolCall.CallId, false)
                                }
                            }
                        }
                    }
                }
            };
        }

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_currentInput.Value))
            {
                new PortableMessageDialog("Please enter a message before sending.", "Error").Show();
                return;
            }

            var newMessages = _messages.Value.ToList();
            newMessages.Add(new ChatMessage(ChatRole.User, _currentInput.Value));
            _messages.Value = newMessages;
            _currentInput.Value = string.Empty;
            _isLoading.Value = true;

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
                _isLoading.Value = false;
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

                _messages.Value.AddRange(response.Messages);
                MarkDirty();

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
            MarkDirty();
            return taskCompletionSource.Task;
        }

        private bool HasBeenApproved(string toolCallId)
        {
            return !_pendingApprovalRequests.ContainsKey(toolCallId);
        }

        private void ApproveToolCall(string toolCallId, bool approved)
        {
            if (_pendingApprovalRequests.TryGetValue(toolCallId, out var tcs))
            {
                tcs.SetResult(approved);
                _pendingApprovalRequests.Remove(toolCallId);
                MarkDirty();
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
            _messages.Value.Add(chatMessage);
            MarkDirty();
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
                """);

            var contextMessage = $"""
                Current app state:
                - CurrentSemesterIdentifier: {_semester.Identifier}
                - CurrentPage: calendar
                - CurrentOpenMonth: {_displayMonth.ToString("yyyy-MM")}
                - CurrentDateTime: {DateTime.Now.ToString("yyyy-MM-ddTHH:mm")}
                - Current classes in the semester ({_classes.Count}):
                """;

            foreach (var c in MainScreenViewModel.Classes)
            {
                contextMessage += $"\n  - Class: \"{c.Name}\", Identifier: {c.Identifier}";
            }

            var currentContextMessage = new ChatMessage(ChatRole.System, contextMessage);

            var messages = _messages.Value.ToList();
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

        [JsonSourceGenerationOptions(
            PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonSerializable(typeof(ChatTurnRequest))]
        [JsonSerializable(typeof(ChatTurnResponse))]
        internal partial class ChatTurnJsonContext : JsonSerializerContext
        {
        }

        public class ChatTurnRequest
        {
            [JsonPropertyName("messages")]
            public required ChatMessage[] Messages { get; set; }

            [JsonPropertyName("tools")]
            public required ChatTurnTool[] Tools { get; set; }
        }

        public class ChatTurnResponse
        {
            [JsonPropertyName("finishReason")]
            public ChatFinishReason? FinishReason { get; set; }

            [JsonPropertyName("messages")]
            public required IList<ChatMessage> Messages { get; set; }
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
}
