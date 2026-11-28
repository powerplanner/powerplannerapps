using Microsoft.Extensions.AI;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat
{
    public partial class ChatViewModel : PopupComponentViewModel
    {
        private readonly ChatViewModelState _state;

        public ChatViewModel(MainScreenViewModel mainScreenViewModel, ChatViewModelState state) : base(mainScreenViewModel)
        {
            _state = state;
            Subscribe(_state);
            SubscribeToCollection(_state.Messages);

            // Enable platform keyboard height updates for software keyboard
            EnablePlatformKeyboardHeight = true;

            Title = R.S("AiEdit_Title");

            PrimaryCommand = new PopupCommand
            {
                Glyph = MaterialDesign.MaterialDesignIcons.Refresh,
                Text = "New chat",
                Click = _state.NewChat
            };
        }

        private const float MESSAGE_INDENT = 24;
        private const float MESSAGE_SPACING = 12;

        protected override View Render()
        {
            var renderedMessages = new LinearLayout
            {
                Margin = new Thickness(
                    Theme.Current.PageMargin + NookInsets.Left,
                    Theme.Current.PageMargin + NookInsets.Top,
                    Theme.Current.PageMargin + NookInsets.Right,
                    Theme.Current.PageMargin)
            };

            renderedMessages.Children.Add(new TextBlock
            {
                Text = "Beta feature, requires internet, uses generative AI. Actions that modify your data require user approval.",
                FontSize = 10,
                TextColor = Theme.Current.SubtleForegroundColor,
                Margin = new Thickness(0, 0, 0, MESSAGE_SPACING)
            });

            bool needsToolCallApproval = false;

            foreach (var m in _state.Messages)
            {
                if (m.Role == ChatRole.Assistant)
                {
                    foreach (var toolCall in m.Contents.OfType<FunctionCallContent>())
                    {
                        var localTool = _state.GetLocalTool(toolCall.Name);
                        if (localTool != null)
                        {
                            if (localTool.RequiresApproval && !_state.HasBeenApproved(toolCall.CallId))
                            {
                                var renderedApproval = RenderApproval(toolCall, localTool);
                                renderedApproval.Margin = new Thickness(MESSAGE_INDENT, 0, 0, MESSAGE_SPACING);
                                renderedMessages.Children.Add(renderedApproval);

                                needsToolCallApproval = true;
                            }
                            else
                            {
                                renderedMessages.Children.Add(new TextBlock
                                {
                                    Text = localTool.DisplayName ?? localTool.Name,
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    Margin = new Thickness(MESSAGE_INDENT + 6, 0, 0, MESSAGE_SPACING),
                                    IsTextSelectionEnabled = true
                                });
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(m.Text))
                    {
                        renderedMessages.Children.Add(RenderMessageFromAgent(m.Text));
                    }
                }
                else if (m.Role == ChatRole.User)
                {
                    renderedMessages.Children.Add(new Border
                    {
                        BackgroundColor = Theme.Current.BackgroundAlt1Color,
                        CornerRadius = 6,
                        Padding = new Thickness(6),
                        Content = new HyperlinkTextBlock
                        {
                            Text = m.Text,
                            WrapText = true,
                            TextColor = Theme.Current.ForegroundColor,
                            IsTextSelectionEnabled = true
                        },
                        Margin = new Thickness(MESSAGE_INDENT, 0, 0, MESSAGE_SPACING)
                    });
                }
            }

            if (_state.IsLoading && !needsToolCallApproval)
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
                    Margin = new Thickness(0, 0, MESSAGE_INDENT, MESSAGE_SPACING)
                });
            }

            if (!_state.IsLoading && !needsToolCallApproval && _state.Messages.Count <= 1)
            {
                renderedMessages.Children.Add(new TextButton
                {
                    Text = "⚡ Tell me what you can do",
                    Margin = new Thickness(MESSAGE_INDENT, 0, 0, MESSAGE_SPACING),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Click = () =>
                    {
                        _state.SetCurrentInputSilently("What actions can you help me with in Power Planner?");
                        _state.SendMessage();
                    }
                });
            }

            bool canSend = _state.HasInput && !_state.IsLoading && !needsToolCallApproval;
            bool canCancel = _state.IsLoading || needsToolCallApproval;

            var sendButton = new Border
            {
                BackgroundColor = canCancel ? Theme.Current.BackgroundAlt1Color : canSend ? Theme.Current.AccentColor : Theme.Current.AccentColor.Opacity(0.8),
                CornerRadius = 6,
                Margin = new Thickness(12, 0, 0, 0),
                Content = new FontIcon
                {
                    Glyph = canCancel ? MaterialDesign.MaterialDesignIcons.Cancel : MaterialDesign.MaterialDesignIcons.Send,
                    Color = canCancel ? Theme.Current.ForegroundColor : canSend ? System.Drawing.Color.White : System.Drawing.Color.White.Opacity(0.8),
                    FontSize = 18,
                    Margin = new Thickness(6)
                },
                Tapped = () =>
                {
                    if (canCancel)
                    {
                        _state.Cancel();
                    }
                    else
                    {
                        _state.SendMessage();
                    }
                }
            };

            return new LinearLayout
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundColor,
                Children =
                {
                    new ScrollView
                    {
                        Content = renderedMessages,
                        ScrollToBottomWhenContentHeightChanges = true
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 12, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + Math.Max(NookInsets.Bottom, PlatformKeyboardHeight)),
                        Children =
                        {
                            new MultilineTextBox
                            {
                                PlaceholderText = "Type your message...",
                                Text = new VxValue<string>(_state.CurrentInput, v => _state.SetCurrentInputSilently(v)),
                                OnSubmit = _state.SendMessage,
                                AutoFocus = true,
                                IsEnabled = _state.IsLoading == false
                            }.LinearLayoutWeight(1),

                            sendButton
                        }
                    }
                }
            };
        }

        private View RenderMessageFromAgent(string text)
        {
            return new Border
            {
                BackgroundColor = Theme.Current.AccentColor,
                CornerRadius = 6,
                Padding = new Thickness(6),
                Content = new TextBlock
                {
                    Text = text,
                    TextColor = System.Drawing.Color.White,
                    IsTextSelectionEnabled = true
                },
                Margin = new Thickness(0, 0, MESSAGE_INDENT, 12)
            };
        }

        [JsonSourceGenerationOptions(
            WriteIndented = true)]
        [JsonSerializable(typeof(JsonElement))]
        internal partial class JsonArgumentsContext : JsonSerializerContext
        {
        }

        private string PrettifyArguments(JsonElement arguments)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    TypeInfoResolver = JsonArgumentsContext.Default
                };
                string formattedJson = JsonSerializer.Serialize(arguments, jsonTypeInfo: JsonArgumentsContext.Default.JsonElement);
                return formattedJson;
            }
            catch
            {
                return arguments.ToString();
            }
        }

        private View RenderApproval(FunctionCallContent toolCall, LocalTool localTool)
        {
            string toolArguments;
            try
            {
                toolArguments = PrettifyArguments(LocalTool.ConvertToolCallArgumentsToJsonElement(toolCall.Arguments));
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
                                localTool.IsDeleteItems ? new DestructiveButton
                                {
                                    Text = "Delete items",
                                    Margin = new Thickness(0, 0, 3, 0),
                                    Click = () => _state.ApproveToolCall(toolCall.CallId, true)
                                } : new AccentButton
                                {
                                    Text = "Approve",
                                    Margin = new Thickness(0, 0, 3, 0),
                                    Click = () => _state.ApproveToolCall(toolCall.CallId, true)
                                },

                                new Button
                                {
                                    Text = "Deny",
                                    Margin = new Thickness(3, 0, 0, 0),
                                    Click = () => _state.ApproveToolCall(toolCall.CallId, false)
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
