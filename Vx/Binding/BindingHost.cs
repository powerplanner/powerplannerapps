using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ToolsPortable;
using System.Collections;
using BareMvvm.Core;
using System.Diagnostics;
using System.Reflection;

namespace BareMvvm.Core.Binding
{
    public class BindingHost
    {
        private PropertyChangedEventHandler _dataContextPropertyChangedHandler;
        private object _dataContext;
        /// <summary>
        /// The DataContext for binding
        /// </summary>
        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (value == _dataContext)
                {
                    return;
                }

                // Unregister old
                UnregisterDataContextListener();

                _dataContext = value;

                // Register new
                RegisterDataContextListener();

                UpdateAllBindings();
            }
        }

        private void UnregisterDataContextListener()
        {
            if (_dataContext is INotifyPropertyChanged && _dataContextPropertyChangedHandler != null)
            {
                (_dataContext as INotifyPropertyChanged).PropertyChanged -= _dataContextPropertyChangedHandler;
            }
        }

        private void RegisterDataContextListener()
        {
            if (_dataContext is INotifyPropertyChanged notifyPropertyChanged)
            {
                if (_dataContextPropertyChangedHandler == null)
                {
                    _dataContextPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(DataContext_PropertyChanged).Handler;
                }
                notifyPropertyChanged.PropertyChanged += _dataContextPropertyChangedHandler;
            }
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We call ToArray() since a binding action could cause a new binding to be added while we're working
            Action[] existingActions = null;
            if (_bindings.TryGetValue(e.PropertyName, out List<InternalBindingRegistration> bindings))
            {
                if (_bindingRegistrationToSkipOnce != null)
                {
                    existingActions = bindings.Where(i => i != _bindingRegistrationToSkipOnce).Select(i => i.Action).ToArray();
                }
                else
                {
                    existingActions = bindings.Select(i => i.Action).ToArray();
                }
            }

            _subPropertyBindings.TryGetValue(e.PropertyName, out BindingHost existingSubBinding);

            if (existingActions != null)
            {
                ExecuteActions(existingActions);
            }

            if (existingSubBinding != null)
            {
                existingSubBinding.DataContext = GetValueFromSingleProperty(e.PropertyName);
            }
        }

        private void UpdateAllBindings()
        {
            // Grab these as a copied array so that any of the sub actions don't modify them as we iterate
            Action[] existingActions = _bindings.Values.SelectMany(i => i).Select(i => i.Action).ToArray();
            KeyValuePair<string, BindingHost>[] existingSubBindings = _subPropertyBindings.ToArray();

            ExecuteActions(existingActions);

            foreach (var subBinding in existingSubBindings)
            {
                subBinding.Value.DataContext = GetValueFromSingleProperty(subBinding.Key);
            }
        }

        private void ExecuteActions(Action[] actions)
        {
            foreach (var a in actions)
            {
                try
                {
                    a.Invoke();
                }
                catch
#if DEBUG
                (Exception ex)
#endif
                {
#if DEBUG
                    Debug.WriteLine("Failed to update binding: " + ex);
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
#endif
                }
            }
        }

        internal class InternalBindingRegistration
        {
            public Action Action { get; private set; }

            public InternalBindingRegistration(Action action)
            {
                Action = action;
            }
        }

        private Dictionary<string, List<InternalBindingRegistration>> _bindings = new Dictionary<string, List<InternalBindingRegistration>>();
        private Dictionary<string, BindingHost> _subPropertyBindings = new Dictionary<string, BindingHost>();

        internal void UnregisterBinding(BindingRegistration registration)
        {
            if (registration.InternalRegistration != null)
            {
                if (_bindings.TryGetValue(registration.PropertyName, out List<InternalBindingRegistration> internalRegistrations))
                {
                    internalRegistrations.Remove(registration.InternalRegistration);
                    if (internalRegistrations.Count == 0)
                    {
                        _bindings.Remove(registration.PropertyName);
                    }
                }
            }
            else
            {
                if (_subPropertyBindings.TryGetValue(registration.PropertyName, out BindingHost subBindingHost))
                {
                    subBindingHost.UnregisterBinding(registration.SubRegistration);

                    if (subBindingHost.IsEmpty())
                    {
                        subBindingHost.Unregister();
                        _subPropertyBindings.Remove(registration.PropertyName);
                    }
                }
            }
        }

        private bool IsEmpty()
        {
            return _bindings.Count == 0 && _subPropertyBindings.Count == 0;
        }

        public BindingRegistration SetBinding<T>(string propertyPath, Action<T> action)
        {
            return SetBinding(propertyPath, () =>
            {
                object value = GetValue(propertyPath);
                if (value == null)
                {
                    action(default(T));
                }
                else
                {
                    action((T)value);
                }
            });
        }

        public BindingRegistration SetBinding(string propertyPath, Action<object> action)
        {
            return SetBinding(propertyPath, () =>
            {
                object value = GetValue(propertyPath);
                action(value);
            });
        }

        public BindingRegistration GetEmptyRegistration(string propertyPath)
        {
            return new BindingRegistration(this, null, internalRegistration: null, propertyPath.Split('.'));
        }

        public BindingRegistration SetBinding(string propertyPath, Action action, bool skipInvokingActionImmediately = false)
        {
            return SetBinding(propertyPath.Split('.'), action, skipInvokingActionImmediately);
        }

        private BindingRegistration SetBinding(string[] propertyPaths, Action action, bool skipInvokingActionImmediately, bool topLevel = true)
        {
            string immediatePath = propertyPaths[0];

            if (propertyPaths.Length == 1)
            {
                List<InternalBindingRegistration> storedBindings;
                if (!_bindings.TryGetValue(immediatePath, out storedBindings))
                {
                    storedBindings = new List<InternalBindingRegistration>();
                    _bindings[immediatePath] = storedBindings;
                }

                var internalRegistration = new InternalBindingRegistration(action);
                storedBindings.Add(internalRegistration);

                // We require DataContext to be set here since bindings can be wired before DataContext is set
                if (DataContext != null && !skipInvokingActionImmediately)
                {
                    action();
                }

                return new BindingRegistration(this, immediatePath, internalRegistration, topLevel ? propertyPaths : null);
            }
            else
            {
                BindingHost subBinding;
                if (!_subPropertyBindings.TryGetValue(immediatePath, out subBinding))
                {
                    subBinding = new BindingHost()
                    {
                        DataContext = GetValueFromSingleProperty(propertyPaths[0])
                    };
                    _subPropertyBindings[immediatePath] = subBinding;
                }

                var subRegistration = subBinding.SetBinding(propertyPaths.Skip(1).ToArray(), action, skipInvokingActionImmediately, topLevel: false);

                // For this we need to execute first time even if data context was null (for example binding Class.Name should execute even if Class was null)
                if (DataContext != null && subBinding.DataContext == null && !skipInvokingActionImmediately)
                {
                    action();
                }

                return new BindingRegistration(this, immediatePath, subRegistration, topLevel ? propertyPaths : null);
            }
        }

        private object GetValueFromSingleProperty(string propertyName)
        {
            return DataContext?.GetType().GetProperty(propertyName).GetValue(DataContext);
        }

        private object GetValue(string propertyPath)
        {
            // We support binding to the data context itself
            if (propertyPath.Length == 0)
            {
                return DataContext;
            }

            string[] paths = propertyPath.Split('.');

            object obj = DataContext;
            foreach (var propertyName in paths)
            {
                if (obj == null)
                {
                    return null;
                }

                var prop = obj.GetType().GetProperty(propertyName);
                if (prop == null)
                {
                    var exMessage = $"Failed to find property {propertyName} on object {obj.GetType()} when resolving property path {propertyPath}.";
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    throw new KeyNotFoundException(exMessage);
                }
                obj = prop.GetValue(obj);
            }

            return obj;
        }

        public PropertyInfoAndObject GetProperty(string[] propertyPath)
        {
            string[] paths = propertyPath;

            object obj = DataContext;
            foreach (var propertyName in paths.Take(paths.Length - 1))
            {
                if (obj == null)
                {
                    return null;
                }

                obj = obj.GetType().GetProperty(propertyName).GetValue(obj);
            }

            if (obj == null)
            {
                return null;
            }

            var prop = obj.GetType().GetProperty(paths.Last());
            if (prop == null)
            {
                return null;
            }

            return new PropertyInfoAndObject(obj, prop);
        }

        private InternalBindingRegistration _bindingRegistrationToSkipOnce;

        /// <summary>
        /// Will ensure events aren't triggered when value is set
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="value"></param>
        internal void SetValue(string[] propertyPath, object value, BindingRegistration bindingRegistrationToSkipNotifying, PropertyInfoAndObject preObtainedSourceProperty = null)
        {
            var property = preObtainedSourceProperty ?? GetProperty(propertyPath);
            if (property != null)
            {
                BindingHost bindingHost = null;
                InternalBindingRegistration internalRegistrationToSkip = null;
                if (bindingRegistrationToSkipNotifying != null)
                {
                    internalRegistrationToSkip = bindingRegistrationToSkipNotifying.GetFinalInternalRegistration();
                    bindingHost = FindBindingHost(propertyPath);
                    if (bindingHost != null)
                    {
                        bindingHost._bindingRegistrationToSkipOnce = internalRegistrationToSkip;
                    }
                }

                try
                {
                    property.PropertyInfo.SetValue(property.Object, value);
                }
                finally
                {
                    if (bindingHost != null && bindingHost._bindingRegistrationToSkipOnce == internalRegistrationToSkip)
                    {
                        bindingHost._bindingRegistrationToSkipOnce = null;
                    }
                }
            }
        }

        private BindingHost FindBindingHost(string[] paths)
        {
            if (paths.Length == 1)
            {
                return this;
            }

            if (_subPropertyBindings.TryGetValue(paths[0], out BindingHost subBindingHost))
            {
                return subBindingHost.FindBindingHost(paths.Skip(1).ToArray());
            }
            else
            {
                return null;
            }
        }

        public void SetBindings(string[] propertyPaths, Action action)
        {
            foreach (var p in propertyPaths)
            {
                SetBinding(p, action, skipInvokingActionImmediately: true);
            }

            if (DataContext != null)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Unregisters all handlers and everything and sets DataContext to null.
        /// </summary>
        public void Unregister()
        {
            _bindings.Clear();

            foreach (var subBinding in _subPropertyBindings.Values)
            {
                subBinding.Unregister();
            }

            _subPropertyBindings.Clear();

            Detach();
        }

        /// <summary>
        /// Detaches from DataContext (and sets it to null) but leaves the bindings in place so that if view is added again, when DataContext is set all bindings will be applied.
        /// </summary>
        public void Detach()
        {
            foreach (var subBinding in _subPropertyBindings.Values)
            {
                subBinding.Detach();
            }

            UnregisterDataContextListener();
            _dataContext = null;
        }
    }
}