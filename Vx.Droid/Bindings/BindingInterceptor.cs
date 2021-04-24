using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.Bindings;
using Google.Android.Material.TextField;
using InterfacesDroid.Views;
using IO.Github.Inflationx.Viewpump;
using ToolsPortable;

namespace InterfacesDroid.Bindings
{
    public class BindingInterceptor : Java.Lang.Object, IInterceptor
    {
        private BindingApplicator _currentBindingApplicator;

        public InflateResult Intercept(IInterceptorChain chain)
        {
            var request = chain.Request();

            if (request.Parent() is InflatedViewWithBinding inflatedViewWithBinding)
            {
                // Note that we never clear the applicator... sometimes the parent is seemingly null even though
                // it's still under the applicator. If we start binding a new host, the first view should assign it correctly.
                // However the problem is when we have a subview that we inflate that's also an InflatedViewWithBinding... when that
                // returns, we're going to have the wrong binding applicator...
                _currentBindingApplicator = inflatedViewWithBinding.BindingApplicator;
            }

            // Calling proceed will sometimes recursively continue down, when we return we need to restore the binding applicator
            var thisCurrentBindingApplicator = _currentBindingApplicator;
            var result = chain.Proceed(request);
            _currentBindingApplicator = thisCurrentBindingApplicator;

            var view = result.View();
            if (view != null)
            {
                // Localize text strings
                // This will stop executing on the first matched type
                bool localized = LocalizeProperty<EditText>(view, (v) => v.Hint, (v, str) => { v.Hint = str; })
                    || LocalizeProperty<TextView>(view, (v) => v.Text, (v, str) => { v.Text = str; })
                    || LocalizeProperty<TextInputLayout>(view, (v) => v.Hint, (v, str) => { v.Hint = str; });

                var attrSet = result.Attrs();

                var bindingStr = attrSet.GetAttributeValue("http://schemas.android.com/apk/res-auto", "Binding");
                if (bindingStr != null)
                {
                    var bindingExpressions = ParseBindingString(view, bindingStr);
                    if (bindingExpressions.Count > 0)
                    {
                        if (_currentBindingApplicator == null)
                        {
#if DEBUG
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
#endif
                        }
                        else
                        {
                            _currentBindingApplicator.ApplyBindings(view, bindingExpressions);
                        }
                    }
                }
            }

            return result;
        }

        private static readonly Regex sourceRegex = new Regex(@"Source=(@?\w+(.\w+)+)", RegexOptions.Compiled);
        private static readonly Regex targetRegex = new Regex(@"Target=(\w+(.\w+)+)", RegexOptions.Compiled);
        private static readonly Regex converterRegex = new Regex(@"Converter=(\w+)", RegexOptions.Compiled);
        private static readonly Regex converterParameterRegex = new Regex(@"ConverterParameter='([^']+)'|ConverterParameter=(\w+)", RegexOptions.Compiled);
        private static readonly Regex modeRegex = new Regex(@"Mode=(\w+)", RegexOptions.Compiled);
        private static readonly Regex changedEventRegex = new Regex(@"ChangedEvent=(\w+)", RegexOptions.Compiled);

        private static List<BindingExpression> ParseBindingString(View view, string bindingString)
        {
            if (!bindingString.StartsWith("{") || !bindingString.EndsWith("}"))
            {
#if DEBUG
                var errorMsg = "The following XML binding operation is not well formatted, it should start "
                    + $"with '{{' and end with '}}:'{System.Environment.NewLine}{bindingString}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
#endif

                return new List<BindingExpression>();
            }

            List<BindingExpression> answer = new List<BindingExpression>();
            string[] bindingStrings = bindingString.Split(';');

            foreach (var bindingText in bindingStrings)
            {
                // Source isn't required, by default source is the data context itself
                string sourceValue = "";
                var sourceValueMatch = sourceRegex.Match(bindingText);
                if (sourceValueMatch.Success)
                {
                    sourceValue = sourceValueMatch.Groups[1].Value;
                }

                var targetValue = targetRegex.Match(bindingText).Groups[1].Value;
                var converterValue = converterRegex.Match(bindingText).Groups[1].Value;

                var converterParameterGroups = converterParameterRegex.Match(bindingText).Groups;
                string converterParameterValue = converterParameterGroups[1].Value;
                if (converterParameterValue.Length == 0)
                {
                    converterParameterValue = converterParameterGroups[2].Value;
                }

                var bindingMode = BindingMode.OneWay;

                var modeRegexMatch = modeRegex.Match(bindingText);

                if (modeRegexMatch.Success)
                {
                    if (!Enum.TryParse(modeRegexMatch.Groups[1].Value, true, out bindingMode))
                    {
#if DEBUG
                        var errorMsg = "The Mode property of the following XML binding operation "
                            + "is not well formatted, it should be \'OneWay\' "
                            + $"or 'TwoWay':{System.Environment.NewLine}{bindingString}";
                        System.Diagnostics.Debug.WriteLine(errorMsg);
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
#endif

                        continue;
                    }
                }

                var viewValueChangedEvent = changedEventRegex.Match(bindingText).Groups[1].Value;

                answer.Add(new BindingExpression
                {
                    View = view,
                    Source = sourceValue,
                    Target = targetValue,
                    Converter = converterValue,
                    ConverterParameter = converterParameterValue,
                    Mode = bindingMode,
                    ViewValueChangedEvent = viewValueChangedEvent,
                });
            }

            return answer;
        }

        private static bool LocalizeProperty<T>(View view, Func<T, string> getOriginalText, Action<T, string> assignLocalizedText)
            where T : class
        {
            T castedView = view as T;
            if (castedView == null)
            {
                return false;
            }

            string originalText = getOriginalText(castedView);

            if (originalText != null && originalText.Length > 2 && originalText[0] == '{' && originalText[originalText.Length - 1] == '}')
            {
                assignLocalizedText(castedView, PortableLocalizedResources.GetString(originalText.Substring(1, originalText.Length - 2)));
            }

            // Returning true indicates that the view was of the specified type, so that the caller can stop checking different types
            return true;
        }
    }
}