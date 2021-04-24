using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using ToolsPortable;

namespace Vx.Views
{
    public abstract class VxComponent
    {
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
            var stateType = typeof(VxState);
            foreach (var prop in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => i.CanRead && stateType.IsAssignableFrom(i.PropertyType)))
            {
                var state = prop.GetValue(this) as VxState;
                if (state == null)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new NullReferenceException("VxState was null");
                }
                state.ValueChanged += State_ValueChanged;
            }
            foreach (var prop in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => stateType.IsAssignableFrom(i.FieldType)))
            {
                var state = prop.GetValue(this) as VxState;
                if (state == null)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new NullReferenceException("VxState was null");
                }
                state.ValueChanged += State_ValueChanged;
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

            var seenProps = new HashSet<string>();

            foreach (var prop in this.GetType().GetProperties().Where(i => i.CanWrite && i.CanRead && _iNotifyPropertyChangedType.IsAssignableFrom(i.PropertyType) && i.GetCustomAttribute<VxSubscribeAttribute>() != null))
            {
                // Avoid subscribing to properties overridden by "new" keyword
                if (seenProps.Add(prop.Name))
                {
                    var propVal = prop.GetValue(this) as INotifyPropertyChanged;
                    if (propVal != null)
                    {
                        SubscribeToPropertyValue(propVal);
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

        private void RenderActual()
        {
            lock (this)
            {
                _dirty = false;
            }

            var now = DateTime.Now;

            View newView = Render();
            View oldView = RenderedContent;

            if (oldView == null || oldView.GetType() != newView.GetType())
            {
                RenderedContent = newView;
                NativeComponent.ChangeView(newView);
            }

            else
            {
                // Transfer over the properties
                oldView.NativeView.Apply(newView);
            }

            //LastMillisecondsToRender = (DateTime.Now - now).Milliseconds;
        }

        private View RenderedContent { get; set; }

        protected VxState<T> Bind<T>(string propertyName, object source)
        {
            var bound = VxState<T>.CreateBound(propertyName, source);
            bound.ValueChanged += State_ValueChanged;
            return bound;
        }
    }
}
