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
using InterfacesDroid.Views;

namespace InterfacesDroid.ViewModelPresenters
{
    public class ViewModelToViewConverter
    {
        private static Dictionary<Type, Func<ViewGroup, View>> ViewModelToViewMappings = new Dictionary<Type, Func<ViewGroup, View>>();
        private static Dictionary<Type, Func<ViewGroup, View>> ViewModelToSplashMappings = new Dictionary<Type, Func<ViewGroup, View>>();
        private static Dictionary<Type, Func<ViewGroup, View>> GenericViewModelToViewMappings = new Dictionary<Type, Func<ViewGroup, View>>();

        public static void AddMapping(Type viewModelType, Func<ViewGroup, View> createView)
        {
            ViewModelToViewMappings[viewModelType] = createView;
        }

        public static void AddSplashMapping(Type viewModelType, Func<ViewGroup, View> createView)
        {
            ViewModelToSplashMappings[viewModelType] = createView;
        }

        public static void AddGenericMapping(Type viewModelType, Func<ViewGroup, View> createView)
        {
            GenericViewModelToViewMappings[viewModelType] = createView;
        }

        private static bool TryFindGeneric(Type viewModelType, out Func<ViewGroup, View> createView)
        {
            foreach (var pair in GenericViewModelToViewMappings)
            {
                if (pair.Key.IsAssignableFrom(viewModelType))
                {
                    createView = pair.Value;
                    return true;
                }
            }

            createView = null;
            return false;
        }

        public static View GetSplash(ViewGroup root, object viewModel)
        {
            if (viewModel == null)
                return null;

            if (ViewModelToSplashMappings.TryGetValue(viewModel.GetType(), out Func<ViewGroup, View> createView))
            {
                return createView(root);
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

            View view = null;

            if (ViewModelToViewMappings.TryGetValue(value.GetType(), out Func<ViewGroup, View> createView))
            {
                view = createView(root);
            }

            else if (TryFindGeneric(value.GetType(), out Func<ViewGroup, View> createGenericView))
            {
                view = createGenericView(root);
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

            if (view is not IViewModelHost viewModelHost)
            {
                throw new InvalidOperationException("Mapped view must implement IViewModelHost.");
            }

            viewModelHost.ViewModel = (BaseViewModel)value;

            // And return the view
            return view;
        }

    }
}