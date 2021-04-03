using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public abstract class VxView
    {
        internal Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        internal void SetProperty(object value, [CallerMemberName] string propertyName = null)
        {
            if (value is VxView[] views)
            {
                Properties[propertyName] = views.Where(i => i != null).ToArray();
                return;
            }

            Properties[propertyName] = value;
        }

        internal View NativeView { get; set; }

        internal View CreateView()
        {
            NativeView = CreateNativeView();
            return NativeView;
        }

        protected abstract View CreateNativeView();

        internal void ApplyDifferentView(VxView view)
        {

        }
    }

    public class VxView<V> : VxView where V : View
    {
        protected override View CreateNativeView()
        {
            var view = Activator.CreateInstance<V>();

            foreach (var prop in Properties)
            {
                SetProperty(prop.Key, prop.Value);
            }

            return view;
        }

        protected void SetProperty(string propName, object value)
        {
            typeof(V).GetProperty(propName).SetValue(this, value);
        }
    }

    public static class VxViewExtensions
    {
        public static T Margin<T>(T view, Thickness value) where T : VxView
        {
            view.SetProperty(value);
            return view;
        }
    }
}
