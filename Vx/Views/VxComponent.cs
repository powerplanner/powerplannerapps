using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace Vx.Views
{
    public abstract class VxComponent : View
    {
        public event EventHandler Rendered;

        protected virtual View Render()
        {
            return null;
        }

        public INativeComponent NativeComponent { get; set; }

        public void InitializeForDisplay(INativeComponent nativeComponent)
        {
            NativeComponent = nativeComponent;
            InitializeForDisplay();
        }

        private bool _hasInitializedForDisplay;
        private void InitializeForDisplay()
        {
            //if (IsDependentOnBindingContext && BindingContext == null)
            //{
            //    return;
            //}

            if (_hasInitializedForDisplay)
            {
                return;
            }

            _hasInitializedForDisplay = true;

            NativeComponent.ComponentSizeChanged += new WeakEventHandler<SizeF>(NativeComponent_ComponentSizeChanged).Handler;

            Initialize();

            //if (IsRootComponent && _additionalComponentsToInitialize != null)
            //{
            //    foreach (var c in _additionalComponentsToInitialize)
            //    {
            //        c.InitializeForDisplay();
            //    }

            //    _additionalComponentsToInitialize = null;
            //}

            //var renderedContentContainer = PrepRenderedContentContainer();
            //if (renderedContentContainer != null)
            //{
            //    base.Content = renderedContentContainer;
            //}

            SubscribeToStates();
            SubscribeToProperties();

            RenderActual();

            EnableHotReload();
        }

        private void NativeComponent_ComponentSizeChanged(object sender, SizeF e)
        {
            OnSizeChanged(e);
        }

        private async void EnableHotReload()
        {
#if DEBUG
            // Enable hot reload by refreshing every second since we can't subscribe to MetadataUpdateHandler yet
            if (System.Diagnostics.Debugger.IsAttached && VxPlatform.Current == Platform.Uwp)
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(1000);

                        MarkDirty();
                    }
                }
                catch { }
            }
#endif
        }

        /// <summary>
        /// This is called only once, before Render is called, and only called when the component is actually going to be displayed (not called for the virtual components that will be discarded). No need to call base.Initialize().
        /// </summary>
        protected virtual void Initialize()
        {
            // Nothing
        }

        private void SubscribeToStates()
        {
            foreach (var state in AllStates())
            {
                state.ValueChanged += State_ValueChanged;
            }
        }

        private IEnumerable<VxState> AllStates()
        {
            var stateType = typeof(VxState);
            foreach (var prop in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(i => i.CanRead && stateType.IsAssignableFrom(i.PropertyType)))
            {
                var state = prop.GetValue(this) as VxState;
                if (state == null)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new NullReferenceException("VxState was null");
                }
                yield return state;
            }
            foreach (var prop in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(i => stateType.IsAssignableFrom(i.FieldType)))
            {
                var state = prop.GetValue(this) as VxState;
                if (state == null)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new NullReferenceException("VxState was null");
                }
                yield return state;
            }
        }

        private void UnsubscribeFromStates()
        {
            foreach (var state in AllStates())
            {
                state.ValueChanged -= State_ValueChanged;
            }
        }

        private void State_ValueChanged(object sender, EventArgs e)
        {
            MarkDirty();
        }

        private PropertyChangedEventHandler _propertyValuePropertyChangedHandler;
        private static Type _iNotifyPropertyChangedType = typeof(INotifyPropertyChanged);

        private void SubscribeToProperties()
        {
            _propertyValuePropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(PropertyValue_PropertyChanged).Handler;

            foreach (var propVal in AllSubscribeablePropertyValues())
            {
                SubscribeToPropertyValue(propVal);
            }
        }

        private void UnsubscribeFromProperties()
        {
            if (_propertyValuePropertyChangedHandler == null)
            {
                return;
            }

            foreach (var propVal in AllSubscribeablePropertyValues())
            {
                UnsubscribeToPropertyValue(propVal);
            }
        }

        private IEnumerable<INotifyPropertyChanged> AllSubscribeablePropertyValues()
        {
            var seenProps = new HashSet<string>();

            foreach (var prop in this.GetType().GetProperties().Where(i => i.CanWrite && i.CanRead && _iNotifyPropertyChangedType.IsAssignableFrom(i.PropertyType) && i.GetCustomAttribute<VxSubscribeAttribute>() != null))
            {
                // Avoid subscribing to properties overridden by "new" keyword
                if (seenProps.Add(prop.Name))
                {
                    var propVal = prop.GetValue(this) as INotifyPropertyChanged;
                    if (propVal != null)
                    {
                        yield return propVal;
                    }
                }
            }
        }

        private void SubscribeToPropertyValue(INotifyPropertyChanged propertyValue)
        {
            propertyValue.PropertyChanged += _propertyValuePropertyChangedHandler;
        }

        private void UnsubscribeToPropertyValue(INotifyPropertyChanged propertyValue)
        {
            propertyValue.PropertyChanged -= _propertyValuePropertyChangedHandler;
        }

        private void PropertyValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MarkDirty();
        }

        private bool _dirty = true;

        /// <summary>
        /// Marks this component for re-render on next UI cycle
        /// </summary>
        protected void MarkDirty()
        {
            lock (this)
            {
                if (_dirty)
                {
                    return;
                }

                _dirty = true;
            }

            PortableDispatcher.GetCurrentDispatcher().Run(RenderActual);
        }

        internal void MarkInternalComponentDirty()
        {
            MarkDirty();
        }

        private void RenderActual()
        {
            var now = DateTime.Now;

            View newView = Render();
            View oldView = RenderedContent;

            try
            {
                if (oldView == null || newView == null || oldView.GetType() != newView.GetType())
                {
                    RenderedContent = newView;
                    NativeComponent.ChangeView(newView);
                }

                else
                {
                    // Transfer over the properties
                    oldView.NativeView.Apply(newView);

#if DEBUG
                    if (newView.NativeView == null)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#endif
                }
            }

            catch (ObjectDisposedException ex)
            {
                if (VxPlatform.Current == Platform.Android)
                {
                    // Only Android should be throwing this and should be needing to detach
                    Detach();
                }
                else
                {
                    throw ex;
                }
            }

            lock (this)
            {
                _dirty = false;
            }

            Rendered?.Invoke(this, null);
            //LastMillisecondsToRender = (DateTime.Now - now).Milliseconds;
        }

        protected virtual void Detach()
        {
            UnsubscribeFromStates();
            UnsubscribeFromProperties();
        }

        private View RenderedContent { get; set; }

        private Dictionary<string, object> _states = new Dictionary<string, object>();

        protected T GetState<T>(T defaultValue = default(T), [CallerMemberName]string stateName = null)
        {
            if (_states.TryGetValue(stateName, out object stateValue) && stateValue is T)
            {
                return (T)stateValue;
            }
            else
            {
                return defaultValue;
            }
        }

        protected void SetState<T>(T value, [CallerMemberName]string stateName = null)
        {
            bool changed = false;

            if (_states.TryGetValue(stateName, out object stateValue))
            {
                changed = !object.Equals(value, stateValue);
            }
            else
            {
                changed = true;
            }

            _states[stateName] = value;

            if (changed)
            {
                MarkDirty();
            }
        }

        public SizeF Size => NativeComponent.ComponentSize;

        /// <summary>
        /// Components can override this to create adaptive UI, choosing to mark dirty at different sizes
        /// </summary>
        /// <param name="size"></param>
        protected virtual void OnSizeChanged(SizeF size)
        {
            // Nothing here
        }
    }
}
