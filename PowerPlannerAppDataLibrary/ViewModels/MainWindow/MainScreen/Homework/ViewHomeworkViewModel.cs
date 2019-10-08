using System;
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

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework
{
    public class ViewHomeworkViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemTaskOrEvent Item { get; private set; }

        public bool IsUnassigedMode { get; private set; }

        private ViewHomeworkViewModel(BaseViewModel parent) : base(parent)
        {
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

        public static ViewHomeworkViewModel Create(BaseViewModel parent, ViewItemTaskOrEvent item)
        {
            return new ViewHomeworkViewModel(parent)
            {
                Item = item
            };
        }

        /// <summary>
        /// Used for the grade page, when user clicks on an unassigned item
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ViewHomeworkViewModel CreateForUnassigned(BaseViewModel parent, ViewItemTaskOrEvent item)
        {
            return new ViewHomeworkViewModel(parent)
            {
                Item = item,
                IsUnassigedMode = true
            };
        }

        public void Edit()
        {
            MainScreenViewModel.EditHomeworkOrExam(Item);
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
            ViewItemTaskOrEvent homework = Item as ViewItemTaskOrEvent;
            if (homework == null || homework.PercentComplete == percentComplete)
            {
                return;
            }

            TryStartDataOperationAndThenNavigate(delegate
            {
                DataChanges changes = new DataChanges();

                changes.Add(new DataItemMegaItem()
                {
                    Identifier = homework.Identifier,
                    PercentComplete = percentComplete
                });

                return PowerPlannerApp.Current.SaveChanges(changes);

            }, delegate
            {
                // Go back immediately before 
                if (percentComplete == 1)
                    this.GoBack();
            });
        }

        public async void ConvertToGrade()
        {
            await TryHandleUserInteractionAsync("ConvertToGrade", async (cancellationToken) =>
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
                    IsUnassignedItem = true
                });

                MainScreenViewModel.ShowPopup(model);
            });
        }
    }
}
