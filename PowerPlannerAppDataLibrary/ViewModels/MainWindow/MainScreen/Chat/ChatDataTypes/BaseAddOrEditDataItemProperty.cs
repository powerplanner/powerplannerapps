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
    internal abstract class BaseAddOrEditDataItemProperty
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public abstract Type Type { get; }
        public required PropertyPresence WhenAdding { get; set; }
        public required PropertyPresence WhenEditing { get; set; }

        /// <summary>
        /// Optionally enforce a max length
        /// </summary>
        public int? MaxLength { get; set; }
        public bool RequireNonDefaultValueIfSet { get; set; }
        public bool RequireNonEmptyStringIfSet { get; set; }
        public bool RequireSingleLineIfSet { get; set; }
        public abstract Dictionary<string, string> GetJsonSchemaTypeProperties();
        public abstract void SetValue(JsonElement value);
        public abstract bool HasSetValue { get; }
        public abstract void ApplyToDataItem(BaseDataItem dataItem);
    }

    internal enum PropertyPresence
    {
        Required,
        Optional,
        NotPresent
    }

    internal class BaseAddOrEditDataItemProperty<D, T> : BaseAddOrEditDataItemProperty where D : BaseDataItem
    {
        override public Type Type => typeof(T);
        public Func<T, bool> Validator { get; set; }
        public Action<D, T> Apply { get; set; }
        private bool _hasSetValue = false;
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                _hasSetValue = true;
            }
        }
        public override bool HasSetValue => _hasSetValue;
        public override Dictionary<string, string> GetJsonSchemaTypeProperties()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "type", JsonSchemaTypeName }
            };

            if (typeof(T) == typeof(Guid))
            {
                properties.Add("format", "uuid");
            }
            else if (typeof(T) == typeof(DateOnly))
            {
                properties.Add("format", "date");
            }
            else if (typeof(T) == typeof(Color))
            {
                properties.Add("pattern", "^#[0-9a-fA-F]{6}$");
            }

            return properties;
        }

        private string JsonSchemaTypeName
        {
            get
            {
                if (typeof(T) == typeof(string))
                {
                    return "string";
                }
                else if (typeof(T) == typeof(int) || typeof(T) == typeof(double) || typeof(T) == typeof(float))
                {
                    return "number";
                }
                else if (typeof(T) == typeof(bool))
                {
                    return "boolean";
                }
                else if (typeof(T) == typeof(DateOnly))
                {
                    return "string"; // DateOnly is represented as a string in JSON
                }
                else if (typeof(T) == typeof(Guid))
                {
                    return "string"; // Guids are represented as strings in JSON
                }
                else if (typeof(T) == typeof(Color))
                {
                    return "string";
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T).Name} is not supported for JSON schema generation.");
                }
            }
        }

        public override void SetValue(JsonElement value)
        {
            if (typeof(T) == typeof(string))
            {
                Value = (T)(object)value.GetString();
                if (MaxLength.HasValue && Value is string strValue && strValue.Length > MaxLength.Value)
                {
                    throw new ArgumentException($"Value for property '{Name}' exceeds maximum length of {MaxLength.Value}.");
                }
                if (RequireNonEmptyStringIfSet && string.IsNullOrWhiteSpace(Value as string))
                {
                    throw new ArgumentException($"Value for property '{Name}' must be a non-empty string when it is being specified.");
                }
                if (RequireSingleLineIfSet && Value is string singleLineValue && (singleLineValue.Contains('\n') || singleLineValue.Contains('\r')))
                {
                    throw new ArgumentException($"Value for property '{Name}' must be a single line string when it is being specified.");
                }
            }
            else if (typeof(T) == typeof(int))
            {
                Value = (T)(object)value.GetInt32();
            }
            else if (typeof(T) == typeof(double))
            {
                Value = (T)(object)value.GetDouble();
            }
            else if (typeof(T) == typeof(bool))
            {
                Value = (T)(object)value.GetBoolean();
            }
            else if (typeof(T) == typeof(DateOnly))
            {
                Value = (T)(object)DateOnly.Parse(value.GetString());
            }
            else if (typeof(T) == typeof(Guid))
            {
                Value = (T)(object)value.GetGuid();
            }
            else if (typeof(T) == typeof(Color))
            {
                string colorString = value.GetString();
                Value = (T)(object)ColorTranslator.FromHtml(colorString);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported for JSON deserialization.");
            }

            if (RequireNonDefaultValueIfSet && (Value == null || Value.Equals(default(T))))
            {
                throw new ArgumentException($"Value for property '{Name}' must be a non-default value when it is being specified.");
            }
        }

        public override void ApplyToDataItem(BaseDataItem dataItem)
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
