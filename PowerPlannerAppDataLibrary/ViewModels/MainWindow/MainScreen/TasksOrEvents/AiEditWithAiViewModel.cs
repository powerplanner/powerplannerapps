using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Services;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Task = System.Threading.Tasks.Task;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents
{
    public class AiEditWithAiViewModel : PopupComponentViewModel
    {
        public enum ViewState { Input, Preview }

        private ViewState _state = ViewState.Input;
        public ViewState State
        {
            get => _state;
            set => SetProperty(ref _state, value, nameof(State));
        }

        private string _userInput = "";
        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value, nameof(UserInput));
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value, nameof(IsLoading));
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value, nameof(ErrorMessage));
        }

        private string _descriptionOfChanges;
        public string DescriptionOfChanges
        {
            get => _descriptionOfChanges;
            set => SetProperty(ref _descriptionOfChanges, value, nameof(DescriptionOfChanges));
        }

        public List<AiProposedChange> ProposedChanges { get; set; }

        private readonly MyObservableList<ViewItemClass> _classes;
        private readonly ViewItemSemester _semester;
        private readonly SemesterItemsViewGroup _semesterItems;
        private readonly DateOnly _displayMonth;

        public AiEditWithAiViewModel(
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

            Title = R.S("AiEdit_Title");
            UseCancelForBack();
        }

        public async Task PreviewResults()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
            {
                ErrorMessage = R.S("AiEdit_ErrorEmpty");
                return;
            }
            
            TelemetryExtension.Current?.TrackEvent("AIEdit_PreviewResults");

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var service = new AiService();
                var accountToken = MainScreenViewModel.CurrentAccount?.Token;
                var changes = await service.GenerateChangesAsync(
                    UserInput,
                    _classes,
                    _semester,
                    _semesterItems,
                    _displayMonth,
                    accountToken);

                ProposedChanges = changes;
                DescriptionOfChanges = service.DescriptionOfChanges;
                State = ViewState.Preview;
            }
            catch (Exception ex)
            {
                ErrorMessage = R.S("AiEdit_ErrorGeneric");
                TelemetryExtension.Current?.TrackException(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SaveChanges()
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("AIEdit_SaveChanges", new Dictionary<string, string>
                {
                    { "Changes", ProposedChanges.Count.ToString() }
                });

                var changes = new DataChanges();

                foreach (var change in ProposedChanges.Where(c => c.IsSelected))
                {
                    switch (change.Operation)
                    {
                        case AiChangeOperation.Add:
                            AddItem(changes, change);
                            break;

                        case AiChangeOperation.Edit:
                            EditItem(changes, change);
                            break;

                        case AiChangeOperation.Delete:
                            if (change.ExistingItemId.HasValue)
                            {
                                changes.DeleteItem(change.ExistingItemId.Value);
                            }
                            break;
                    }
                }

                if (!changes.IsEmpty())
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);
                }

                this.RemoveViewModel();
            }
            catch (Exception ex)
            {
                ErrorMessage = R.S("AiEdit_ErrorSaveFailed");
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void AddItem(DataChanges changes, AiProposedChange change)
        {
            var dataItem = new DataItemMegaItem
            {
                Identifier = Guid.NewGuid()
            };

            ApplyChangeToDataItem(dataItem, change, isNew: true);
            changes.Add(dataItem);
        }

        private void EditItem(DataChanges changes, AiProposedChange change)
        {
            if (!change.ExistingItemId.HasValue)
                return;

            var dataItem = new DataItemMegaItem
            {
                Identifier = change.ExistingItemId.Value
            };

            // Find the existing item to determine its current MegaItemType
            var existingItem = _semesterItems.Items.FirstOrDefault(i => i.Identifier == change.ExistingItemId.Value);
            if (existingItem is ViewItemTaskOrEvent taskOrEvent)
            {
                dataItem.MegaItemType = taskOrEvent.Type == TaskOrEventType.Task
                    ? (taskOrEvent.Class.IsNoClassClass ? PowerPlannerSending.MegaItemType.Task : PowerPlannerSending.MegaItemType.Homework)
                    : (taskOrEvent.Class.IsNoClassClass ? PowerPlannerSending.MegaItemType.Event : PowerPlannerSending.MegaItemType.Exam);
            }
            else
            {
                dataItem.MegaItemType = MegaItemType.Holiday;
            }

            ApplyChangeToDataItem(dataItem, change, isNew: false);
            changes.Add(dataItem);
        }

        private void ApplyChangeToDataItem(DataItemMegaItem dataItem, AiProposedChange change, bool isNew)
        {
            // Determine type and upper identifier
            var itemType = change.Type ?? AiItemType.Task;
            bool hasClass = change.ClassId.HasValue && change.ClassId.Value != _semester.Identifier;

            if (isNew)
            {
                switch (itemType)
                {
                    case AiItemType.Task:
                        dataItem.MegaItemType = hasClass ? PowerPlannerSending.MegaItemType.Homework : PowerPlannerSending.MegaItemType.Task;
                        break;
                    case AiItemType.Event:
                        dataItem.MegaItemType = hasClass ? PowerPlannerSending.MegaItemType.Exam : PowerPlannerSending.MegaItemType.Event;
                        break;
                    case AiItemType.Holiday:
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Holiday;
                        break;
                }

                if (hasClass)
                {
                    dataItem.UpperIdentifier = change.ClassId.Value;
                }
                else
                {
                    dataItem.UpperIdentifier = _semester.Identifier;
                }

                if (!hasClass)
                {
                    dataItem.WeightCategoryIdentifier = Guid.Empty;
                }
            }
            else if (change.ClassId.HasValue)
            {
                // Editing: class is changing
                if (hasClass)
                {
                    dataItem.UpperIdentifier = change.ClassId.Value;

                    // Update MegaItemType to match class vs no-class distinction
                    var effectiveType = GetEffectiveType(dataItem.MegaItemType);
                    if (effectiveType == AiItemType.Task)
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Homework;
                    else if (effectiveType == AiItemType.Event)
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Exam;
                }
                else
                {
                    // Moving to No Class
                    dataItem.UpperIdentifier = _semester.Identifier;
                    dataItem.WeightCategoryIdentifier = Guid.Empty;

                    var effectiveType = GetEffectiveType(dataItem.MegaItemType);
                    if (effectiveType == AiItemType.Task)
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Task;
                    else if (effectiveType == AiItemType.Event)
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Event;
                }
            }

            if (change.Name != null)
            {
                dataItem.Name = change.Name;
            }

            if (change.Details != null)
            {
                dataItem.Details = change.Details;
            }

            if (change.PercentComplete.HasValue)
            {
                dataItem.PercentComplete = change.PercentComplete.Value;
            }

            // Handle dates and times
            if (change.Date.HasValue)
            {
                var date = change.Date.Value.ToDateTime(TimeOnly.MinValue);
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

                if (dataItem.MegaItemType == MegaItemType.Holiday)
                {
                    dataItem.Date = date;
                }
                else
                {
                    var effectiveType = isNew ? itemType : GetEffectiveType(dataItem.MegaItemType);
                    ApplyTimeEncoding(dataItem, date, change.Time, change.EndTime, effectiveType);
                }
            }

            if (dataItem.MegaItemType == MegaItemType.Holiday)
            {
                if (change.EndDate.HasValue)
                {
                    var endDate = change.EndDate.Value.ToDateTime(TimeOnly.MinValue);
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                    dataItem.EndTime = endDate.AddDays(1).AddSeconds(-1);
                }
                else if (isNew)
                {
                    dataItem.EndTime = dataItem.Date;
                }
            }
        }

        private static AiItemType GetEffectiveType(PowerPlannerSending.MegaItemType megaItemType)
        {
            switch (megaItemType)
            {
                case PowerPlannerSending.MegaItemType.Homework:
                case PowerPlannerSending.MegaItemType.Task:
                    return AiItemType.Task;
                case PowerPlannerSending.MegaItemType.Exam:
                case PowerPlannerSending.MegaItemType.Event:
                    return AiItemType.Event;
                case PowerPlannerSending.MegaItemType.Holiday:
                    return AiItemType.Holiday;
                default:
                    return AiItemType.Task;
            }
        }

        private static void ApplyTimeEncoding(DataItemMegaItem dataItem, DateTime date, string time, string endTime, AiItemType itemType)
        {
            var timeOption = time?.ToLowerInvariant() ?? "allday";

            if (itemType == AiItemType.Task)
            {
                switch (timeOption)
                {
                    case "beforeclass":
                        dataItem.Date = date.AddSeconds(1);
                        break;
                    case "startofclass":
                        dataItem.Date = date;
                        break;
                    case "duringclass":
                        dataItem.Date = date.AddSeconds(2);
                        break;
                    case "endofclass":
                        dataItem.Date = date.AddSeconds(3);
                        break;
                    case "allday":
                        dataItem.Date = date.AddDays(1).AddSeconds(-1);
                        break;
                    default:
                        // Custom time "HH:mm"
                        if (TimeSpan.TryParse(timeOption, out var customTime))
                        {
                            dataItem.Date = date.Add(customTime).AddSeconds(4);
                        }
                        else
                        {
                            dataItem.Date = date.AddDays(1).AddSeconds(-1);
                        }
                        break;
                }
            }
            else if (itemType == AiItemType.Event)
            {
                switch (timeOption)
                {
                    case "duringclass":
                        dataItem.Date = date;
                        dataItem.EndTime = DateValues.UNASSIGNED;
                        break;
                    case "allday":
                        dataItem.Date = date;
                        dataItem.EndTime = date.AddDays(1).AddSeconds(-1);
                        break;
                    default:
                        // Custom time
                        if (TimeSpan.TryParse(timeOption, out var startTime))
                        {
                            dataItem.Date = date.Add(startTime);
                            if (endTime != null && TimeSpan.TryParse(endTime, out var parsedEndTime))
                            {
                                dataItem.EndTime = date.Add(parsedEndTime);
                            }
                            else
                            {
                                dataItem.EndTime = date.Add(startTime).AddHours(1);
                            }
                        }
                        else
                        {
                            dataItem.Date = date;
                            dataItem.EndTime = date.AddDays(1).AddSeconds(-1);
                        }
                        break;
                }
            }
        }

        public new void GoBack()
        {
            State = ViewState.Input;
        }

        protected override View Render()
        {
            if (State == ViewState.Input)
            {
                return RenderInputState();
            }
            else
            {
                return RenderPreviewState();
            }
        }

        private string MoveOverdueToTodayPrompt => R.S("AiEdit_SuggestedPrompt_MoveOverdue");

        private View RenderInputState()
        {
            var suggestedPrompt = MoveOverdueToTodayPrompt;
            var views = new List<View>
            {
                new MultilineTextBox
                {
                    Text = VxValue.Create(UserInput, v => UserInput = v),
                    PlaceholderText = R.S("AiEdit_Placeholder"),
                    AutoFocus = true,
                    Header = R.S("AiEdit_Header")
                },

                UserInput == suggestedPrompt ? null : new Border
                {
                    BackgroundColor = Theme.Current.PopupPageBackgroundAltColor,
                    CornerRadius = 6,
                    Padding = new Thickness(2),
                    Content = new TextBlock
                    {
                        Text = "✨ " + suggestedPrompt,
                        FontSize = Theme.Current.CaptionFontSize
                    },
                    Tapped = () =>
                    {
                        UserInput = suggestedPrompt;
                        _ = PreviewResults();
                    }
                },

                new TextBlock
                {
                    Text = R.S("AiEdit_Description"),
                    FontSize = Theme.Current.CaptionFontSize,
                    TextColor = Theme.Current.SubtleForegroundColor,
                    Margin = new Thickness(0, 6, 0, 0)
                },
            };

            if (ErrorMessage != null)
            {
                views.Add(new TextBlock
                {
                    Text = ErrorMessage,
                    TextColor = Color.Red,
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }

            views.Add(new AccentButton
            {
                Text = R.S("AiEdit_PreviewResults"),
                Click = () => _ = PreviewResults(),
                IsEnabled = !IsLoading,
                Margin = new Thickness(0, 12, 0, 0)
            });

            if (IsLoading)
            {
                views.Add(new ProgressBar
                {
                    IsIndeterminate = true,
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }

            return RenderGenericPopupContent(views);
        }

        private View RenderPreviewState()
        {
            var views = new LinearLayout()
            {
                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin)
            };

            if (!string.IsNullOrEmpty(DescriptionOfChanges))
            {
                views.Children.Add(new TextBlock
                {
                    Text = DescriptionOfChanges,
                    WrapText = true,
                    Margin = new Thickness(0, 0, 0, 12)
                });
            }
            
            if (ProposedChanges != null)
            {
                foreach (var change in ProposedChanges)
                {
                    var changeView = AiTaskOrEventChangePreview.RenderChangeItem(change, new AiTaskOrEventChangePreview.CurrentItems
                    {
                        SemesterItems = _semesterItems,
                        Semester = _semester,
                        Classes = _classes
                    });
                    views.Children.Add(changeView);
                }
            }

            return new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = views
                    }.LinearLayoutWeight(1),

                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin / 2, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Children =
                        {
                            new Button
                            {
                                Text = R.S("String_Back"),
                                Click = GoBack,
                                Margin = new Thickness(0, 0, 6, 0)
                            }.LinearLayoutWeight(0.5f),

                            new AccentButton
                            {
                                Text = R.S("AiEdit_SaveChanges"),
                                Click = () => _ = SaveChanges(),
                                Margin = new Thickness(6, 0, 0, 0)
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };

            //views.Add(new AccentButton
            //{
            //    Text = "Save changes",
            //    Click = () => _ = SaveChanges(),
            //    Margin = new Thickness(0, 12, 0, 0)
            //});

            //views.Add(new Button
            //{
            //    Text = "Back",
            //    Click = GoBack,
            //    Margin = new Thickness(0, 6, 0, 0)
            //});

            //return RenderGenericPopupContent(views);
        }
    }
}
