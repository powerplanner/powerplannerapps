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
    internal class GetTasksAndEventsFromClassTool : LocalTool<ClassInputParams, GetTasksAndEventsToolResponse>
    {
        public override string Name => "list_tasks_and_events_from_class";
        public override string Description => "Lists the tasks and events from the specified class, including active and complete tasks and events.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetTasksAndEventsFromClassTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Get tasks and events from class";
        }

        public override async Task<GetTasksAndEventsToolResponse> InvokeWithResultAsync(ClassInputParams parameters)
        {
            var classViewItemsGroup = await _mainScreenViewModel.LoadClassViewItemsGroupAsync(parameters.ClassId);
            classViewItemsGroup.LoadTasksAndEvents();
            await classViewItemsGroup.LoadTasksAndEventsTask;
            if (classViewItemsGroup.HasPastCompletedEvents)
            {
                classViewItemsGroup.ShowPastCompletedEvents();
                await classViewItemsGroup.LoadPastCompleteTasksAndEventsTask;
            }
            else if (classViewItemsGroup.HasPastCompletedTasks)
            {
                classViewItemsGroup.ShowPastCompletedTasks();
                await classViewItemsGroup.LoadPastCompleteTasksAndEventsTask;
            }

            return new GetTasksAndEventsToolResponse
            {
                ActiveTasks = classViewItemsGroup.Tasks
                    .OfType<ViewItemTaskOrEvent>()
                    .Select(i => new ChatTaskItem(i)).ToList(),

                CompletedTasks = classViewItemsGroup.PastCompletedTasks
                    .Select(i => new ChatTaskItem(i)).ToList(),

                ActiveEvents = classViewItemsGroup.Events
                    .Select(i => new ChatEventItem(i)).ToList(),

                PastEvents = classViewItemsGroup.PastCompletedEvents
                    .Select(i => new ChatEventItem(i)).ToList(),
            };
        }
    }
}
