using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatViewModel;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools
{
    internal abstract class LocalTool<T, R> : LocalTool
    {
        public override Type ParametersType => typeof(T);
        public override Type ReturnType => typeof(R);

        public abstract Task<R> InvokeWithResultAsync(T parameters);

        public override async Task<JsonElement> InvokeWithResultAsync(IDictionary<string, object> arguments)
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
    }

    /// <summary>
    /// Remember to register the tool's parameter type in <see cref="LocalToolJsonContext"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class LocalTool<T> : LocalTool<T, object>
    {
        public override Type ParametersType => typeof(T);

        public override Type ReturnType => null;

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
        public abstract Type ParametersType { get; }
        public abstract Type ReturnType { get; }
        public abstract Task<JsonElement> InvokeWithResultAsync(IDictionary<string, object> arguments);

        public ChatTurnTool ToChatTurnTool()
        {
            return new ChatTurnTool
            {
                Name = Name,
                Description = Description,
                ParametersSchema = AIJsonUtilities.CreateJsonSchema(ParametersType, serializerOptions: (JsonSerializerOptions)LocalToolJsonContext.Default.Options),
                ReturnSchema = ReturnType != null ? AIJsonUtilities.CreateJsonSchema(ReturnType, serializerOptions: (JsonSerializerOptions)LocalToolJsonContext.Default.Options) : null
            };
        }
    }
}
