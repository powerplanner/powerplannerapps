using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class NestedObject : BaseAddOrEditItem
    {
        public override string DataTypeName => "NestedObject";

        public required BaseAddOrEditItemProperty[] NestedProperties { get; set; }

        public override BaseDataItem CreateDataItem(AddOrEditMode mode)
        {
            throw new NotImplementedException();
        }

        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return NestedProperties;
        }

        public override BaseAddOrEditItemProperty[] Properties => NestedProperties;
    }

    internal class NestedObjectProperty : BaseAddOrEditItemProperty
    {
        public required NestedObject NestedObject { get; set; }

        private bool _hasSetValue = false;
        public override void SetValue(JsonElement value)
        {
            _hasSetValue = true;

            // Note that nested objects don't support add vs edit modes, so just using add.
            // Nested objects should use the same values for both add and edit modes.
            NestedObject.SetPropertiesAndValidateHasRequiredProperties(value, AddOrEditMode.Add);
        }

        public override object GetValue()
        {
            return NestedObject;
        }

        public override bool HasSetValue => _hasSetValue;

        public override Dictionary<string, JsonNode> GetJsonSchemaTypeProperties()
        {
            var (properties, required) = NestedObject.GeneratePropertiesAndRequiredSchema(AddOrEditMode.Add);

            var answer = new Dictionary<string, JsonNode>
            {
                ["type"] = "object",
                ["properties"] = properties
            };

            if (required != null)
            {
                answer["required"] = required;
            }

            return answer;
        }
    }
}
