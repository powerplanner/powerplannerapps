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
        [Description("The list of identifiers to delete.")]
        [NotNull]
        [DisallowNull]
        public List<Guid> IdentifiersToDelete { get; set; }
    }
    internal class DeleteItemsTool : LocalTool<DeleteItemsToolRequest, object>
    {
        public override string Name => "delete_" + PluralItemTypeName;
        public override string Description => $"Deletes the specified {PluralItemTypeName} from the account, including any children underneath it if applicable.";
        public required string PluralItemTypeName { get; init; }
        private MainScreenViewModel _mainScreenViewModel;
        public DeleteItemsTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            RequiresApproval = true;
            IsDeleteItems = true;
            DisplayName = "Delete " + PluralItemTypeName;
        }
        public override async Task<object> InvokeWithResultAsync(DeleteItemsToolRequest parameters)
        {
            if (parameters.IdentifiersToDelete.Count == 0)
            {
                throw new ArgumentException("You must provide at least one identifier to delete.");
            }

            DataChanges changes = new DataChanges();
            foreach (var d in parameters.IdentifiersToDelete)
            {
                changes.DeleteItem(d);
            }
            await _mainScreenViewModel.SaveChanges(changes);
            return "Successfully deleted " + parameters.IdentifiersToDelete.Count + " items.";
        }
    }
}
