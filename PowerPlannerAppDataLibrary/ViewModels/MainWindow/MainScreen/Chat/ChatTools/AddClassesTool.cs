using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatValidators;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ToolsPortable;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools.AddClassesTool;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    public class AddClassesToolParameters
    {
        [Description("The classes to add.")]
        [NotNull]
        [DisallowNull]
        public required AddClassItem[] Classes { get; set; }

        public class AddClassItem
        {
            [Description("Required: The name of the class.")]
            [NotNull]
            [DisallowNull]
            public required string Name { get; set; }

            [Description("Required: The color of the class, in hex string format, ex \"#FF0000\" for red. Use darker colors, since white text will be displayed on top of the class color.")]
            [NotNull]
            [DisallowNull]
            public required string Color { get; set; }

            [Description("Optional details (free-form user-visible text) for the class.")]
            public string Details { get; set; }

            [Description("Optional: Credits the class is worth, or leave null if no credits assigned.")]
            public double? Credits { get; set; }

            [Description("Optional start date of the class, if it starts later than the semester itself.")]
            public DateOnly? StartDate { get; set; }

            [Description("Optional end date of the class, if it ends earlier than the semester itself.")]
            public DateOnly? EndDate { get; set; }
        }
    }

    internal class AddClassesTool : LocalTool<AddClassesToolParameters, List<ChatClassItem>>
    {
        public override string Name => "add_classes";
        public override string Description => "Adds classes to the currently open semester, and returns the added classes.";
        private MainScreenViewModel _mainScreenViewModel;

        public AddClassesTool(MainScreenViewModel mainScreenViewModel)
        {
            _mainScreenViewModel = mainScreenViewModel;
        }

        public override async Task<List<ChatClassItem>> InvokeWithResultAsync(AddClassesToolParameters parameters)
        {
            if (parameters.Classes == null || parameters.Classes.Length == 0)
            {
                throw new ArgumentException("No classes provided.");
            }

            var newIds = await PowerPlannerApp.Current.NewGuidsForCurrentAccountAsync(parameters.Classes.Length * 2);
            Stack<Guid> newIdsStack = new(newIds);


            DataChanges changes = new DataChanges();

            foreach (var c in parameters.Classes)
            {
                ClassValidator.ValidateName(c.Name);

                var classChanges = AccountDataStore.GenerateNewDefaultClass(
                    account: _mainScreenViewModel.CurrentAccount,
                    semesterId: _mainScreenViewModel.CurrentSemesterId,
                    newClassId: newIdsStack.Pop(),
                    name: c.Name.Trim(),
                    rawColor: ClassValidator.GetColor(c.Color),
                    newWeightId: newIdsStack.Pop());

                var newClass = classChanges[0] as DataItemClass;
                if (c.Credits != null)
                {
                    newClass.Credits = c.Credits.Value;
                }
                if (!string.IsNullOrWhiteSpace(c.Details))
                {
                    newClass.Details = c.Details.Trim();
                }
                if (c.StartDate != null)
                {
                    newClass.StartDate = DateTime.SpecifyKind(c.StartDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    if (!SqlDate.IsValid(newClass.StartDate))
                    {
                        throw new ArgumentException($"Start date {c.StartDate} is not valid for SQL Server.");
                    }
                }

                if (c.EndDate != null)
                {
                    newClass.EndDate = DateTime.SpecifyKind(c.EndDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    if (!SqlDate.IsValid(newClass.EndDate))
                    {
                        throw new ArgumentException($"End date {c.EndDate} is not valid for SQL Server.");
                    }
                }

                foreach (var change in classChanges)
                {
                    changes.Add(change);
                }
            }

            await PowerPlannerApp.Current.SaveChanges(changes);

            TelemetryExtension.Current?.TrackEvent("AIAddClassesTool_Success", new Dictionary<string, string> {
                { "Classes", parameters.Classes.Length.ToString() }
            });

            return _mainScreenViewModel.Classes.Where(i => newIds.Contains(i.Identifier)).Select(i => new ChatClassItem(i)).ToList();
        }
    }
}
