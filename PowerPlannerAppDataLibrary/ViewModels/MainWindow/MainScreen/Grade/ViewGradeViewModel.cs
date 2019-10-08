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

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade
{
    public class ViewGradeViewModel : BaseMainScreenViewModelChild
    {
        public BaseViewItemMegaItem Grade { get; private set; }
        public bool IsInWhatIfMode { get; private set; }

        public ViewGradeViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public static ViewGradeViewModel Create(BaseViewModel parent, BaseViewItemMegaItem item, bool isInWhatIfMode = false)
        {
            return new ViewGradeViewModel(parent)
            {
                Grade = item,
                IsInWhatIfMode = isInWhatIfMode
            };
        }

        public void Edit()
        {
            MainScreenViewModel.EditGrade(Grade, IsInWhatIfMode);
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
                    // Homeworks/exams don't actually get deleted, just excluded from grades
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
