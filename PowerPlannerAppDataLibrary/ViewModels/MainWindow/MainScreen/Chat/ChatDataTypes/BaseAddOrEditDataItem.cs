using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal abstract class BaseAddOrEditDataItem<D> : BaseAddOrEditDataItem where D : BaseDataItem, new()
    {
        public class Property<T> : ChatDataTypeProperty<D, T>
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

                if (property.HasSetValue)
                {
                    property.ApplyToDataItem(dataItem);
                }
            }
        }
    }

    internal abstract class BaseAddOrEditDataItem
    {
        /// <summary>
        /// Should be a lowercase name like "classes"
        /// </summary>
        public abstract string DataTypeName { get; }

        public ChatDataTypeProperty[] Properties { get; private set; }

        protected abstract ChatDataTypeProperty[] GetProperties();

        public BaseAddOrEditDataItem()
        {
            Properties = GetProperties();
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
