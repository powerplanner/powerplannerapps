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
using BareMvvm.Core.Bindings;
using System.Diagnostics;
using Android.Util;
using InterfacesDroid.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using InterfacesDroid.Bindings.Programmatic;
using ToolsPortable;
using BareMvvm.Core.Binding;

namespace InterfacesDroid.Views
{
    public class InflatedViewWithBindingHost : RelativeLayout, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object _dataContext;
        public object DataContext
        {
            get => _dataContext;
            set
            {
                _dataContext = value;
                BindingHost.DataContext = value;
            }
        }

        public readonly BindingHost BindingHost = new BindingHost();

        public InflatedViewWithBindingHost(int resource, ViewGroup root) : base(root.Context)
        {
            // By default we disable auto fill. Classes inheriting from this (like LoginView) can choose to re-enable auto fill.
            AutofillHelper.DisableForAll(this);

            // Issue: Since we place our content in a frame layout, we can't control wrap_content or match_parent from the level below
            var view = CreateView(LayoutInflater.FromContext(root.Context), resource, this);
            base.AddView(view);
        }

        public InflatedViewWithBindingHost(int resource, Context context, IAttributeSet attrs) : base(context, attrs)
        {
            // Issue: Since we place our content in a frame layout, we can't control wrap_content or match_parent from the level below
            var view = CreateView(LayoutInflater.FromContext(context), resource, this);
            base.AddView(view);
        }

        public InflatedViewWithBindingHost(int resource, Context context) : base(context)
        {
            // By default we disable auto fill. Classes inheriting from this (like LoginView) can choose to re-enable auto fill.
            AutofillHelper.DisableForAll(this);

            // Issue: Since we place our content in a frame layout, we can't control wrap_content or match_parent from the level below
            var view = CreateView(LayoutInflater.FromContext(context), resource, this);
            base.AddView(view);
        }

        public InflatedViewWithBindingHost(Context context) : base(context)
        {
            // By default we disable auto fill. Classes inheriting from this (like LoginView) can choose to re-enable auto fill.
            AutofillHelper.DisableForAll(this);
        }

        protected virtual View CreateView(LayoutInflater inflater, int resourceId, ViewGroup root)
        {
            try
            {
                var view = inflater.Inflate(resourceId, root, false); // Setting this to false but including the root ensures that the resource's root layout properties will be respected
                return view;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                    System.Diagnostics.Debug.WriteLine(ex);
                }

                throw;
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnDetachedFromWindow()
        {
            BindingHost.Detach();
            base.OnDetachedFromWindow();
        }

        protected override void OnAttachedToWindow()
        {
            BindingHost.DataContext = DataContext;
            base.OnAttachedToWindow();
        }

        ~InflatedViewWithBindingHost()
        {
            BindingHost.Unregister();
        }
    }
}