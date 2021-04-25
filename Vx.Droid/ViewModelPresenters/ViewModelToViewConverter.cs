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
using BareMvvm.Core.ViewModels;
using System.Reflection;
using InterfacesDroid.Views;

namespace InterfacesDroid.ViewModelPresenters
{
    public class ViewModelToViewConverter
    {
        private static Dictionary<Type, Type> ViewModelToViewMappings = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> ViewModelToSplashMappings = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> GenericViewModelToViewMappings = new Dictionary<Type, Type>();

        public static void AddMapping(Type viewModelType, Type viewType)
        {
            ViewModelToViewMappings[viewModelType] = viewType;
        }

        public static void AddSplashMapping(Type viewModelType, Type viewType)
        {
            ViewModelToSplashMappings[viewModelType] = viewType;
        }

        public static void AddGenericMapping(Type viewModelType, Type viewType)
        {
            GenericViewModelToViewMappings[viewModelType] = viewType;
        }

        private static bool TryFindGeneric(Type viewModelType, out Type genericViewType)
        {
            foreach (var pair in GenericViewModelToViewMappings)
            {
                if (pair.Key.IsAssignableFrom(viewModelType))
                {
                    genericViewType = pair.Value;
                    return true;
                }
            }

            genericViewType = null;
            return false;
        }

        public static View GetSplash(ViewGroup root, object viewModel)
        {
            if (viewModel == null)
                return null;

            Type viewType;

            if (ViewModelToSplashMappings.TryGetValue(viewModel.GetType(), out viewType))
            {
                return CreateView(root, viewType);
            }

            return null;
        }

        public static View Convert(ViewGroup root, object value)
        {
            if (value == null)
                return null;

            if (value is BaseViewModel)
            {
                var baseViewModel = value as BaseViewModel;
                var cached = baseViewModel.GetNativeView();
                if (cached is View cachedView)
                {
                    // If there's already an existing cached native view (like for paging scenarios)
                    // then use that
                    return cachedView;
                }
            }

            Type viewType;
            
            View view = null;

            if (ViewModelToViewMappings.TryGetValue(value.GetType(), out viewType))
            {
                view = CreateView(root, viewType);
            }

            else if (TryFindGeneric(value.GetType(), out Type genericViewType))
            {
                view = CreateView(root, genericViewType);
            }

            else if (value is PagedViewModelWithPopups)
            {
                view = new PagedViewModelWithPopupsPresenter(root.Context);
            }

            else if (value is PagedViewModel)
            {
                view = new PagedViewModelPresenter(root.Context);
            }

            else
            {
                throw new NotImplementedException("ViewModel type was unknown: " + value.GetType());
            }

            // And assign the native view to the view model
            if (value is BaseViewModel)
            {
                (value as BaseViewModel).SetNativeView(view);
            }

            // Get the ViewModel property
            var viewModelProperty = view.GetType().GetProperties().FirstOrDefault(p => p.Name.Equals("ViewModel"));
            if (viewModelProperty == null)
            {
                throw new InvalidOperationException("View must have a ViewModel property");
            }

            // And set the property
            viewModelProperty.SetValue(view, value);

            // And return the view
            return view;
        }

        private static View CreateView(ViewGroup root, Type viewType)
        {
            try
            {
                if (viewType.IsSubclassOf(typeof(ViewHostGeneric)))
                    return (View)Activator.CreateInstance(viewType, root);
                else
                    return (View)Activator.CreateInstance(viewType, root.Context);
            }
            catch (TargetInvocationException ex)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new TargetInvocationException("View likely didn't have the correct constructor.", ex);
            }
        }
    }
}