using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.Swift;
using System.Text;
using System.Text.Json;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal interface IPropertyTiedToDataItem
    {
        void ApplyToDataItem(BaseDataItem dataItem);
    }

    internal abstract class BaseAddOrEditItemProperty
    {
        public required string Name { get; set; }
        /// <summary>
        /// User-facing name
        /// </summary>
        public string DisplayName { get; set; }
        public required string Description { get; set; }
        public required PropertyPresence WhenAdding { get; set; }
        public required PropertyPresence WhenEditing { get; set; }

        /// <summary>
        /// Optionally enforce a max length
        /// </summary>
        public int? MaxLength { get; set; }
        public bool RequireNonDefaultValueIfSet { get; set; }
        public bool RequireNonEmptyStringIfSet { get; set; }
        public bool RequireSingleLineIfSet { get; set; }
        public abstract Dictionary<string, System.Text.Json.Nodes.JsonNode> GetJsonSchemaTypeProperties();
        public abstract void SetValue(JsonElement value);
        public abstract bool HasSetValue { get; }
        public abstract object GetValue();
    }

    internal enum PropertyPresence
    {
        Required,
        Optional,
        NotPresent
    }

    internal class BaseAddOrEditDataItemProperty<D, T> : BaseAddOrEditItemPropertyWithValue<T>, IPropertyTiedToDataItem where D : BaseDataItem
    {
        public Action<D, T> Apply { get; set; }

        public void ApplyToDataItem(BaseDataItem dataItem)
        {
            if (Apply == null)
            {
                throw new InvalidOperationException($"Apply action is not set for property '{Name}'.");
            }

            if (dataItem is D typedDataItem)
            {
                Apply(typedDataItem, Value);
            }
            else
            {
                throw new InvalidOperationException($"Data item is not of the expected type '{typeof(D).Name}'.");
            }
        }
    }
}
