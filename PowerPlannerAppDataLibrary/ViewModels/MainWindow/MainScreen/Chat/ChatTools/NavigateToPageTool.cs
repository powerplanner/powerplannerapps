using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class NavigateToPageTool : LocalTool<NavigateToPageTool.Parameters>
    {
        public override string Name => "navigate_to_page";
        public override string Description => "Navigates to the specified page.";
        private MainScreenViewModel _mainScreenViewModel;

        public class Parameters
        {
            [JsonPropertyName("page")]
            [Description("The page to navigate to.")]
            public required ChatPageType Page { get; set; }
        }

        public NavigateToPageTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
        }

        public override Task InvokeAsync(Parameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
