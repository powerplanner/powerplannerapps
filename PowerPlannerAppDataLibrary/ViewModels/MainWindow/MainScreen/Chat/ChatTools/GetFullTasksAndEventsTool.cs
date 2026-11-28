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
    internal class GetFullTasksAndEventsToolRequest
    {
        [Description("Required: The IDs of the semester to retrieve tasks and events from.")]
        public required Guid SemesterId { get; set; }

        [Description("The IDs of the tasks to retrieve.")]
        public required Guid[] Tasks { get; set; }

        [Description("The IDs of the events to retrieve.")]
        public required Guid[] Events { get; set; }
    }

    internal class GetFullTasksAndEventsToolResponse
    {
        [Description("The list of tasks, with all properties and details fully populated.")]
        public required List<ChatFullTaskItem> Tasks { get; set; }

        [Description("The list of events, with all properties and details fully populated.")]
        public required List<ChatFullEventItem> Events { get; set; }
    }

    internal class GetFullTasksAndEventsTool : LocalTool<GetFullTasksAndEventsToolRequest, GetFullTasksAndEventsToolResponse>
    {
        public override string Name => "list_full_tasks_and_events";
        public override string Description => "Lists the specified tasks and events from the specified semester, returning fully populated items, with all properties and details populated.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetFullTasksAndEventsTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Get specific tasks and events";
        }

        public override async Task<GetFullTasksAndEventsToolResponse> InvokeWithResultAsync(GetFullTasksAndEventsToolRequest parameters)
        {
            var semesterItemsViewGroup = await _mainScreenViewModel.LoadSemesterItemsViewGroupAsync(parameters.SemesterId);
            await semesterItemsViewGroup.LoadingTask;

            DateTime today = DateTime.Today;

            return new GetFullTasksAndEventsToolResponse
            {
                Tasks = semesterItemsViewGroup.Items
                    .OfType<ViewItemTaskOrEvent>()
                    .Where(i => i.Type == TaskOrEventType.Task && parameters.Tasks.Contains(i.Identifier))
                    .Select(i => new ChatFullTaskItem(i)).ToList(),
                Events = semesterItemsViewGroup.Items
                    .OfType<ViewItemTaskOrEvent>()
                    .Where(i => i.Type == TaskOrEventType.Event && parameters.Events.Contains(i.Identifier))
                    .Select(i => new ChatFullEventItem(i)).ToList()
            };
        }
    }
}
