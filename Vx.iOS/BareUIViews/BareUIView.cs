using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.ComponentModel;
using ToolsPortable;
using InterfacesiOS.Binding;
using System.Runtime.CompilerServices;

namespace InterfacesiOS.Views
{
#if DEBUG
    internal static class ActiveViews
    {
        private static readonly WeakReferenceList<UIView> ACTIVE_VIEWS = new WeakReferenceList<UIView>();

        static ActiveViews()
        {
            OutputActiveViews();
        }

        public static void Track(UIView view)
        {
            ACTIVE_VIEWS.Add(view);
        }

        private static string _prevResult;
        private static async void OutputActiveViews()
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(1500);

                string result = "";
                int count = 0;
                foreach (var v in ACTIVE_VIEWS)
                {
                    count++;
                }
                result = $"ACTIVE VIEWS ({count})";
                if (_prevResult != result)
                {
                    _prevResult = result;
                    System.Diagnostics.Debug.WriteLine(result);
                }
            }
        }
    }
#endif

    public class BareUIView : UIView, INotifyPropertyChanged
    {
        public BareUIView()
        {
#if DEBUG
            ActiveViews.Track(this);
#endif
        }

        public BareUIView(CoreGraphics.CGRect frame) : base(frame)
        {
#if DEBUG
            ActiveViews.Track(this);
#endif
        }

        public BindingHost BindingHost { get; private set; } = new BindingHost();
        private object _dataContext;

        public event PropertyChangedEventHandler PropertyChanged;

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (value == _dataContext)
                {
                    return;
                }

                _dataContext = value;
                BindingHost.DataContext = value;

                OnDataContextChanged();
            }
        }

        protected virtual void OnDataContextChanged()
        {
            // Nothing
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                    eventHandler(this, new PropertyChangedEventArgs(propertyNames[i]));
            }
        }
    }
}