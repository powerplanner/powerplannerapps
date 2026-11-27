using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    public enum AddOrEditMode
    {
        Add,
        Edit
    }

    internal class AddOrEditItemsTool<TChatDataItem> : LocalTool where TChatDataItem : BaseAddOrEditItem, new()
    {
        private TChatDataItem _dataTypeInstance = new TChatDataItem();
        public string DataTypeName => _dataTypeInstance.DataTypeName;
        public required AddOrEditMode Mode { get; set; }
        public MainScreenViewModel MainScreenViewModel { get; private set; }

        public AddOrEditItemsTool(MainScreenViewModel mainScreenViewModel)
        {
            MainScreenViewModel = mainScreenViewModel;
        }

        public override string Name => Mode == AddOrEditMode.Add ? $"add_{DataTypeName}" : $"edit_{DataTypeName}";
        public override string Description => Mode == AddOrEditMode.Add ? $"Add new {DataTypeName}." : $"Edit existing {DataTypeName}. For each item, you must provide the identifier of the item you want to edit, and you can set the properties you want to update. Only set properties you actually want to change, unset properties will retain their current value.";

        private JsonElement? _parametersSchema;

        public override JsonElement ParametersSchema
        {
            get
            {
                if (_parametersSchema == null)
                {
                    JsonObject itemsObject = _dataTypeInstance.GenerateObjectSchema(Mode);

                    string uppercaseDataTypeName = char.ToUpper(DataTypeName[0]) + DataTypeName.Substring(1);

                    JsonObject schema = new()
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            [uppercaseDataTypeName] = new JsonObject
                            {
                                ["description"] = Mode == AddOrEditMode.Add ? $"The {DataTypeName} to add." : $"The {DataTypeName} to edit.",
                                ["type"] = "array",
                                ["items"] = itemsObject
                            }
                        }
                        // note that we cannot set Required on the top level properties, the OpenAI endpoint throws an error about that.
                    };

                    _parametersSchema = JsonDocument.Parse(schema.ToJsonString()).RootElement;
                }

                return _parametersSchema.Value;
            }
        }

        public override JsonElement? ReturnSchema => null;

        public override async Task<object> InvokeWithResultAsync(IDictionary<string, object> arguments)
        {
            var itemsObj = arguments.FirstOrDefault(i => i.Key.Equals(DataTypeName, StringComparison.OrdinalIgnoreCase)).Value;
            if (itemsObj == null)
            {
                throw new ArgumentException($"Missing required argument: {DataTypeName}");
            }



            if (itemsObj is not JsonElement itemsJsonElement || itemsJsonElement.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException($"Argument {DataTypeName} must be a JSON array.");
            }

            List<TChatDataItem> items = new List<TChatDataItem>();

            var enumerator = itemsJsonElement.EnumerateArray();
            while (enumerator.MoveNext())
            {
                var itemJsonElement = enumerator.Current;
                if (itemJsonElement.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException($"Each item in {DataTypeName} must be a JSON object.");
                }

                var item = new TChatDataItem();

                item.SetPropertiesAndValidateHasRequiredProperties(itemJsonElement, Mode);

                items.Add(item);
            }

            await SaveItemsAsync(items);
            return null;
        }

        protected virtual async Task SaveItemsAsync(List<TChatDataItem> items)
        {
            DataChanges changes = new DataChanges();

            if (Mode == AddOrEditMode.Add)
            {
                Stack<Guid> newIds = new(await PowerPlannerApp.Current.NewGuidsForCurrentAccountAsync(items.Count));

                foreach (var i in items)
                {
                    var dataItem = i.CreateDataItem(Mode);
                    dataItem.Identifier = newIds.Pop();
                    changes.Add(dataItem);
                }
            }
            else
            {
                foreach (var i in items)
                {
                    var dataItem = i.CreateDataItem(Mode);
                    changes.Add(dataItem, onlyEdit: true);
                }
            }

            await MainScreenViewModel.SaveChanges(changes);
        }
    }
}
