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

        public static bool ExtractChecklistFromDetails(string details, out string cleanedDetails, out ViewItemChecklistItem[] checklist)
        {
            // Handle extracting checklist from details before passing to base method
            if (details != null)
            {
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

                    cleanedDetails = string.Join(Environment.NewLine, cleanedLines);
                    return true;
                }
                else
                {
                    checklist = Array.Empty<ViewItemChecklistItem>();
                    cleanedDetails = details;
                    return false;
                }
            }
            else
            {
                checklist = Array.Empty<ViewItemChecklistItem>();
                cleanedDetails = details;
                return false;
            }
        }

        public static string ProduceDetailsWithChecklist(string details, ViewItemChecklistItem[] checklist)
        {
            if (checklist == null || checklist.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Count() == 0)
            {
                return details;
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
            return sb.ToString().TrimEnd();
        }
    }
}
