using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vx.Views
{
    public interface IVxView
    {
        int GridRow { set; }

        int GridColumn { set; }

        VxHorizontalAlignment HorizontalAlignment { set; }

        VxVerticalAlignment VerticalAlignment { set; }
    }

    public class VxView : IVxView
    {
        internal Dictionary<string, object> AttachedProperties = new Dictionary<string, object>();

        internal Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        protected void SetProperty(object value, [CallerMemberName]string propertyName = null)
        {
            if (value is VxView[] views)
            {
                Properties[propertyName] = views.Where(i => i != null).ToArray();
                return;
            }

            Properties[propertyName] = value;
        }

        protected T GetProperty<T>([CallerMemberName]string propertyName = null)
        {
            return (T)Properties[propertyName];
        }

        internal VxNativeView NativeView { get; set; }

        public int GridRow
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public int GridColumn
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public VxHorizontalAlignment HorizontalAlignment
        {
            get => GetProperty<VxHorizontalAlignment>();
            set => SetProperty(value);
        }

        public VxVerticalAlignment VerticalAlignment
        {
            get => GetProperty<VxVerticalAlignment>();
            set => SetProperty(value);
        }

        internal void SetAttachedProperty(string propertyName, object value)
        {
            AttachedProperties[propertyName] = value;
        }

        internal bool TryGetAttachedProperty(string propertyName, out object obj)
        {
            return AttachedProperties.TryGetValue(propertyName, out obj);
        }

        internal T GetAttachedProperty<T>(string propertyName, T valueIfNull)
        {
            if (AttachedProperties.TryGetValue(propertyName, out object val))
            {
                return (T)val;
            }

            return valueIfNull;
        }
    }

    public static class VxViewExtensions
    {
        public static T GridRow<T>(this T view, int value) where T : VxView
        {
            view.GridRow = value;
            return view;
        }

        public static T GridColumn<T>(this T view, int value) where T : VxView
        {
            view.GridColumn = value;
            return view;
        }

        public static T HorizontalAlignment<T>(this T view, VxHorizontalAlignment value) where T : VxView
        {
            view.HorizontalAlignment = value;
            return view;
        }

        public static T VerticalAlignment<T>(this T view, VxVerticalAlignment value) where T : VxView
        {
            view.VerticalAlignment = value;
            return view;
        }
    }

    public enum VxHorizontalAlignment
    {
        Left,
        Center,
        Right,
        Stretch
    }

    public enum VxVerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Stretch
    }
}
