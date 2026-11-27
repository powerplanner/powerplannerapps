using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal abstract class LocalTool<T, R> : LocalTool
    {
        public abstract Task<R> InvokeWithResultAsync(T parameters);

        public override async Task<object> InvokeWithResultAsync(IDictionary<string, object> arguments)
        {
            var jsonElements = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

            foreach (var arg in arguments)
            {
                if (arg.Value is JsonElement jsonElement)
                {
                    jsonElements[arg.Key] = jsonElement;
                }
                else
                {
                    throw new InvalidOperationException("Unknown argument type");
                }
            }

            var dictTypeInfo = LocalToolJsonContext.Default.DictionaryStringJsonElement;
            JsonElement argsElement = JsonSerializer.SerializeToElement(jsonElements, dictTypeInfo);

            var typeInfo = (JsonTypeInfo<T>)LocalToolJsonContext.Default.GetTypeInfo(typeof(T));
            var parsedParams = JsonSerializer.Deserialize(argsElement, typeInfo);

            var returnValue = await InvokeWithResultAsync(parsedParams);
            if (returnValue != null)
            {
                var returnTypeInfo = (JsonTypeInfo<R>)LocalToolJsonContext.Default.GetTypeInfo(typeof(R));
                return JsonSerializer.SerializeToElement(returnValue, returnTypeInfo);
            }
            else
            {
                return default;
            }
        }

        public override JsonElement ParametersSchema
        {
            get
            {
                var context = new LocalToolJsonContext(new JsonSerializerOptions()
                {
                    RespectNullableAnnotations = true
                });
                var options = context.Options;

                return AIJsonUtilities.CreateJsonSchema(typeof(T), serializerOptions: options);
            }
        }

        public override JsonElement? ReturnSchema
        {
            get
            {
                var context = new LocalToolJsonContext(new JsonSerializerOptions()
                {
                    RespectNullableAnnotations = true
                });
                var options = context.Options;

                return AIJsonUtilities.CreateJsonSchema(typeof(R), serializerOptions: options);
            }
        }
    }

    /// <summary>
    /// Remember to register the tool's parameter type in <see cref="LocalToolJsonContext"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class LocalTool<T> : LocalTool<T, object>
    {
        public override JsonElement? ReturnSchema => null;
        public abstract Task InvokeAsync(T parameters);

        public override async Task<object> InvokeWithResultAsync(T parameters)
        {
            await InvokeAsync(parameters);
            return null;
        }
    }

    internal abstract class LocalTool
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract JsonElement ParametersSchema { get; }
        public abstract JsonElement? ReturnSchema { get; }
        public abstract Task<object> InvokeWithResultAsync(IDictionary<string, object> arguments);

        public ChatTurnTool ToChatTurnTool()
        {
            var context = new LocalToolJsonContext(new JsonSerializerOptions()
            {
                RespectNullableAnnotations = true
            });
            var options = context.Options;

            return new ChatTurnTool
            {
                Name = Name,
                Description = Description,
                ParametersSchema = ParametersSchema,
                ReturnSchema = ReturnSchema
            };
        }
    }
}
