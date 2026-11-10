using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemChecklistItem
    {
        public string Name { get; set; } = "";
        public bool IsComplete { get; set; }

        public ViewItemChecklistItem Clone()
        {
            return new ViewItemChecklistItem
            {
                Name = Name,
                IsComplete = IsComplete
            };
        }

        private const string PRIORITY_HIGH = "❗ ";
        private const string PRIORITY_LOW = "⬇️ ";

        public static void ExtractChecklistFromDetails(string details, out string cleanedDetails, out ViewItemChecklistItem[] checklist, out PowerPlannerSending.Priority priority)
        {
            // Handle extracting checklist from details before passing to base method
            if (details != null)
            {
                var trimmedStart = details.TrimStart();
                if (trimmedStart.StartsWith(PRIORITY_HIGH))
                {
                    priority = PowerPlannerSending.Priority.High;
                    details = trimmedStart.Substring(PRIORITY_HIGH.Length);
                }
                else if (trimmedStart.StartsWith(PRIORITY_LOW))
                {
                    priority = PowerPlannerSending.Priority.Low;
                    details = trimmedStart.Substring(PRIORITY_LOW.Length);
                }
                else
                {
                    priority = PowerPlannerSending.Priority.Normal;
                }

                string[] lines = details.ReplaceLineEndings().Split(Environment.NewLine);
                int countOfItems = lines.Count(i => i.StartsWith("- [ ] ") || i.StartsWith("- [x] ", StringComparison.CurrentCultureIgnoreCase));
                if (countOfItems > 0)
                {
                    checklist = new ViewItemChecklistItem[countOfItems];
                    string[] cleanedLines = new string[lines.Length - countOfItems];

                    int itemIndex = 0;
                    int cleanedLinesIndex = 0;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("- [ ] "))
                        {
                            checklist[itemIndex] = new ViewItemChecklistItem
                            {
                                Name = line.Substring(6).Trim()
                            };
                            itemIndex++;
                        }
                        else if (line.StartsWith("- [x] ", StringComparison.CurrentCultureIgnoreCase))
                        {
                            checklist[itemIndex] = new ViewItemChecklistItem
                            {
                                Name = line.Substring(6).Trim(),
                                IsComplete = true
                            };
                            itemIndex++;
                        }
                        else
                        {
                            cleanedLines[cleanedLinesIndex] = line;
                            cleanedLinesIndex++;
                        }
                    }

                    cleanedDetails = string.Join(Environment.NewLine, cleanedLines).Trim();
                    return;
                }
                else
                {
                    checklist = Array.Empty<ViewItemChecklistItem>();
                    cleanedDetails = details.Trim();
                    return;
                }
            }
            else
            {
                checklist = Array.Empty<ViewItemChecklistItem>();
                cleanedDetails = details;
                priority = PowerPlannerSending.Priority.Normal;
                return;
            }
        }

        public static string ProduceDetailsWithChecklist(string details, ViewItemChecklistItem[] checklist, PowerPlannerSending.Priority priority)
        {
            if (checklist == null || checklist.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Count() == 0)
            {
                if (priority != PowerPlannerSending.Priority.Normal)
                {
                    string priorityPrefix = priority == PowerPlannerSending.Priority.High ? PRIORITY_HIGH : PRIORITY_LOW;
                    return priorityPrefix + details;
                }
                else
                {
                    return details;
                }
            }

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(details))
            {
                sb.Append(details.ReplaceLineEndings());
                sb.Append(Environment.NewLine);
            }
            foreach (var item in checklist.Where(i => !string.IsNullOrWhiteSpace(i.Name)))
            {
                string checkbox = item.IsComplete ? "- [x] " : "- [ ] ";
                sb.Append(checkbox);
                sb.Append(item.Name.ReplaceLineEndings(" "));
                sb.Append(Environment.NewLine);
            }

            if (priority != PowerPlannerSending.Priority.Normal)
            {
                sb.Insert(0, priority == PowerPlannerSending.Priority.High ? PRIORITY_HIGH : PRIORITY_LOW);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
