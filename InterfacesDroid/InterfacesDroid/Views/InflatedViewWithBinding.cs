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
    public class InflatedViewWithBinding
        : RelativeLayout, INotifyPropertyChanged
    {
        internal readonly BindingApplicator BindingApplicator = new BindingApplicator();

        public InflatedViewWithBinding(int resource, ViewGroup root) : base(root.Context)
        {
            // By default we disable auto fill. Classes inheriting from this (like LoginView) can choose to re-enable auto fill.
            AutofillHelper.DisableForAll(this);

            // Issue: Since we place our content in a frame layout, we can't control wrap_content or match_parent from the level below
            var view = CreateView(LayoutInflater.FromContext(root.Context), resource, this);
            base.AddView(view);
        }

        public InflatedViewWithBinding(int resource, Context context, IAttributeSet attrs) : base(context, attrs)
        {
            // Issue: Since we place our content in a frame layout, we can't control wrap_content or match_parent from the level below
            var view = CreateView(LayoutInflater.FromContext(context), resource, this);
            base.AddView(view);
        }

        public InflatedViewWithBinding(int resource, Context context) : base(context)
        {
            // By default we disable auto fill. Classes inheriting from this (like LoginView) can choose to re-enable auto fill.
            AutofillHelper.DisableForAll(this);

            // Issue: Since we place our content in a frame layout, we can't control wrap_content or match_parent from the level below
            var view = CreateView(LayoutInflater.FromContext(context), resource, this);
            base.AddView(view);
        }

        private View _viewForBinding;
        protected virtual View CreateView(LayoutInflater inflater, int resourceId, ViewGroup root)
        {
            try
            {
                _viewForBinding = inflater.Inflate(resourceId, root, false); // Setting this to false but including the root ensures that the resource's root layout properties will be respected
                return _viewForBinding;
            }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            catch (Exception ex)
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

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

                object oldValue = _dataContext;

                _dataContext = value;

                try
                {
                    BindingApplicator.BindingHost.DataContext = value;

                    OnDataContextChanged(oldValue, value);
                }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                catch (Exception ex)
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        protected virtual void OnDataContextChanged(object oldValue, object newValue)
        {
            // Nothing
        }

        public void SetBinding(string dataContextSourcePropertyName, object target, string targetPropertyName)
        {
            SetBinding(dataContextSourcePropertyName, delegate
            {
                try
                {
                    var targetProp = target.GetType().GetProperty(targetPropertyName);

                    object rawValue = DataContext.GetType().GetProperty(dataContextSourcePropertyName).GetValue(DataContext);

                    BindingApplicator.SetTargetProperty(
                        rawValue: rawValue,
                        view: target,
                        targetProperty: targetProp,
                        converter: null,
                        converterParameter: null);
                }
#if DEBUG
                catch (Exception ex)
                {
                    Debugger.Break();
                }
#else
                catch { }
#endif
            });
        }

        public void SetBinding(string dataContextSourcePropertyName, Action onValueChanged)
        {
            BindingApplicator.BindingHost.SetBinding(dataContextSourcePropertyName, onValueChanged);
        }

        public void SetBinding<T>(string dataContextSourcePropertyName, Action<T> onValueChanged)
        {
            BindingApplicator.BindingHost.SetBinding(dataContextSourcePropertyName, onValueChanged);
        }

        private bool _detached;
        protected override void OnDetachedFromWindow()
        {
            // We do NOT call the unregister methods since the view might become attached again (RecyclerView scenarios)...
            // and the applicator's unregister unwires the view listeners and all bindings... we just need to detach from the
            // data context, which is what the Detach method does. If DataContext is set again later, 
            BindingApplicator.BindingHost.Detach();
            _detached = true;

            base.OnDetachedFromWindow();
        }

        protected override void OnAttachedToWindow()
        {
            // Ensure data context is set if we detached and then re-attached
            if (_detached)
            {
                if (DataContext != null)
                {
                    // Note that if DataContext is already equal to this, it no-ops
                    BindingApplicator.BindingHost.DataContext = DataContext;
                }

                _detached = false;
            }

            base.OnAttachedToWindow();
        }

        ~InflatedViewWithBinding()
        {
            BindingApplicator.Unregister();
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}