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

                // Add classes
                new AddClassesTool(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Add
                },

                // Edit classes
                new AddOrEditItemsTool<AddOrEditClassDataItem>(MainScreenViewModel)
                {
                    Mode = AddOrEditMode.Edit
                },
            ];
        }

        protected override View Render()
        {
            var renderedMessages = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
            };

            foreach (var m in _messages.Value)
            {
                if (m.Role == ChatRole.Assistant)
                {
                    foreach (var toolCall in m.Contents.OfType<FunctionCallContent>())
                    {
                        renderedMessages.Children.Add(new TextBlock
                        {
                            Text = $"Tool call: {toolCall.Name}",
                            FontSize = Theme.Current.CaptionFontSize,
                            TextColor = Theme.Current.SubtleForegroundColor,
                            Margin = new Thickness(24, 0, 0, 12)
                        });
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

            if (_isLoading.Value)
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
                        Margin = new Thickness(0, 12, 0, 0),
                        Text = new VxValue<string>(_currentInput.Value, v => _currentInput.SetValueSilently(v)),
                        OnSubmit = SendMessage,
                        AutoFocus = true,
                        IsEnabled = _isLoading.Value == false
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

                var toolResults = await HandleToolCallsAsync(toolCalls);

                var updatedMessages = _messages.Value.ToList();
                updatedMessages.AddRange(response.Messages);
                updatedMessages.Add(new ChatMessage(ChatRole.Tool, toolResults));
                _messages.Value = updatedMessages;

                // If we had updated tool results, we need to repeat the cycle
                if (toolResults.Any())
                {
                    await SendMessageAndHandleResponseAsync();
                }
            }
        }

        private async Task<List<AIContent>> HandleToolCallsAsync(FunctionCallContent[] functionCalls)
        {
            List<AIContent> answer = new List<AIContent>();

            foreach (var call in functionCalls)
            {
                try
                {
                    var result = await HandleToolCallAsync(call);
                    answer.Add(new FunctionResultContent(call.CallId, result));
                }
                catch (Exception ex)
                {
                    answer.Add(new FunctionResultContent(call.CallId, "Error: " + ex.Message));
                }
            }

            return answer;
        }

        private async Task<object> HandleToolCallAsync(FunctionCallContent functionCall)
        {
            var localTool = LocalTools.FirstOrDefault(i => i.Name == functionCall.Name);
            if (localTool != null)
            {
                return await localTool.InvokeWithResultAsync(functionCall.Arguments);
            }
            else
            {
                throw new InvalidOperationException("Unknown tool: " + functionCall.Name);
            }
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
                """);

            var contextMessage = $"""
                Current app state:
                - CurrentSemesterIdentifier: {_semester.Identifier}
                - CurrentPage: calendar
                - CurrentOpenMonth: {_displayMonth.ToString("yyyy-MM")}
                - Current classes in the semester ({_classes.Count}):
                """;

            foreach (var c in MainScreenViewModel.Classes)
            {
                contextMessage += $"\n  - Class: \"{c.Name}\", Identifier: {c.Identifier}";
            }

            var currentContextMessage = new ChatMessage(ChatRole.System, $"""
                Current app state:
                - CurrentSemesterIdentifier: {_semester.Identifier}
                - CurrentPage: calendar
                - CurrentOpenMonth: {_displayMonth.ToString("yyyy-MM")}
                """);

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
