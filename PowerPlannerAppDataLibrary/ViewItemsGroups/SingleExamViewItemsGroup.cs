using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Exceptions;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    public class SingleExamViewItemsGroup : BaseSingleItemViewItemsGroup<ViewItemExam>
    {
        private SingleExamViewItemsGroup(Guid localAccountId, bool trackChanges) : base(localAccountId, trackChanges)
        {
        }

        public static async Task<SingleExamViewItemsGroup> LoadAsync(Guid localAccountId, Guid examId, bool trackChanges = true)
        {
            try
            {
                var answer = new SingleExamViewItemsGroup(localAccountId, trackChanges);
                await Task.Run(async delegate { await answer.LoadBlocking(examId); });
                return answer;
            }
            catch (SemesterNotFoundException) { return null; }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        protected override ViewItemExam CreateItem(DataItemMegaItem dataItem, ViewItemClass c)
        {
            return new ViewItemExam(dataItem)
            {
                Class = c
            };
        }
    }
}
