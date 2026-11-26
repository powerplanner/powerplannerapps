using PowerPlannerAppDataLibrary.ViewItems;
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
    internal class GetClassesTool : LocalTool<BlankParams, List<ChatClassItem>>
    {
        public override string Name => "get_classes";
        public override string Description => "Gets the list of classes in the currently open semester.";
        private MainScreenViewModel _mainScreenViewModel;

        public GetClassesTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
        }

        public override Task<List<ChatClassItem>> InvokeWithResultAsync(BlankParams parameters)
        {
            return Task.FromResult(_mainScreenViewModel.Classes.Select(i => new ChatClassItem(i)).ToList());
        }
    }
}
