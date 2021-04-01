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

        VxThickness Margin { set; }

        float LinearLayoutWeight { set; }
    }

    public class VxView : IVxView
    {
        internal Dictionary<string, object> AttachedProperties = new Dictionary<string, object>();

        internal Dictionary<string, object> Properties { get; } = new Dictionary<string, object>()
        {
            { nameof(HorizontalAlignment), VxHorizontalAlignment.Stretch },
            { nameof(Margin), new VxThickness(0) }
        };

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
            if (Properties.TryGetValue(propertyName, out object val))
            {
                return (T)val;
            }

            return default(T);
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

        public float LinearLayoutWeight
        {
            get => GetProperty<float>();
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

        public VxThickness Margin
        {
            get => GetProperty<VxThickness>();
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

        public static T LinearLayoutWeight<T>(this T view, float value) where T : VxView
        {
            view.LinearLayoutWeight = value;
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

        public static T Margin<T>(this T view, VxThickness value) where T : VxView
        {
            view.Margin = value;
            return view;
        }

        public static T Margin<T>(this T view, double value) where T : VxView
        {
            view.Margin = new VxThickness(value);
            return view;
        }

        public static T Margin<T>(this T view, double left, double top, double right, double bottom) where T : VxView
        {
            view.Margin = new VxThickness(left, top, right, bottom);
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

    public struct VxThickness
    {
        public VxThickness(double uniformLength)
        {
            Bottom = uniformLength;
            Top = uniformLength;
            Left = uniformLength;
            Right = uniformLength;
        }

        public VxThickness(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VxThickness other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }
        public bool Equals(VxThickness thickness)
        {
            return Bottom == thickness.Bottom
                && Left == thickness.Left
                && Right == thickness.Right
                && Top == thickness.Top;
        }

        public override int GetHashCode()
        {
            return (Bottom + Left + Right + Top).GetHashCode();
        }

        public override string ToString()
        {
            return $"{Left}, {Top}, {Right}, {Bottom}";
        }

        public static bool operator ==(VxThickness t1, VxThickness t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(VxThickness t1, VxThickness t2)
        {
            return !t1.Equals(t2);
        }
    }
}
