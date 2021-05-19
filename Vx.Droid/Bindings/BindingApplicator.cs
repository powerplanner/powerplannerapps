#region File and License Information
/*
<File>
	<License Type="BSD">
		Copyright © 2009 - 2016, Outcoder. All rights reserved.
	
		This file is part of Calcium (http://calciumsdk.net).

		Redistribution and use in source and binary forms, with or without
		modification, are permitted provided that the following conditions are met:
			* Redistributions of source code must retain the above copyright
			  notice, this list of conditions and the following disclaimer.
			* Redistributions in binary form must reproduce the above copyright
			  notice, this list of conditions and the following disclaimer in the
			  documentation and/or other materials provided with the distribution.
			* Neither the name of the <organization> nor the
			  names of its contributors may be used to endorse or promote products
			  derived from this software without specific prior written permission.

		THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
		ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
		DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
		DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
		(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
		LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
		ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
		(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
		SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	</License>
	<Owner Name="Daniel Vaughan" Email="danielvaughan@outcoder.com" />
	<CreationDate>$CreationDate$</CreationDate>
</File>
*/
#endregion

#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.Content;
using Android.Views;
using Exception = System.Exception;
using Android.Widget;
using ToolsPortable;
using System.Reflection;
using BareMvvm.Core.Binding;
using Android.Content.Res;
using AndroidX.Core.View;
using System.Globalization;
using InterfacesDroid.Helpers;

namespace BareMvvm.Core.Bindings
{
    internal class ApplicationContextHolder
    {
        internal static Context Context { get; set; }
    }

    /// <summary>
    /// See http://www.codeproject.com/Articles/1070662/Data-Binding-in-Xamarin-Android for documentation.
    /// </summary>
	public class BindingApplicator
    {
        private static readonly ViewBinderRegistry ViewBinderRegistry = new ViewBinderRegistry();
        public BindingHost BindingHost { get; private set; } = new BindingHost();

        private static readonly List<Assembly> _assembliesThatNeedProcessing = new List<Assembly>()
        {
            // Include this assembly as it has several converters
            Assembly.GetExecutingAssembly()
        };

#if DEBUG
        private static readonly List<Assembly> _processedAssemblies = new List<Assembly>();
#endif

        /// <summary>
        /// Registers the assembly calling this method as an assembly that'll be searched for type converters
        /// </summary>
        public static void RegisterThisAssembly()
        {
            RegisterAssembly(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Registers the specified assembly as a type converter source
        /// </summary>
        /// <param name="assembly"></param>
        public static void RegisterAssembly(Assembly assembly)
        {
#if DEBUG
            // We only do this check in debug, since assuming in release we already would have seen the issue in debug
            if (_processedAssemblies.Contains(assembly))
            {
                Debugger.Break();
                throw new InvalidOperationException("Assembly cannot be registered twice");
            }
#endif

            if (!_assembliesThatNeedProcessing.Contains(assembly))
            {
                _assembliesThatNeedProcessing.Add(assembly);
            }
        }

        private static Dictionary<string, Type> _valueConverterTypes = new Dictionary<string, Type>();

        private static Dictionary<string, Type> GetValueConverterTypes()
        {
            if (_assembliesThatNeedProcessing.Count > 0)
            {
                foreach (var assembly in _assembliesThatNeedProcessing)
                {
                    foreach (var type in TypeUtility.GetTypes<IValueConverter>(assembly))
                    {
#if DEBUG
                        if (_valueConverterTypes.ContainsKey(type.Name))
                        {
                            // Alert about overwriting type
                            Debugger.Break();
                        }
#endif

                        _valueConverterTypes[type.Name] = type;
                    }

#if DEBUG
                    _processedAssemblies.Add(assembly);
#endif
                }

                _assembliesThatNeedProcessing.Clear();
            }

            return _valueConverterTypes;
        }

        private static Type GetValueConverter(string valueConverterName)
        {
            if (GetValueConverterTypes().TryGetValue(valueConverterName, out Type valueConverterType))
            {
                return valueConverterType;
            }
            else
            {
                return null;
            }
        }

        private List<Action> _unbindActions = new List<Action>();

        public void Unregister()
        {
            foreach (var action in _unbindActions)
            {
                try
                {
                    action();
                }
                catch { }
            }

            BindingHost.Unregister();
        }

        public void ApplyBindings(View view, List<BindingExpression> bindingExpressions)
        {
            foreach (var bindingInfo in bindingExpressions)
            {
                IValueConverter valueConverter = null;
                string valueConverterName = bindingInfo.Converter;

                if (!string.IsNullOrWhiteSpace(valueConverterName))
                {
                    var converterType = GetValueConverter(valueConverterName);
                    if (converterType != null)
                    {
                        var constructor = converterType.GetConstructor(Type.EmptyTypes);
                        if (constructor != null)
                        {
                            valueConverter = constructor.Invoke(null) as IValueConverter;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Value converter {valueConverterName} needs "
                                + "an empty constructor to be instanciated.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"There is no converter named {valueConverterName}.");
                    }
                }

                ApplyBinding(bindingInfo, valueConverter);
            }
        }

        private void ApplyBinding(
            BindingExpression bindingExpression,
            IValueConverter converter)
        {
            bool oppositeOneWay = false;

            PropertyInfo targetProperty = null;
            if (bindingExpression.Target == "Strikethrough" && bindingExpression.View is TextView tv)
            {
                targetProperty = typeof(TextViewStrikethroughWrapper).GetProperty(nameof(TextViewStrikethroughWrapper.Strikethrough));
            }
            else if (bindingExpression.Target == "HasFocus")
            {
                oppositeOneWay = true;
            }
            else
            {
                targetProperty = bindingExpression.View.GetType().GetProperty(bindingExpression.Target);
            }

            if (targetProperty == null && !oppositeOneWay)
            {
                string exMessage = "targetProperty on View could not be found. View: " + bindingExpression.View.GetType() + ". Target: " + bindingExpression.Target;

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw new KeyNotFoundException(exMessage);
            }

            // We try localizing, otherwise we bind
            if (!TryHandleLocalizationBinding(bindingExpression, targetProperty, converter))
            {
                BindingRegistration bindingRegistration = null;

                if (oppositeOneWay)
                {
                    bindingRegistration = BindingHost.GetEmptyRegistration(bindingExpression.Source);
                }
                else
                {
                    Action<object> bindingCallback = value =>
                    {
                        try
                        {
                            SetTargetProperty(
                                rawValue: value,
                                view: bindingExpression.View,
                                targetProperty,
                                converter,
                                bindingExpression.ConverterParameter);
                        }
                        catch (Exception ex)
                        {
                        // View is disposed, should unregister
                        if (ex is TargetInvocationException && ex.InnerException is ObjectDisposedException)
                            {
                            // Note that don't need to call unbind action on the two way view binder since view is already disposed
                            bindingRegistration?.Unregister();
                            }
                            else
                            {
                                if (Debugger.IsAttached)
                                {
                                    Debugger.Break();
                                }
                            }
                        }
                    };

                    bindingRegistration = BindingHost.SetBinding(bindingExpression.Source, bindingCallback);
                }

                if (bindingExpression.Mode == BindingMode.TwoWay)
                {
                    if (ViewBinderRegistry.TryGetViewBinder(bindingExpression.View.GetType(), bindingExpression.Target, out IViewBinder binder))
                    {
                        var unbindAction = binder.BindView(bindingExpression, bindingRegistration, converter);
                        if (unbindAction != null)
                        {
                            _unbindActions.Add(unbindAction);
                        }
                    }
                    else
                    {
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }
                }
            }
        }

        private static bool TryHandleLocalizationBinding(BindingExpression bindingExpression, PropertyInfo targetProperty, IValueConverter converter)
        {
            if (bindingExpression.Source.StartsWith("@"))
            {
                string locName = bindingExpression.Source.Substring(1);
                string locValue = PortableLocalizedResources.GetString(locName);

                SetTargetProperty(locValue, bindingExpression.View, targetProperty, converter, bindingExpression.ConverterParameter);

                return true;
            }

            return false;
        }

        public static void SetTargetProperty(object rawValue,
            object view, PropertyInfo targetProperty, IValueConverter converter, string converterParameter)
        {
            try
            {
                if (targetProperty == null)
                    throw new ArgumentNullException(nameof(targetProperty));

                if (view == null)
                {
                    throw new ArgumentNullException(nameof(view));
                }

                // Use the converter
                var sourcePropertyValue = converter == null
                    ? rawValue
                    : converter.Convert(rawValue,
                        targetProperty.PropertyType,
                        converterParameter,
                        CultureInfo.CurrentCulture);

                /* Need some implicit type coercion here. 
                 * Perhaps pull that in from Calciums property binding system. */
                var property = targetProperty;
                if (property.PropertyType == typeof(string)
                    && !(sourcePropertyValue is string)
                    && sourcePropertyValue != null)
                {
                    sourcePropertyValue = sourcePropertyValue.ToString();
                }
                else if (property.PropertyType == typeof(Android.Views.ViewStates))
                {
                    if (!(sourcePropertyValue is Android.Views.ViewStates))
                    {
                        // Implicit visibility converter
                        bool? shouldBeVisible = null;
                        if (sourcePropertyValue is bool boolean)
                        {
                            shouldBeVisible = boolean;
                        }
                        else
                        {
                            // If not null, visible
                            shouldBeVisible = sourcePropertyValue != null;
                        }

                        if (shouldBeVisible != null)
                        {
                            sourcePropertyValue = shouldBeVisible.Value ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone;
                        }
                    }
                }

                if (targetProperty.DeclaringType == typeof(TextViewStrikethroughWrapper))
                {
                    var wrapper = new TextViewStrikethroughWrapper(view as TextView);
                    targetProperty.SetValue(wrapper, sourcePropertyValue);
                }
                else if (view is View androidView && targetProperty.Name == nameof(androidView.BackgroundTintList) && sourcePropertyValue is ColorStateList colorStateList)
                {
                    // Use ViewCompat since this property didn't exist till API 21
                    try
                    {
                        ViewCompat.SetBackgroundTintList(androidView, colorStateList);
                    }
                    catch (Java.Lang.RuntimeException)
                    {
                        // This theoretically shouldn't ever fail, yet it seems to fail sometimes due to a null reference exception
                        // which makes no sense. So I'll just catch it.
                    }
                }
                else if (targetProperty.Name == nameof(View.HasFocus))
                {
                    // Don't do anything, these are only one-way where the viewmodel updates but the view never updates
                }
                else
                {
                    targetProperty.SetValue(view, sourcePropertyValue);
                }
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Setting property error. View: " + view.GetType() + ". Target: " + targetProperty.Name + ". CanWrite: " + targetProperty.CanWrite, ex);
            }
        }

        private class TextViewStrikethroughWrapper
        {
            private TextView _tv;
            public TextViewStrikethroughWrapper(TextView tv)
            {
                _tv = tv;
            }

            public bool Strikethrough
            {
                get => _tv.GetStrikethrough();
                set => _tv.SetStrikethrough(value);
            }
        }
    }
}
#endif