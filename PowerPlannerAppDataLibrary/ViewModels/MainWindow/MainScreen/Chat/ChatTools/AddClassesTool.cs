using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatValidators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class AddClassesTool : AddOrEditItemsTool<AddOrEditClassDataItem>
    {
        public AddClassesTool(MainScreenViewModel mainScreenViewModel) : base(mainScreenViewModel)
        {
            Mode = AddOrEditMode.Add;
        }

        protected override async Task SaveItemsAsync(List<AddOrEditClassDataItem> items)
        {
            var newIds = await PowerPlannerApp.Current.NewGuidsForCurrentAccountAsync(items.Count * 2);
            Stack<Guid> newIdsStack = new(newIds);

            DataChanges changes = new DataChanges();

            foreach (var c in items)
            {
                var classChanges = AccountDataStore.GenerateNewDefaultClass(
                    account: MainScreenViewModel.CurrentAccount,
                    semesterId: Guid.Empty,
                    newClassId: newIdsStack.Pop(),
                    name: "",
                    rawColor: [0,0,0],
                    newWeightId: newIdsStack.Pop());

                var newClass = classChanges[0] as DataItemClass;

                // Actually populate the class with the properties
                c.PopulateDataItem(newClass, Mode);

                foreach (var change in classChanges)
                {
                    changes.Add(change);
                }
            }

            await MainScreenViewModel.SaveChanges(changes);
        }
    }
}
