using System;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents
{
    public enum AiChangeOperation { Add, Edit, Delete }

    public enum AiItemType { Task, Event, Holiday }

    public class AiProposedChange
    {
        public AiChangeOperation Operation { get; set; }
        public bool IsSelected { get; set; } = true;

        // For Edit/Delete: the GUID of the existing item (mapped from int ID by AiService)
        public Guid? ExistingItemId { get; set; }

        public string Name { get; set; }
        public AiItemType? Type { get; set; }
        public DateOnly? Date { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Time { get; set; }       // "BeforeClass", "StartOfClass", "DuringClass", "EndOfClass", "AllDay", or "HH:mm"
        public string EndTime { get; set; }     // For events with custom time
        public double? PercentComplete { get; set; }
        public string Details { get; set; }
        public Guid? ClassId { get; set; }      // GUID of the class (mapped from int ID by AiService)
    }
}
