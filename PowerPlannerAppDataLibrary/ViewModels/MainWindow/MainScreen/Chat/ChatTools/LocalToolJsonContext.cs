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
    [JsonSerializable(typeof(ClassInputParams))]
    [JsonSerializable(typeof(GradesResponse))]
    [JsonSerializable(typeof(DeleteItemsToolRequest))]
    [JsonSerializable(typeof(GetYearsAndSemestersToolResponse))]
    [JsonSerializable(typeof(GetFullTasksAndEventsToolRequest))]
    [JsonSerializable(typeof(GetFullTasksAndEventsToolResponse))]
    [JsonSerializable(typeof(SemesterParams))]
    internal partial class LocalToolJsonContext : JsonSerializerContext
    {

    }
}
