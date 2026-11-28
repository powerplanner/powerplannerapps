using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    public class DeleteItemsToolRequest
    {
        [Description("The list of IDs to delete.")]
        [NotNull]
        [DisallowNull]
        public required List<Guid> IdsToDelete { get; set; }
    }
    internal class DeleteItemsTool : LocalTool<DeleteItemsToolRequest, object>
    {
        public override string Name => "delete_" + PluralItemTypeName;
        public override string Description => $"Deletes the specified {PluralItemTypeName} from the account{(HasChildren ? ", including all descendants" : "")}.";
        public required string PluralItemTypeName { get; init; }
        public required bool HasChildren { get; init; }
        public MainScreenViewModel MainScreenViewModel { get; private set; }
        public DeleteItemsTool(MainScreenViewModel mainScreenViewModel)
        {
            MainScreenViewModel = mainScreenViewModel;
            RequiresApproval = true;
            IsDeleteItems = true;
            DisplayName = "Delete " + PluralItemTypeName;
        }
        public override async Task<object> InvokeWithResultAsync(DeleteItemsToolRequest parameters)
        {
            if (parameters.IdsToDelete.Count == 0)
            {
                throw new ArgumentException("You must provide at least one ID to delete.");
            }

            DataChanges changes = new DataChanges();
            foreach (var d in parameters.IdsToDelete)
            {
                changes.DeleteItem(d);
            }
            await MainScreenViewModel.SaveChanges(changes);
            return "Successfully deleted " + parameters.IdsToDelete.Count + " items.";
        }
    }
}
