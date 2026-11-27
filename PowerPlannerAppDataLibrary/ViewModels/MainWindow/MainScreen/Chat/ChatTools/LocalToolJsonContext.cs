using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    [JsonSourceGenerationOptions(
            PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            UseStringEnumConverter = true)]
    [JsonSerializable(typeof(NavigateToPageToolParameters))]
    [JsonSerializable(typeof(Dictionary<string, JsonElement>))]
    [JsonSerializable(typeof(BlankParams))]
    [JsonSerializable(typeof(List<ChatClassItem>))]
    [JsonSerializable(typeof(List<ChatHolidayItem>))]
    [JsonSerializable(typeof(GetTasksAndEventsToolResponse))]
    [JsonSerializable(typeof(DisplayItemToolParams))]
    internal partial class LocalToolJsonContext : JsonSerializerContext
    {

    }
}
