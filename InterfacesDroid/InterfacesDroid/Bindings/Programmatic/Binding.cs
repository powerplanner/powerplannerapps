using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using ToolsPortable;

namespace InterfacesDroid.Bindings.Programmatic
{
    public static class Binding
    {
        /// <summary>
        /// Right now only supports a single-level path. You need to hold onto the <see cref="BindingInstance"/> for as long as you'd like the binding to last.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="sourcePath"></param>
        /// <param name="onValueChanged"></param>
        public static BindingInstance SetBinding<TSource>(this TSource dataContext, string sourcePath, Action<TSource> onValueChanged)
            where TSource : INotifyPropertyChanged
        {
            return BindingInstance.Initialize(dataContext, sourcePath, onValueChanged);
        }
    }

    public class BindingInstance : IDisposable
    {
        private INotifyPropertyChanged _dataContext;
        private PropertyChangedEventHandler _propertyChangedHandler;

        private BindingInstance() { }

        internal static BindingInstance Initialize<TSource>(TSource dataContext, string sourcePath, Action<TSource> onValueChanged)
            where TSource : INotifyPropertyChanged
        {
            var answer = new BindingInstance();

            answer._dataContext = dataContext;
            answer._propertyChangedHandler = (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Binding: " + e.PropertyName);
                if (e.PropertyName.Equals(sourcePath))
                {
                    try
                    {
                        onValueChanged(dataContext);
                    }
                    catch
                    {
                        answer.Dispose();
                    }
                }
            };
            dataContext.PropertyChanged += answer._propertyChangedHandler;

            // Invoke right now
            answer._propertyChangedHandler(dataContext, new PropertyChangedEventArgs(sourcePath));

            return answer;
        }

        public void Dispose()
        {
            _dataContext.PropertyChanged -= _propertyChangedHandler;
        }
    }
}