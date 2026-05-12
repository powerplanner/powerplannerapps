using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using static PowerPlannerAppDataLibrary.Services.AiService;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    public class ScheduleWelcomeComponent : VxComponent
    {
        public ScheduleViewModel ScheduleViewModel { get; set; }
        public Thickness NookInsets { get; set; }
        private VxState<string> _inputPrompt = new VxState<string>("");

        private VxState<bool> _displayInScrollView = new VxState<bool>(false);

        private VxState<GenerateClassesResponse> _generateClassesResponse = new VxState<GenerateClassesResponse>(null);

        private VxState<bool> _isGeneratingClasses = new VxState<bool>(false);

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            _displayInScrollView.Value = size.Height <= 500;

            base.OnSizeChanged(size, previousSize);
        }

        protected override View Render()
        {
            if (_generateClassesResponse.Value != null)
            {
                return new PreviewNewScheduleComponent()
                {
                    GenerateClassesResponse = _generateClassesResponse.Value,
                    NookInsets = NookInsets
                };
            }

            const int MAX_WIDTH = 700;

            var content = new LinearLayout
            {
                Margin = new Thickness(Theme.Current.PageMargin)
                                .Combine(NookInsets)
                                .Combine(new Thickness(0, 60, 0, 60)),
                Children =
                {
                    new TextBlock
                    {
                        Text = R.S("SchedulePage_TextBlockWelcomeTitle.Text"),
                        TextAlignment = HorizontalAlignment.Center,
                        MaxWidth = MAX_WIDTH,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = Theme.Current.TitleFontSize
                    },

                    new TextBlock
                    {
                        Text = R.S("SchedulePage_TextBlockWelcomeSubtitle.Text"),
                        TextAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 0),
                        TextColor = Theme.Current.SubtleForegroundColor,
                        MaxWidth = MAX_WIDTH,
                    },

                    new MultilineTextBox
                    {
                        PlaceholderText = "Paste your class schedule in any format, ex:\n\nANTH 160D2 - Mon, Wed 10-10:50am in ILC 130, and Fri 12-12:50pm in Modern Languages 402\nCSC 337 - Tue, Thu 9:30-10:45am in ILC 150",
                        Height = 120,
                        MaxWidth = MAX_WIDTH,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 12, 0, 0),
                        AutoFocus = true,
                        Text = VxValue.Create(_inputPrompt.Value, v => _inputPrompt.Value = v),
                        IsEnabled = !_isGeneratingClasses.Value
                    },

                    new AccentButton
                    {
                        Text = _isGeneratingClasses.Value ? "✨ Creating classes and schedule..." : "✨ Create classes and schedule",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Click = GenerateSchedule,
                        Margin = new Thickness(0, 12, 0, 0),
                        IsEnabled = !_isGeneratingClasses.Value
                    },

                    new TextBlock
                    {
                        Text = "Or, add classes manually...",
                        TextAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 0),
                        TextColor = Theme.Current.SubtleForegroundColor
                    },

                    new AccentButton
                    {
                        Text = "+ " + R.S("SchedulePage_ButtonAddClass.Content"),
                        Click = ScheduleViewModel.AddClass,
                        MaxWidth = 180,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 0),
                        IsEnabled = !_isGeneratingClasses.Value
                    }
                }
            };

            View contentContainer;

            if (!_displayInScrollView.Value)
            {
                content.VerticalAlignment = VerticalAlignment.Center;
                contentContainer = content;
            }
            else
            {
                contentContainer = new ScrollView
                {
                    Content = content
                };
            }

            return new FrameLayout
            {
                BackgroundColor = Theme.Current.BackgroundAlt2Color,
                Children =
                {
                    contentContainer,

                    ScheduleViewModel.IsReturningUserVisible ? new LinearLayout
                    {
                        Margin = new Thickness(Theme.Current.PageMargin, 0, Theme.Current.PageMargin, 0).Combine(NookInsets),
                        BackgroundColor = Theme.Current.BackgroundAlt2Color,
                        VerticalAlignment = VerticalAlignment.Top,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = R.S("SchedulePage_TextBlockReturningUser.Text"),
                                TextAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0, Theme.Current.PageMargin + NookInsets.Top - 8, 0, 0)
                            },

                            new AccentButton
                            {
                                Text = R.S("WelcomePage_ButtonLogin.Content"),
                                Margin = new Thickness(0, 6, 0, 12),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Click = ScheduleViewModel.LogIn,
                                Width = 120
                            }
                        }
                    } : null

                }
            };
        }

        private async void GenerateSchedule()
        {
            var userInput = _inputPrompt.Value;
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return;
            }

            try
            {
                TelemetryExtension.Current?.TrackEvent("AISchedule_GenerateClasses");

                var service = new AiService();
                var semester = ScheduleViewModel.SemesterViewItemsGroup.Semester;
                var accountToken = ScheduleViewModel.Account?.Token;

                DateOnly? semesterStartDate = PowerPlannerSending.DateValues.IsUnassigned(semester.Start)
                    ? null
                    : DateOnly.FromDateTime(semester.Start);
                DateOnly? semesterEndDate = PowerPlannerSending.DateValues.IsUnassigned(semester.End)
                    ? null
                    : DateOnly.FromDateTime(semester.End);

                _isGeneratingClasses.Value = true;

                var response = await service.GenerateClassesAsync(
                    userInput,
                    semesterStartDate,
                    semesterEndDate,
                    accountToken);

                SaveClassesAsync(response);
                //_generateClassesResponse.Value = response;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                new PortableMessageDialog("Sorry, something went wrong while generating your schedule. Make sure you're connected to the internet and please try again.", "Error").Show();
            }
            finally
            {
                _isGeneratingClasses.Value = false;
            }
        }

        private void SaveClassesAsync(GenerateClassesResponse response)
        {
            if (response.Classes.Count > 15)
            {
                new PortableMessageDialog("There were over 15 classes that were trying to be created. This is probaly an error.", "Too many classes").Show();
                return;
            }

            if (response.Classes.Any(i => i.Schedules.Count > 15))
            {
                new PortableMessageDialog("There were over 15 schedules that were trying to be created for a single class. This is probaly an error.", "Too many schedules").Show();
                return;
            }

            if (response.Classes.Any(i => string.IsNullOrWhiteSpace(i.Name)))
            {
                new PortableMessageDialog("One or more classes had an invalid name.", "Invalid class name").Show();
                return;
            }

            if (response.Classes.Any(i => i.Color == null))
            {
                new PortableMessageDialog("One or more classes had an invalid color.", "Invalid class color").Show();
                return;
            }

            if (response.Classes.Any(i => i.Schedules.Any(s => s.StartTime >= s.EndTime)))
            {
                new PortableMessageDialog("One or more schedules had an invalid start time and end time.", "Invalid schedule time").Show();
                return;
            }

            BaseMainScreenViewModelDescendant.TryStartDataOperationAndThenNavigate(async delegate
            {
                await ActuallySaveClassesAsync(response);
            }, delegate
            {

            });
        }

        private async Task ActuallySaveClassesAsync(GenerateClassesResponse response)
        {
            DataChanges changes = new DataChanges();

            foreach (var c in response.Classes)
            {
                var classChanges = AccountDataStore.GenerateNewDefaultClass(ScheduleViewModel.Account, ScheduleViewModel.SemesterViewItemsGroup.Semester.Identifier, c.Name, c.Color);
                var dataItemC = classChanges.OfType<DataItemClass>().First();
                if (c.Details != null)
                {
                    dataItemC.Details = c.Details;
                }
                if (c.StartDate != null)
                {
                    dataItemC.StartDate = AsUtc(c.StartDate.Value.ToDateTime(TimeOnly.MinValue));
                    if (!SqlDate.IsValid(dataItemC.StartDate))
                    {
                        dataItemC.StartDate = SqlDate.MinValue;
                    }
                }
                if (c.EndDate != null)
                {
                    dataItemC.EndDate = AsUtc(c.EndDate.Value.ToDateTime(TimeOnly.MinValue));
                    if (!SqlDate.IsValid(dataItemC.EndDate))
                    {
                        dataItemC.EndDate = SqlDate.MinValue;
                    }
                }

                foreach (var change in classChanges)
                {
                    changes.Add(change);
                }

                foreach (var s in c.Schedules)
                {
                    foreach (var day in s.DayOfWeeks)
                    {
                        DataItemSchedule dataItemS = new DataItemSchedule()
                        {
                            Identifier = Guid.NewGuid(),
                            UpperIdentifier = dataItemC.Identifier,
                            StartTime = AsUtc(s.StartTime),
                            EndTime = AsUtc(s.EndTime),
                            Room = s.Room ?? "",
                            DayOfWeek = day,
                            ScheduleWeek = s.ScheduleWeek,
                            ScheduleType = PowerPlannerSending.Schedule.Type.Normal
                        };

                        changes.Add(dataItemS);
                    }
                }
            }

            await PowerPlannerApp.Current.SaveChanges(changes);
        }

        private DateTime AsUtc(TimeSpan time)
        {
            return AsUtc(DateTime.Today.Add(time));
        }

        private DateTime AsUtc(DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }
}
