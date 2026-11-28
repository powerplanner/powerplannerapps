using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal class SemesterParams
    {
        [Description("Required: The id of the semester to retrieve items from.")]
        [JsonPropertyName("semesterId")]
        public required Guid SemesterIdentifier { get; set; }
    }
}
