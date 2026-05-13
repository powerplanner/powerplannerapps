using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;
using static PowerPlannerAppDataLibrary.Services.AiService;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    internal class AddClassesAndScheduleWithAiViewModel : PopupComponentViewModel
    {
        private ScheduleViewModel _scheduleViewModel;

        public AddClassesAndScheduleWithAiViewModel(ScheduleViewModel parent) : base(parent)
        {
            Title = R.S("AddClassesWithAi_Title");
            _scheduleViewModel = parent;
            AllowLightDismiss = false;
        }

        private VxState<string> _inputPrompt = new VxState<string>("");

        private VxState<bool> _isGeneratingClasses = new VxState<bool>(false);

        private VxState<string> _errorText = new VxState<string>(null);

        protected override View Render()
        {
            var content = new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = R.S("AddClassesWithAi_Description")
                                },

                                new MultilineTextBox
                                {
                                    PlaceholderText = R.S("AddClassesWithAi_PlaceholderText"),
                                    Margin = new Thickness(0, 6, 0, 0),
                                    AutoFocus = true,
                                    Text = VxValue.Create(_inputPrompt.Value, v => _inputPrompt.Value = v),
                                    IsEnabled = !_isGeneratingClasses.Value,
                                    Height = 200
                                }
                            }
                        }
                    }.LinearLayoutWeight(1),

                    _errorText.Value == null ? null : new TextBlock
                    {
                        Text = _errorText.Value,
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 0, Theme.Current.PageMargin + NookInsets.Right, 12),
                        TextColor = System.Drawing.Color.Red,
                    },
                    
                    new AccentButton
                    {
                        Text = R.S("AddClassesWithAi_ButtonCreate"),
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 0, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Click = CreateClassesAndSchedule,
                        IsEnabled = !_isGeneratingClasses.Value
                    }
                }
            };

            var overlay = _isGeneratingClasses ? new Border
            {
                BackgroundColor = Theme.Current.PopupPageBackgroundColor.Opacity(0.8),
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin).Combine(NookInsets),
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = R.S("AddClassesWithAi_GeneratingTitle"),
                            TextAlignment = HorizontalAlignment.Center,
                        },

                        new TextBlock
                        {
                            Text = R.S("AddClassesWithAi_GeneratingSubtitle"),
                            TextAlignment = HorizontalAlignment.Center,
                            TextColor = Theme.Current.SubtleForegroundColor,
                            FontSize = Theme.Current.CaptionFontSize
                        },

                        new ProgressBar
                        {
                            IsIndeterminate = true,
                            Margin = new Thickness(0, 12, 0, 0)
                        },

                        new TextButton
                        {
                            Text = PowerPlannerResources.GetStringCancel(),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 6, 0, 0),
                        }
                    }
                }
            } : null;

            return new FrameLayout
            {
                Children =
                {
                    content,
                    overlay
                }
            };
        }

        private async void CreateClassesAndSchedule()
        {
            _errorText.Value = null;

            if (string.IsNullOrWhiteSpace(_inputPrompt.Value))
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorNoText");
                return;
            }

            _isGeneratingClasses.Value = true;

            try
            {
                var service = new AiService();
                var semester = _scheduleViewModel.SemesterViewItemsGroup.Semester;
                var accountToken = _scheduleViewModel.Account?.Token;

                DateOnly? semesterStartDate = PowerPlannerSending.DateValues.IsUnassigned(semester.Start)
                    ? null
                    : DateOnly.FromDateTime(semester.Start);
                DateOnly? semesterEndDate = PowerPlannerSending.DateValues.IsUnassigned(semester.End)
                    ? null
                    : DateOnly.FromDateTime(semester.End);

                _isGeneratingClasses.Value = true;

                var response = await service.GenerateClassesAsync(
                    _inputPrompt.Value.Trim(),
                    null,
                    semesterStartDate,
                    semesterEndDate,
                    accountToken);

                SaveClasses(response);
            }
            catch (Exception ex)
            {
                if (ExceptionHelper.IsHttpWebIssue(ex))
                {
                    _errorText.Value = R.S("AddClassesWithAi_ErrorNetwork");
                    return;
                }
                else
                {
                    _errorText.Value = R.S("AddClassesWithAi_ErrorGeneric");
                    TelemetryExtension.Current.TrackEvent("AIAddClassesAndSchedule_Failed", new Dictionary<string, string> {
                        { "Exception", ExceptionHelper.GetFullDetail(ex) },
                        { "Input", _inputPrompt.Value }
                    });
                }
            }
            finally
            {
                _isGeneratingClasses.Value = false;
            }
        }

        private void TrackFailed(string reason)
        {
            TelemetryExtension.Current.TrackEvent("AIAddClassesAndSchedule_Failed", new Dictionary<string, string> {
                { "Reason", reason },
                { "Input", _inputPrompt.Value }
            });
        }

        private void SaveClasses(GenerateClassesResponse response)
        {
            if (response.Classes.Count == 0)
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorNoClasses");
                return;
            }

            if (response.Classes.Count > 15)
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorTooManyClasses");
                TrackFailed("TooManyClasses");
                return;
            }

            if (response.Classes.Any(i => i.Schedules.Count > 15))
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorTooManySchedules");
                TrackFailed("TooManySchedules");
                return;
            }

            if (response.Classes.Any(i => string.IsNullOrWhiteSpace(i.Name) || i.Name.Length > 100))
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorInvalidName");
                TrackFailed("InvalidClassName");
                return;
            }

            if (response.Classes.Any(i => i.Color == null))
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorInvalidColor");
                TrackFailed("InvalidClassColor");
                return;
            }

            if (response.Classes.Any(i => i.Schedules.Any(s => s.StartTime >= s.EndTime)))
            {
                _errorText.Value = R.S("AddClassesWithAi_ErrorInvalidTime");
                TrackFailed("InvalidScheduleTime");
                return;
            }

            TryStartDataOperationAndThenNavigate(async delegate
            {
                await ActuallySaveClassesAsync(response);
            }, delegate
            {
                RemoveViewModel();
            });
        }

        private async System.Threading.Tasks.Task ActuallySaveClassesAsync(GenerateClassesResponse response)
        {
            DataChanges changes = new DataChanges();

            foreach (var c in response.Classes)
            {
                var classChanges = AccountDataStore.GenerateNewDefaultClass(_scheduleViewModel.Account, _scheduleViewModel.SemesterViewItemsGroup.Semester.Identifier, c.Name, c.Color);
                var dataItemC = classChanges.OfType<DataItemClass>().First();

                foreach (var change in classChanges)
                {
                    changes.Add(change);
                }

                foreach (var s in c.Schedules)
                {
                    DataItemSchedule dataItemS = new DataItemSchedule()
                    {
                        Identifier = Guid.NewGuid(),
                        UpperIdentifier = dataItemC.Identifier,
                        StartTime = AsUtc(s.StartTime),
                        EndTime = AsUtc(s.EndTime),
                        Room = s.Room ?? "",
                        DayOfWeek = s.DayOfWeek,
                        ScheduleWeek = s.ScheduleWeek,
                        ScheduleType = PowerPlannerSending.Schedule.Type.Normal
                    };

                    changes.Add(dataItemS);
                }
            }

            await PowerPlannerApp.Current.SaveChanges(changes);

            TelemetryExtension.Current?.TrackEvent("AIAddClassesAndSchedule_Success", new Dictionary<string, string> {
                { "NumberOfClasses", response.Classes.Count.ToString() },
                { "NumberOfSchedules", response.Classes.Sum(c => c.Schedules.Count).ToString() },
            });
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
