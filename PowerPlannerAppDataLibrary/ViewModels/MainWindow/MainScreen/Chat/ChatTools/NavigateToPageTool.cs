using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModelState;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    public class NavigateToPageToolParameters
    {
        [JsonPropertyName("page")]
        [Description("The page to show.")]
        public required ChatPageType Page { get; set; }
    }

    internal class NavigateToPageTool : LocalTool<NavigateToPageToolParameters>
    {
        public override string Name => "show_page";
        public override string Description => "Shows the specified page in the app.";
        private MainScreenViewModel _mainScreenViewModel;

        public NavigateToPageTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
            DisplayName = "Navigate to page";
        }

        public override Task InvokeAsync(NavigateToPageToolParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
