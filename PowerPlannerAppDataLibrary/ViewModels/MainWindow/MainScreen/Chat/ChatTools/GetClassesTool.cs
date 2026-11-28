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
    internal class GetClassesTool : LocalTool<SemesterParams, List<ChatClassItem>>
    {
        // Does NOT include weight categories
        public override string Name => "list_classes_with_schedules";
        public override string Description => "Lists all classes in a semester, including each class's id, name, color, credits, dates, details, and recurring schedule entries.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetClassesTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Get classes and schedule from semester";
        }

        public override async Task<List<ChatClassItem>> InvokeWithResultAsync(SemesterParams parameters)
        {
            if (_mainScreenViewModel.CurrentSemesterId == parameters.SemesterIdentifier)
            {
                return _mainScreenViewModel.Classes.Select(i => new ChatClassItem(i)).ToList();
            }
            else
            {
                var scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(_mainScreenViewModel.CurrentLocalAccountId, parameters.SemesterIdentifier, trackChanges: false, includeWeightCategories: false);
                return scheduleViewItemsGroup.Classes.Select(i => new ChatClassItem(i)).ToList();
            }
        }
    }
}
