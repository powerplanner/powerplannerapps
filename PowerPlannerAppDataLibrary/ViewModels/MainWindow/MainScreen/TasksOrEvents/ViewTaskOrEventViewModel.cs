﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using ToolsPortable;
using System.ComponentModel;
using PowerPlannerSending;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using BareMvvm.Core.Snackbar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents
{
    public class ViewTaskOrEventViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemTaskOrEvent Item { get; private set; }

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

        private ViewTaskOrEventViewModel(BaseViewModel parent, ViewItemTaskOrEvent item) : base(parent)
        {
            Item = item;
            Item.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Item_PropertyChanged).Handler;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // When the item type changes, these dependent values need to change
                case nameof(Item.Type):
                    OnPropertyChanged(
                        nameof(PageTitle),
                        nameof(IsCompletionSliderVisible),
                        nameof(ConvertTypeButtonText));
                    break;
            }
        }

        public override string GetPageName()
        {
            if (Item.Type == TaskOrEventType.Task)
            {
                return "ViewTaskView";
            }
            else
            {
                return "ViewEventView";
            }
        }

        public string ConvertTypeButtonText
        {
            get => Item.Type == TaskOrEventType.Task ? PowerPlannerResources.GetString("String_ConvertToEvent") : PowerPlannerResources.GetString("String_ConvertToTask");
        }

        public static ViewTaskOrEventViewModel Create(BaseViewModel parent, ViewItemTaskOrEvent item)
        {
            return new ViewTaskOrEventViewModel(parent, item);
        }

        /// <summary>
        /// Used for the grade page, when user clicks on an unassigned item
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ViewTaskOrEventViewModel CreateForUnassigned(BaseViewModel parent, ViewItemTaskOrEvent item)
        {
            return new ViewTaskOrEventViewModel(parent, item)
            {
                IsUnassigedMode = true
            };
        }

        /// <summary>
        /// If we're in unassigned mode (viewing an unassigned grade), we always hide completion slider. Else we only show it if it's a task.
        /// </summary>
        public bool IsCompletionSliderVisible => Item.Type == TaskOrEventType.Task && !IsUnassigedMode;

        public bool IsButtonAddGradeVisible => IsUnassigedMode;

        public void Edit()
        {
            MainScreenViewModel.EditTaskOrEvent(Item);
        }

        /// <summary>
        /// Toggles the item type. If it's a task, becomes an event, and vice versa.
        /// </summary>
        public void ConvertType()
        {
            MainScreenViewModel.ConvertTaskOrEventType(Item, this);
        }
        
        public void Delete()
        {
            TryStartDataOperationAndThenNavigate(delegate
            {
                if (IsUnassigedMode)
                {
                    var changedItem = Item.CreateBlankDataItem();
                    changedItem.WeightCategoryIdentifier = PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED;

                    var changes = new DataChanges();
                    changes.Add(changedItem);

                    return PowerPlannerApp.Current.SaveChanges(changes);
                }
                else
                {
                    return MainScreenViewModel.DeleteItem(Item);
                }

            }, delegate
            {
                this.GoBack();
            });
        }

        public void SetPercentComplete(double percentComplete)
        {
            ViewItemTaskOrEvent task = Item as ViewItemTaskOrEvent;
            if (task == null || task.PercentComplete == percentComplete || task.Type != TaskOrEventType.Task)
            {
                return;
            }

            MainScreenViewModel.SetTaskPercentComplete(task, percentComplete);

            // Go back immediately
            if (percentComplete == 1)
            {
                this.GoBack();
            }
        }

        public async void AddGrade(bool showViewGradeSnackbarAfterSaving = false)
        {
            await TryHandleUserInteractionAsync("AddGradeFromExisting", async (cancellationToken) =>
            {
                // For free version, block assigning grade if number of graded items exceeds 5
                if (Item.Class.WeightCategories.SelectMany(i => i.Grades).Where(i => i.GradeReceived != PowerPlannerSending.Grade.UNGRADED).Count() >= 5
                    && !await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeGradesLimitReached"));
                    return;
                }

                var model = AddGradeViewModel.CreateForEdit(MainScreenViewModel, new Grade.AddGradeViewModel.EditParameter()
                {
                    Item = Item,
                    OnSaved = delegate { this.RemoveViewModel(); },
                    IsUnassignedItem = true,
                    ShowViewGradeSnackbarAfterSaving = showViewGradeSnackbarAfterSaving
                });

                MainScreenViewModel.ShowPopup(model);
            });
        }
    }
}
