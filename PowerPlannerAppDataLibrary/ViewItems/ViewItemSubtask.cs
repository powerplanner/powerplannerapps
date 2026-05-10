using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemSubtask
    {
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public ViewItemSubtask Clone()
        {
            return new ViewItemSubtask
            {
                Name = Name,
                IsComplete = IsComplete
            };
        }

        public static bool ExtractSubtasksFromDetails(string details, out string cleanedDetails, out ViewItemSubtask[] subtasks)
        {
            // Handle extracting subtasks from details before passing to base method
            if (details != null)
            {
                string[] lines = details.ReplaceLineEndings().Split(Environment.NewLine);
                int countOfSubtasks = lines.Count(i => i.StartsWith("- [ ] ") || i.StartsWith("- [x] ", StringComparison.CurrentCultureIgnoreCase));
                if (countOfSubtasks > 0)
                {
                    subtasks = new ViewItemSubtask[countOfSubtasks];
                    string[] cleanedLines = new string[lines.Length - countOfSubtasks];

                    int subtaskIndex = 0;
                    int cleanedLinesIndex = 0;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("- [ ] "))
                        {
                            subtasks[subtaskIndex] = new ViewItemSubtask
                            {
                                Name = line.Substring(6).Trim()
                            };
                            subtaskIndex++;
                        }
                        else if (line.StartsWith("- [x] ", StringComparison.CurrentCultureIgnoreCase))
                        {
                            subtasks[subtaskIndex] = new ViewItemSubtask
                            {
                                Name = line.Substring(6).Trim(),
                                IsComplete = true
                            };
                            subtaskIndex++;
                        }
                        else
                        {
                            cleanedLines[cleanedLinesIndex] = line;
                            cleanedLinesIndex++;
                        }
                    }

                    cleanedDetails = string.Join(Environment.NewLine, cleanedLines);
                    return true;
                }
                else
                {
                    subtasks = Array.Empty<ViewItemSubtask>();
                    cleanedDetails = details;
                    return false;
                }
            }
            else
            {
                subtasks = Array.Empty<ViewItemSubtask>();
                cleanedDetails = details;
                return false;
            }
        }

        public static string ProduceDetailsWithSubtasks(string details, ViewItemSubtask[] subtasks)
        {
            if (subtasks == null || subtasks.Length == 0)
            {
                return details;
            }

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(details))
            {
                sb.Append(details.ReplaceLineEndings());
                sb.Append(Environment.NewLine);
            }
            foreach (var subtask in subtasks)
            {
                string checkbox = subtask.IsComplete ? "- [x] " : "- [ ] ";
                sb.Append(checkbox);
                sb.Append(subtask.Name.ReplaceLineEndings(" "));
                sb.Append(Environment.NewLine);
            }
            return sb.ToString().TrimEnd();
        }
    }
}
