using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using Vx.Views;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade
{
    public class ViewGradeViewModel : PopupComponentViewModel
    {
        [VxSubscribe]
        public BaseViewItemMegaItem Grade { get; private set; }
        public bool IsInWhatIfMode { get; private set; }

        private ViewGradeViewModel(BaseViewModel parent, BaseViewItemMegaItem item) : base(parent)
        {
            Title = PowerPlannerResources.GetString("ViewGradePage.Title");
            Grade = item;

            Commands = new PopupCommand[]
            {
                PopupCommand.Edit(Edit)
            };

            UpdateSecondaryCommands();
        }

        private bool? _isDropped;
        private void UpdateSecondaryCommands()
        {
            if (_isDropped != null && _isDropped.Value == Grade.IsDropped)
            {
                return;
            }

            _isDropped = Grade.IsDropped;

            SecondaryCommands = new PopupCommand[]
            {
                Grade.IsDropped ? new PopupCommand(PowerPlannerResources.GetString("String_UndropGrade"), UndropGrade) : new PopupCommand(PowerPlannerResources.GetString("String_DropGrade"), DropGrade),
                PopupCommand.DeleteWithQuickConfirm(Delete)
            };
        }

        protected override View Render()
        {
            // This will be triggered every time property changed, so can use it to update the secondary commands menu too
            UpdateSecondaryCommands();

            return RenderGenericPopupContent(

                new TextBlock
                {
                    Text = Grade.Name,
                    FontSize = Theme.Current.TitleFontSize,
                    IsTextSelectionEnabled = true
                },

                new TextBlock
                {
                    Text = Grade.GradeSubtitle,
                    TextColor = Grade.WeightCategory.Class.Color.ToColor(),
                    Margin = new Thickness(0, 3, 0, 0),
                    IsTextSelectionEnabled = true
                },

                new TextBlock
                {
                    Text = Grade.WeightCategory.Name + " - " + Grade.Date.ToLongDateString(),
                    TextColor = Theme.Current.SubtleForegroundColor,
                    FontSize = Theme.Current.CaptionFontSize,
                    Margin = new Thickness(0, 3, 0, 0),
                    IsTextSelectionEnabled = true
                },

                Grade.IsDropped ? new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ViewGradePage_TextBlockDropped.Text"),
                    FontSize = Theme.Current.TitleFontSize,
                    Margin = new Thickness(0, 18, 0, 0)
                } : null,

                !string.IsNullOrWhiteSpace(Grade.Details) ? new HyperlinkTextBlock
                {
                    Text = Grade.Details,
                    Margin = new Thickness(0, 18, 0, 0),
                    IsTextSelectionEnabled = true
                } : null

            );
        }

        public static ViewGradeViewModel Create(BaseViewModel parent, BaseViewItemMegaItem item, bool isInWhatIfMode = false)
        {
            return new ViewGradeViewModel(parent, item)
            {
                IsInWhatIfMode = isInWhatIfMode
            };
        }

        public void Edit()
        {
            MainScreenViewModel.EditGrade(Grade, IsInWhatIfMode);
        }

        public void ConfirmDelete()
        {

        }

        public void Delete()
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                if (IsInWhatIfMode)
                {
                    Grade.WeightCategory.Remove(Grade);
                    Grade.WeightCategory.Class.ResetDream();
                }
                else
                {
                    // Tasks/events don't actually get deleted, just excluded from grades
                    if (Grade is ViewItemTaskOrEvent taskOrEvent)
                    {
                        var dataItem = taskOrEvent.CreateBlankDataItem();
                        dataItem.WeightCategoryIdentifier = PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED;

                        DataChanges changes = new DataChanges();
                        changes.Add(dataItem);

                        await PowerPlannerApp.Current.SaveChanges(changes);
                    }
                    else
                    {
                        await MainScreenViewModel.DeleteItem(Grade);
                    }
                }

            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public void DropGrade()
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                if (IsInWhatIfMode)
                {
                    Grade.IsDropped = true;
                    Grade.WasChanged = true;
                    Grade.WeightCategory.Class.ResetDream();
                }
                else
                {
                    var g = Grade.CreateBlankDataItem();
                    g.IsDropped = true;

                    DataChanges changes = new DataChanges();
                    changes.Add(g);

                    await PowerPlannerApp.Current.SaveChanges(changes);
                }
            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public async void UndropGrade()
        {
            try
            {
                if (IsInWhatIfMode)
                {
                    Grade.IsDropped = false;
                    Grade.WasChanged = true;
                    Grade.WeightCategory.Class.ResetDream();
                }
                else
                {
                    var g = Grade.CreateBlankDataItem();
                    g.IsDropped = false;

                    DataChanges changes = new DataChanges();
                    changes.Add(g);

                    await PowerPlannerApp.Current.SaveChanges(changes);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
