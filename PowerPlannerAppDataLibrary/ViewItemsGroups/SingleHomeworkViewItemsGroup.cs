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
    public class SingleHomeworkViewItemsGroup : BaseSingleItemViewItemsGroup<ViewItemHomework>
    {
        private SingleHomeworkViewItemsGroup(Guid localAccountId, bool trackChanges) : base(localAccountId, trackChanges)
        {
        }

        public static async Task<SingleHomeworkViewItemsGroup> LoadAsync(Guid localAccountId, Guid homeworkId, bool trackChanges = true)
        {
            try
            {
                var answer = new SingleHomeworkViewItemsGroup(localAccountId, trackChanges);
                await Task.Run(async delegate { await answer.LoadBlocking(homeworkId); });
                return answer;
            }
            catch (SemesterNotFoundException) { return null; }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        protected override ViewItemHomework CreateItem(DataItemMegaItem dataItem, ViewItemClass c)
        {
            return new ViewItemHomework(dataItem)
            {
                Class = c
            };
        }
    }
}
