using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.Binding;
using BareMvvm.Core.Bindings;
using InterfacesDroid.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public class VxView
        : RelativeLayout, INotifyPropertyChanged
    {
        public BindingHost BindingHost { get; private set; } = new BindingHost();

        public VxView(Context context) : base(context)
        {
            // By default we disable auto fill. Classes inheriting from this (like LoginView) can choose to re-enable auto fill.
            AutofillHelper.DisableForAll(this);
        }

        /// <summary>
        /// Get or set the view to be displayed.
        /// </summary>
        public virtual View View
        {
            get
            {
                if (base.ChildCount == 0)
                {
                    return null;
                }

                return base.GetChildAt(0);
            }
            set
            {
                base.RemoveAllViews();

                if (value != null)
                {
                    base.AddView(value);
                }
            }
        }

        public ScrollView VxVerticalScrollView(int paddingLeft, int paddingTop, int paddingRight, int paddingBottom, params View[] views)
        {
            return new ScrollView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            }
            .VxChildren(
                new LinearLayout(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Orientation = Orientation.Vertical
                }
                .VxPadding(paddingLeft, paddingTop, paddingRight, paddingBottom)
                .VxChildren(views)
            );
        }

        public ScrollView VxVerticalScrollView(int padding, params View[] views)
        {
            return VxVerticalScrollView(padding, padding, padding, padding, views);
        }

        public ScrollView VxVerticalScrollView(params View[] views)
        {
            return VxVerticalScrollView(0, views);
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
                    BindingHost.DataContext = value;

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

        protected VxBinding Binding(string dataContextSourcePropertyName)
        {
            return new VxBinding(BindingHost, dataContextSourcePropertyName);
        }

        protected VxBinding Binding<S, V>(string dataContextSourcePropertyName, Func<S, V> converter)
        {
            return new VxBinding(BindingHost, dataContextSourcePropertyName, s => converter((S)s));
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
            BindingHost.SetBinding(dataContextSourcePropertyName, onValueChanged);
        }

        public void SetBinding<T>(string dataContextSourcePropertyName, Action<T> onValueChanged)
        {
            BindingHost.SetBinding(dataContextSourcePropertyName, onValueChanged);
        }

        private bool _detached;
        protected override void OnDetachedFromWindow()
        {
            // We do NOT call the unregister methods since the view might become attached again (RecyclerView scenarios)...
            // and the applicator's unregister unwires the view listeners and all bindings... we just need to detach from the
            // data context, which is what the Detach method does. If DataContext is set again later, 
            BindingHost.Detach();
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
                    BindingHost.DataContext = DataContext;
                }

                _detached = false;
            }

            base.OnAttachedToWindow();
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
    }
}