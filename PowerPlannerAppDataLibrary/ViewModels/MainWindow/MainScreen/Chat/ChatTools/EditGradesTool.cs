using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class EditGradesTool : AddOrEditItemsTool<AddOrEditGradeDataItem>
    {
        public EditGradesTool(MainScreenViewModel mainScreenViewModel) : base(mainScreenViewModel)
        {
        }

        protected override async Task SaveItemsAsync(List<AddOrEditGradeDataItem> items)
        {
            if (Mode != AddOrEditMode.Edit)
            {
                throw new InvalidOperationException("Only Edit mode allowed for this tool");
            }

            DataChanges changes = new DataChanges();
            var yearsGroup = await YearsViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentLocalAccountId);

            Dictionary<Guid, BaseViewItemMegaItem> existingMegaItems = yearsGroup.School.Years.SelectMany(i => i.Semesters.SelectMany(i => i.Classes.SelectMany(i => i.WeightCategories.SelectMany(i => i.Grades)))).ToDictionary(i => i.Identifier, i => i);

            foreach (var i in items)
            {
                var identifier = (Guid)i.Properties.First(i => i.Name == "id").GetValue();

                if (existingMegaItems.TryGetValue(identifier, out var existingItem))
                {
                    i.ExistingItem = existingItem;
                    var dataItem = i.CreateDataItem(Mode);
                    changes.Add(dataItem, onlyEdit: true);
                }
            }

            await MainScreenViewModel.SaveChanges(changes);
        }
    }
}
