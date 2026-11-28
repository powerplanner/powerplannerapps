using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class GetHolidaysTool : LocalTool<SemesterParams, List<ChatHolidayItem>>
    {
        public override string Name => "list_holidays";
        public override string Description => "Lists the holidays from the specified semester, including future and past holidays.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetHolidaysTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Get holidays";
        }

        public override async Task<List<ChatHolidayItem>> InvokeWithResultAsync(SemesterParams parameters)
        {
            var semesterItemsViewGroup = await _mainScreenViewModel.LoadSemesterItemsViewGroupAsync(parameters.SemesterIdentifier);
            await semesterItemsViewGroup.LoadingTask;

            return semesterItemsViewGroup.Items
                .OfType<ViewItemHoliday>()
                .Select(i => new ChatHolidayItem(i))
                .ToList();
        }
    }
}
