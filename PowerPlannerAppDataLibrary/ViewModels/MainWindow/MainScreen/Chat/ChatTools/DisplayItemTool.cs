using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class DisplayItemToolParams
    {
        [Description("The id of the item to open.")]
        [NotNull]
        [DisallowNull]
        public required Guid Id { get; set; }
    }

    internal class DisplayItemTool : LocalTool<DisplayItemToolParams, string>
    {
        public override string Name => "open_" + ItemName;
        public override string Description => $"Opens the {ItemName} in the app.";
        public required PowerPlannerSending.ItemType ItemType { get; set; }

        /// <summary>
        /// Specify the item name in lowercase, like "task" or "class"
        /// </summary>
        public required string ItemName { get; set; }

        private MainScreenViewModel _mainScreenViewModel;
        public DisplayItemTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
        }

        public override async Task<string> InvokeWithResultAsync(DisplayItemToolParams parameters)
        {
            const string NOT_FOUND = "Error: Couldn't find the item to display.";
            const string SUCCESS = "Successfully displayed the item.";

            try
            {
                switch (ItemType)
                {
                    case PowerPlannerSending.ItemType.Class:
                        var c = _mainScreenViewModel.Classes.FirstOrDefault(i => i.Identifier == parameters.Id);
                        if (c == null)
                        {
                            return NOT_FOUND;
                        }
                        _mainScreenViewModel.ViewClass(c);
                        return SUCCESS;

                    case PowerPlannerSending.ItemType.MegaItem:
                        if (ItemName == "task_or_event")
                        {
                            if (await _mainScreenViewModel.ShowTaskOrEventAsync(parameters.Id))
                            {
                                return SUCCESS;
                            }
                            return NOT_FOUND;
                        }
                        if (ItemName == "holiday")
                        {
                            if (await _mainScreenViewModel.ShowHolidayAsync(parameters.Id))
                            {
                                return SUCCESS;
                            }
                            return NOT_FOUND;
                        }
                        return NOT_FOUND;
                        break;

                    default:
                        return NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                return "Unknown error occurred.";
            }
        }
    }
}
