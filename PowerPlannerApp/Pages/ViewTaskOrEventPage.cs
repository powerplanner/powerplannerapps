using MaterialDesign;
using PowerPlannerApp.ViewItems;
using PowerPlannerApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages
{
    public class ViewTaskOrEventPage : VxPage
    {
        public ViewItemTaskOrEvent Item { get; set; }

        public bool IsUnassigedMode { get; private set; }

        public string PageTitle
        {
            get
            {
                if (Item.Type == TaskOrEventType.Task)
                {
                    return PowerPlannerResources.GetString("String_ViewTask").ToUpper();
                }
                else
                {
                    return PowerPlannerResources.GetString("String_ViewEvent").ToUpper();
                }
            }
        }

        protected override View Render()
        {
            return new PopupWindow
            {
                Title = PageTitle,
                Commands = new PopupWindowCommand[]
                {
                    new PopupWindowCommand
                    {
                        Glyph = MaterialDesignIcons.Edit,
                        Title = "Edit",
                        Action = Edit
                    }
                },
                SecondaryCommands = new PopupWindowCommand[]
                {
                    new PopupWindowCommand
                    {
                        Title = "Convert to event"
                    },
                    new PopupWindowCommand
                    {
                        Title = "Delete"
                    }
                },
                Content = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Star },
                        new RowDefinition { Height = GridLength.Auto }
                    },

                    Children =
                    {
                        new ScrollView
                        {
                            Content = new StackLayout
                            {
                                Margin = new Thickness(12),
                                Children =
                                {
                                    new Label { Text = Item.Name, FontSize = 20 },

                                    new Label { Text = Item.Subtitle },

                                    new Label { Text = Item.Details }
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// If we're in unassigned mode (viewing an unassigned grade), we always hide completion slider. Else we only show it if it's a task.
        /// </summary>
        public bool IsCompletionSliderVisible => Item.Type == TaskOrEventType.Task && !IsUnassigedMode;

        public bool IsButtonAddGradeVisible => IsUnassigedMode;

        public void Edit()
        {
            //MainScreenViewModel.EditTaskOrEvent(Item);
        }

        /// <summary>
        /// Toggles the item type. If it's a task, becomes an event, and vice versa.
        /// </summary>
        public void ConvertType()
        {
            //MainScreenViewModel.ConvertTaskOrEventType(Item, this);
        }

        public void Delete()
        {
            //TryStartDataOperationAndThenNavigate(delegate
            //{
            //    if (IsUnassigedMode)
            //    {
            //        var changedItem = Item.CreateBlankDataItem();
            //        changedItem.WeightCategoryIdentifier = PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED;

            //        var changes = new DataChanges();
            //        changes.Add(changedItem);

            //        return PowerPlannerApp.Current.SaveChanges(changes);
            //    }
            //    else
            //    {
            //        return MainScreenViewModel.DeleteItem(Item);
            //    }

            //}, delegate
            //{
            //    this.GoBack();
            //});
        }

        public void SetPercentComplete(double percentComplete)
        {
            //ViewItemTaskOrEvent task = Item as ViewItemTaskOrEvent;
            //if (task == null || task.PercentComplete == percentComplete || task.Type != TaskOrEventType.Task)
            //{
            //    return;
            //}

            //MainScreenViewModel.SetTaskPercentComplete(task, percentComplete);

            //// Go back immediately
            //if (percentComplete == 1)
            //{
            //    this.GoBack();
            //}
        }

        public async void AddGrade(bool showViewGradeSnackbarAfterSaving = false)
        {
            //await TryHandleUserInteractionAsync("AddGradeFromExisting", async (cancellationToken) =>
            //{
            //    // For free version, block assigning grade if number of graded items exceeds 5
            //    if (Item.Class.WeightCategories.SelectMany(i => i.Grades).Where(i => i.GradeReceived != PowerPlannerSending.Grade.UNGRADED).Count() >= 5
            //        && !await PowerPlannerApp.Current.IsFullVersionAsync())
            //    {
            //        cancellationToken.ThrowIfCancellationRequested();

            //        PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeGradesLimitReached"));
            //        return;
            //    }

            //    var model = AddGradeViewModel.CreateForEdit(MainScreenViewModel, new Grade.AddGradeViewModel.EditParameter()
            //    {
            //        Item = Item,
            //        OnSaved = delegate { this.RemoveViewModel(); },
            //        IsUnassignedItem = true,
            //        ShowViewGradeSnackbarAfterSaving = showViewGradeSnackbarAfterSaving
            //    });

            //    MainScreenViewModel.ShowPopup(model);
            //});
        }
    }
}
