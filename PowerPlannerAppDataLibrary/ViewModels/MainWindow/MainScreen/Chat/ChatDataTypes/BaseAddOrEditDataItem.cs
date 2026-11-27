using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal abstract class BaseAddOrEditDataItem<D> : BaseAddOrEditItem where D : BaseDataItem, new()
    {
        public class NestedProperty : NestedObjectProperty, IPropertyTiedToDataItem
        {
            public Action<D, NestedObject> Apply { get; set; }

            public void ApplyToDataItem(BaseDataItem dataItem)
            {
                if (Apply == null)
                {
                    throw new InvalidOperationException($"Apply action is not set for property '{Name}'.");
                }

                if (dataItem is D typedDataItem)
                {
                    Apply(typedDataItem, GetValue() as NestedObject);
                }
                else
                {
                    throw new InvalidOperationException($"Data item is not of the expected type '{typeof(D).Name}'.");
                }
            }
        }

        public class Property<T> : BaseAddOrEditDataItemProperty<D, T>
        {

        }

        public override BaseDataItem CreateDataItem(AddOrEditMode mode)
        {
            var dataItem = new D();
            PopulateDataItem(dataItem, mode);
            return dataItem;
        }

        public void PopulateDataItem(D dataItem, AddOrEditMode mode)
        {
            foreach (var property in Properties)
            {
                var propertyPresence = mode == AddOrEditMode.Add ? property.WhenAdding : property.WhenEditing;

                if (propertyPresence == PropertyPresence.NotPresent)
                {
                    continue;
                }

                if (propertyPresence == PropertyPresence.Required && !property.HasSetValue)
                {
                    throw new ArgumentException($"Missing required property: {property.Name} on {DataTypeName} item.");
                }

                if (property.HasSetValue && property is IPropertyTiedToDataItem tiedToDataItem)
                {
                    tiedToDataItem.ApplyToDataItem(dataItem);
                }
            }
        }
    }

    internal abstract class BaseAddOrEditItem
    {
        /// <summary>
        /// Should be a lowercase name like "classes"
        /// </summary>
        public abstract string DataTypeName { get; }

        public virtual BaseAddOrEditItemProperty[] Properties { get; private set; }

        protected abstract BaseAddOrEditItemProperty[] GetProperties();

        public BaseAddOrEditItem()
        {
            Properties = GetProperties();
        }

        public Tuple<JsonNode, JsonNode> GeneratePropertiesAndRequiredSchema(AddOrEditMode mode)
        {
            JsonObject dataTypeProperties = new();
            List<string> requiredProperties = new();

            foreach (var p in Properties)
            {
                var propertyPresence = mode == AddOrEditMode.Add ? p.WhenAdding : p.WhenEditing;

                if (propertyPresence == PropertyPresence.NotPresent)
                {
                    continue;
                }

                var propertyObj = new JsonObject
                {
                    ["description"] = (propertyPresence == PropertyPresence.Required ? "Required: " : "Optional: ") + p.Description
                };

                foreach (var typeProperty in p.GetJsonSchemaTypeProperties())
                {
                    propertyObj[typeProperty.Key] = typeProperty.Value;
                }

                dataTypeProperties.Add(p.Name, propertyObj);

                if ((mode == AddOrEditMode.Add && p.WhenAdding == PropertyPresence.Required) ||
                    (mode == AddOrEditMode.Edit && p.WhenEditing == PropertyPresence.Required))
                {
                    requiredProperties.Add(p.Name);
                }
            }

            JsonArray required = null;

            if (requiredProperties.Count > 0)
            {
                required = new JsonArray(requiredProperties.Select(i => JsonValue.Create(i)).ToArray());
            }

            return Tuple.Create<JsonNode, JsonNode>(dataTypeProperties, required);
        }

        public JsonObject GenerateObjectSchema(AddOrEditMode mode)
        {
            var (dataTypeProperties, required) = GeneratePropertiesAndRequiredSchema(mode);

            JsonObject itemsObject = new()
            {
                ["type"] = "object",
                ["properties"] = dataTypeProperties
            };
            if (required != null)
            {
                itemsObject["required"] = required;
            }

            return itemsObject;
        }

        /// <summary>
        /// Also performs validation on the value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetProperty(string name, JsonElement value)
        {
            var property = Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (property != null)
            {
                property.SetValue(value);
            }
        }

        public void SetPropertiesAndValidateHasRequiredProperties(JsonElement value, AddOrEditMode mode)
        {
            if (value.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException($"Expected a JSON object for {DataTypeName} item, but got {value.ValueKind}.");
            }

            foreach (JsonProperty p in value.EnumerateObject())
            {
                this.SetProperty(p.Name, p.Value);
            }

            this.ValidateHasRequiredProperties(mode);
        }

        public void ValidateHasRequiredProperties(AddOrEditMode mode)
        {
            foreach (var p in Properties)
            {
                var propertyPresence = mode == AddOrEditMode.Add ? p.WhenAdding : p.WhenEditing;
                if (propertyPresence == PropertyPresence.Required)
                {
                    if (!p.HasSetValue)
                    {
                        throw new ArgumentException($"Missing required property: {p.Name} on {DataTypeName} item.");
                    }
                }
            }
        }

        public abstract BaseDataItem CreateDataItem(AddOrEditMode mode);
    }

    internal class PropertyRegistration
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyPresence WhenAdding { get; set; }
        public PropertyPresence WhenEditing { get; set; }
    }
}
