using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class DeleteGradesTool : DeleteItemsTool
    {
        public DeleteGradesTool(MainScreenViewModel mainScreenViewModel) : base(mainScreenViewModel)
        {
        }

        public override async Task<object> InvokeWithResultAsync(DeleteItemsToolRequest parameters)
        {
            if (parameters.IdsToDelete.Count == 0)
            {
                throw new ArgumentException("You must provide at least one ID to delete.");
            }

            // We need to handle deleting grades differently, because if it's a MegaItem, we don't want to delete the actual MegaItem, only its grade portion.
            var yearsGroup = await YearsViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentLocalAccountId);

            Dictionary<Guid, BaseViewItemMegaItem> existingMegaItems = yearsGroup.School.Years.SelectMany(i => i.Semesters.SelectMany(i => i.Classes.SelectMany(i => i.WeightCategories.SelectMany(i => i.Grades)))).ToDictionary(i => i.Identifier, i => i);

            DataChanges changes = new DataChanges();

            foreach (var id in parameters.IdsToDelete)
            {
                if (existingMegaItems.TryGetValue(id, out var existingItem))
                {
                    changes.Add(new DataItemMegaItem
                    {
                        Identifier = id,
                        WeightCategoryIdentifier = PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED
                    }, onlyEdit: true);
                }
                else
                {
                    changes.DeleteItem(id);
                }
            }

            await MainScreenViewModel.SaveChanges(changes);
            return "Successfully deleted " + parameters.IdsToDelete.Count + " items.";
        }
    }
}
