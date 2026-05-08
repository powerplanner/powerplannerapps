using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerSending;
using Task = System.Threading.Tasks.Task;
using PowerPlannerAppDataLibrary.Services;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using ToolsPortable;
using Vx.Views;

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

            Title = "Edit with AI";
            UseCancelForBack();
        }

        public async Task PreviewResults()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
            {
                ErrorMessage = "Please describe the changes you'd like to make.";
                return;
            }

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
                ErrorMessage = "Something went wrong. Please try again.";
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
                ErrorMessage = "Failed to save changes. Please try again.";
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

                if (itemType == AiItemType.Holiday)
                {
                    dataItem.Date = date;
                    if (change.EndDate.HasValue)
                    {
                        var endDate = change.EndDate.Value.ToDateTime(TimeOnly.MinValue);
                        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                        dataItem.EndTime = endDate.AddDays(1).AddSeconds(-1);
                    }
                    else
                    {
                        dataItem.EndTime = date.AddDays(1).AddSeconds(-1);
                    }
                }
                else
                {
                    var effectiveType = isNew ? itemType : GetEffectiveType(dataItem.MegaItemType);
                    ApplyTimeEncoding(dataItem, date, change.Time, change.EndTime, effectiveType);
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

        public void GoBack()
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

        private View RenderInputState()
        {
            var views = new List<View>
            {
                new MultilineTextBox
                {
                    Text = VxValue.Create(UserInput, v => UserInput = v),
                    PlaceholderText = "Describe changes...",
                    AutoFocus = true
                }
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
                Text = "Preview results",
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
            var views = new List<View>();

            if (!string.IsNullOrEmpty(DescriptionOfChanges))
            {
                views.Add(new TextBlock
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
                    var changeView = RenderChangeItem(change);
                    views.Add(changeView);
                }
            }

            views.Add(new AccentButton
            {
                Text = "Save changes",
                Click = () => _ = SaveChanges(),
                Margin = new Thickness(0, 12, 0, 0)
            });

            views.Add(new Button
            {
                Text = "Back",
                Click = GoBack,
                Margin = new Thickness(0, 6, 0, 0)
            });

            return RenderGenericPopupContent(views);
        }

        private View RenderChangeItem(AiProposedChange change)
        {
            Color accentColor;
            string operationLabel;
            switch (change.Operation)
            {
                case AiChangeOperation.Add:
                    accentColor = Color.FromArgb(255, 76, 175, 80); // Green
                    operationLabel = "ADD";
                    break;
                case AiChangeOperation.Edit:
                    accentColor = Color.FromArgb(255, 255, 193, 7); // Yellow/Orange
                    operationLabel = "EDIT";
                    break;
                case AiChangeOperation.Delete:
                    accentColor = Color.FromArgb(255, 244, 67, 54); // Red
                    operationLabel = "DELETE";
                    break;
                default:
                    accentColor = Color.Gray;
                    operationLabel = "";
                    break;
            }

            // Find existing item for edit/delete context
            BaseViewItemMegaItem existingItem = null;
            if (change.ExistingItemId.HasValue)
            {
                existingItem = _semesterItems.Items.FirstOrDefault(i => i.Identifier == change.ExistingItemId.Value);
            }

            // Build the item name - use existing name if the change doesn't provide one
            string itemName = change.Name ?? (existingItem?.Name) ?? "Unknown item";

            var contentLayout = new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    // Item name and operation badge
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = operationLabel,
                                FontSize = 10,
                                TextColor = accentColor,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 0, 6, 0),
                                WrapText = false
                            },

                            new TextBlock
                            {
                                Text = itemName,
                                FontWeight = FontWeights.SemiBold,
                                WrapText = true,
                                VerticalAlignment = VerticalAlignment.Center
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };

            // For edits, show before → after details
            if (change.Operation == AiChangeOperation.Edit && existingItem != null)
            {
                var detailLines = GetEditDetails(change, existingItem);
                foreach (var line in detailLines)
                {
                    contentLayout.Children.Add(new TextBlock
                    {
                        Text = line,
                        FontSize = Theme.Current.CaptionFontSize,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        WrapText = true,
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                }
            }
            // For adds, show key details
            else if (change.Operation == AiChangeOperation.Add)
            {
                var detailLines = GetAddDetails(change);
                foreach (var line in detailLines)
                {
                    contentLayout.Children.Add(new TextBlock
                    {
                        Text = line,
                        FontSize = Theme.Current.CaptionFontSize,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        WrapText = true,
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                }
            }
            // For deletes, show existing item info
            else if (change.Operation == AiChangeOperation.Delete && existingItem != null)
            {
                var datePart = existingItem.DateInSchoolTime.ToString("MMM d, yyyy");
                contentLayout.Children.Add(new TextBlock
                {
                    Text = datePart,
                    FontSize = Theme.Current.CaptionFontSize,
                    TextColor = Theme.Current.SubtleForegroundColor,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }

            return contentLayout;

            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 6, 0, 6),
                Children =
                {
                    new CheckBox
                    {
                        IsChecked = VxValue.Create(change.IsSelected, v => change.IsSelected = v), 
                    },

                    contentLayout.LinearLayoutWeight(1)
                }
            };
        }

        private List<string> GetEditDetails(AiProposedChange change, BaseViewItemMegaItem existingItem)
        {
            var lines = new List<string>();

            // Date change
            if (change.Date.HasValue)
            {
                var oldDate = existingItem.DateInSchoolTime.ToString("MMM d, yyyy");
                var newDate = change.Date.Value.ToString("MMM d, yyyy");
                if (oldDate != newDate)
                {
                    lines.Add($"Date: {oldDate} → {newDate}");
                }
            }

            // Name change
            if (!string.IsNullOrEmpty(change.Name) && change.Name != existingItem.Name)
            {
                lines.Add($"Name: \"{existingItem.Name}\" → \"{change.Name}\"");
            }

            // Time change
            if (!string.IsNullOrEmpty(change.Time))
            {
                lines.Add($"Time: {change.Time}");
            }

            // Details change
            if (change.Details != null && change.Details != existingItem.Details)
            {
                if (string.IsNullOrEmpty(existingItem.Details))
                {
                    lines.Add($"Details: \"{Truncate(change.Details, 50)}\"");
                }
                else
                {
                    lines.Add($"Details: \"{Truncate(existingItem.Details, 30)}\" → \"{Truncate(change.Details, 30)}\"");
                }
            }

            // Percent complete change
            if (change.PercentComplete.HasValue)
            {
                lines.Add($"Progress: {(int)(change.PercentComplete.Value * 100)}%");
            }

            if (lines.Count == 0)
            {
                lines.Add("(minor changes)");
            }

            return lines;
        }

        private List<string> GetAddDetails(AiProposedChange change)
        {
            var lines = new List<string>();

            if (change.Type.HasValue)
            {
                lines.Add(change.Type.Value.ToString());
            }

            if (change.Date.HasValue)
            {
                var datePart = change.Date.Value.ToString("MMM d, yyyy");
                if (!string.IsNullOrEmpty(change.Time) && change.Time.ToLowerInvariant() != "allday")
                {
                    lines.Add($"{datePart}, {change.Time}");
                }
                else
                {
                    lines.Add(datePart);
                }
            }

            if (!string.IsNullOrEmpty(change.Details))
            {
                lines.Add(Truncate(change.Details, 60));
            }

            return lines;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "…";
        }
    }
}
