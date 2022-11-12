using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Vx.Views.DragDrop;

namespace Vx.Views
{
    public interface IView
    {
        Thickness Margin { get; set; }
    }

    public class View : IView
    {
        public NativeView NativeView { get; internal set; }

        private Dictionary<string, object> _attachedProperties = new Dictionary<string, object>();

        public T GetAttachedProperty<T>(string propertyName, T defaultVal = default(T))
        {
            if (_attachedProperties.TryGetValue(propertyName, out object val) && val is T)
            {
                return (T)val;
            }

            return defaultVal;
        }

        public void SetAttachedProperty(string propertyName, object value)
        {
            _attachedProperties[propertyName] = value;
        }

        private Dictionary<string, object> _properties = new Dictionary<string, object>();

#region DragDrop
        /// <summary>
        /// Gets or sets a value that indicates whether the element can be dragged as data in a drag-and-drop operation.
        /// </summary>
        public bool CanDrag { get; set; }

        public Action<DragStartingEventArgs> DragStarting { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether this UIElement can be a drop target for purposes of drag-and-drop operations.
        /// </summary>
        public bool AllowDrop { get; set; }

        /// <summary>
        /// Occurs when the input system reports an underlying drag event with this element as the potential drop target.
        /// </summary>
        public Action<DragEventArgs> DragOver { get; set; }

        /// <summary>
        /// Occurs when an item is dropped.
        /// </summary>
        public Action<DragEventArgs> Drop { get; set; }
        #endregion DragDrop

        /// <summary>
        /// Context menu to display when right clicked.
        /// </summary>
        public Func<ContextMenu> ContextMenu { get; set; }

        public Thickness Margin { get; set; }

        /// <summary>
        /// Value from 0.0 to 1.0 (defaults to 1.0).
        /// </summary>
        public float Opacity { get; set; } = 1;

        public float Width { get; set; } = float.NaN;
        public float Height { get; set; } = float.NaN;

        public Action<View> ViewRef { get; set; }

        public Action Tapped { get; set; }

        /// <summary>
        /// Note that HorizontalAlignment will be ignored and be Stretch when parent is a *horizontal* LinearLayout
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        protected T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out object val) && val is T)
            {
                return (T)val;
            }

            return default(T);
        }

        protected void SetProperty(object value, [CallerMemberName] string propertyName = null)
        {
            _properties[propertyName] = value;
        }

        public void SetNativeView(NativeView nativeView)
        {
            NativeView = nativeView;
            nativeView.Apply(this);
        }

        public NativeView CreateNativeView(View parentView)
        {
            var nativeView = NativeView.CreateNativeView(this);
            nativeView.VxParentView = parentView;
            this.SetNativeView(nativeView);
            return nativeView;
        }

        /// <summary>
        /// Debugging focused method to perform any validation of the view. Only called when in a debug compiled build.
        /// </summary>
        public virtual void Validate()
        {
            // Extending classes can implement
        }
    }
}
