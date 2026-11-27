using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class BaseAddOrEditItemPropertyWithValue<T> : BaseAddOrEditItemProperty
    {
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

        public override object GetValue()
        {
            return Value;
        }

        public override Dictionary<string, JsonNode> GetJsonSchemaTypeProperties()
        {
            Dictionary<string, JsonNode> properties = new Dictionary<string, JsonNode>
            {
                { "type", JsonValue.Create(JsonSchemaTypeName) }
            };

            if (typeof(T) == typeof(Guid))
            {
                properties.Add("format", JsonValue.Create("uuid"));
            }
            else if (typeof(T) == typeof(DateOnly))
            {
                properties.Add("format", JsonValue.Create("date"));
            }
            else if (typeof(T) == typeof(TimeOnly))
            {
                properties.Add("pattern", JsonValue.Create("^([01]\\d|2[0-3]):[0-5]\\d$"));
            }
            else if (typeof(T) == typeof(Color))
            {
                properties.Add("pattern", JsonValue.Create("^#[0-9a-fA-F]{6}$"));
            }
            else if (typeof(T).IsEnum)
            {
                var enumType = typeof(T);
                var enumNames = Enum.GetNames(enumType);
                properties.Add("enum", new JsonArray(enumNames.Select(i => JsonValue.Create(i)).ToArray()));
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
                else if (typeof(T) == typeof(TimeOnly))
                {
                    return "string"; // TimeOnly is represented as a string in JSON
                }
                else if (typeof(T) == typeof(Guid))
                {
                    return "string"; // Guids are represented as strings in JSON
                }
                else if (typeof(T) == typeof(Color))
                {
                    return "string";
                }
                else if (typeof(T).IsEnum)
                {
                    return "string";
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T).Name} is not supported for JSON schema generation.");
                }
            }
        }

        public override bool HasSetValue => _hasSetValue;

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
            else if (typeof(T) == typeof(TimeOnly))
            {
                Value = (T)(object)TimeOnly.Parse(value.GetString());
            }
            else if (typeof(T).IsEnum)
            {
                string enumString = value.GetString();
                Value = (T)Enum.Parse(typeof(T), enumString, ignoreCase: true);
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
    }
}
