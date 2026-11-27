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
    internal class GetTasksAndEventsToolResponse
    {
        [Description("The list of incomplete tasks.")]
        public List<ChatTaskItem> ActiveTasks { get; set; }

        [Description("The list of complete tasks.")]
        public List<ChatTaskItem> CompletedTasks { get; set; }
    }

    internal class GetTasksAndEventsTool : LocalTool<BlankParams, GetTasksAndEventsToolResponse>
    {
        public override string Name => "get_tasks_and_events";
        public override string Description => "Gets the list of tasks and events in the currently open semester, including active and complete tasks and events.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetTasksAndEventsTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
        }

        public override async Task<GetTasksAndEventsToolResponse> InvokeWithResultAsync(BlankParams parameters)
        {
            var semesterItemsViewGroup = SemesterItemsViewGroup.Load(_mainScreenViewModel.CurrentLocalAccountId, _mainScreenViewModel.CurrentSemester);
            await semesterItemsViewGroup.LoadingTask;

            return new GetTasksAndEventsToolResponse
            {
                ActiveTasks = semesterItemsViewGroup.Items
                    .OfType<ViewItemTaskOrEvent>()
                    .Where(i => i.Type == TaskOrEventType.Task && !i.IsComplete)
                    .Select(i => new ChatTaskItem(i)).ToList(),

                CompletedTasks = semesterItemsViewGroup.Items
                    .OfType<ViewItemTaskOrEvent>()
                    .Where(i => i.Type == TaskOrEventType.Task && i.IsComplete)
                    .Select(i => new ChatTaskItem(i)).ToList(),
            };
        }
    }
}
