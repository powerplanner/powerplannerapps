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
using Xamarin.Forms.Platform.Android;
using BareMvvm.Core.App;
using InterfacesDroid.Windows;

namespace InterfacesDroid.ViewModelPresenters
{
    public class ViewModelToViewConverter
    {
        private static Dictionary<Type, Type> ViewModelToSplashMappings = new Dictionary<Type, Type>();

        static ViewModelToViewConverter()
        {
            BareMvvm.Core.ViewModelPresenters.ViewModelToViewConverter.NativeViewCreator = CreateView;
            BareMvvm.Core.ViewModelPresenters.ViewModelToViewConverter.NativeViewWrapper = (nativeDroidView) =>
            {
                var view = nativeDroidView as View;

                return view.ToView();
            };
        }

        public static void AddMapping(Type viewModelType, Type viewType)
        {
            BareMvvm.Core.ViewModelPresenters.ViewModelToViewConverter.AddMapping(viewModelType, viewType);
        }

        public static void AddSplashMapping(Type viewModelType, Type viewType)
        {
            ViewModelToSplashMappings[viewModelType] = viewType;
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

            Type viewType;
            
            View view = null;

            if (BareMvvm.Core.ViewModelPresenters.ViewModelToViewConverter.ViewModelToViewMappings.TryGetValue(value.GetType(), out viewType))
            {
                view = CreateView(root, viewType);
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

        private static object CreateView(Type viewType)
        {
            if (!typeof(View).IsAssignableFrom(viewType))
            {
                // Xamarin Forms view
                return Activator.CreateInstance(viewType);
            }

            Context context = (PortableApp.Current.GetCurrentWindow().NativeAppWindow as NativeDroidAppWindow).Activity;

            try
            {
                if (viewType.IsSubclassOf(typeof(ViewHostGeneric)))
                    return Activator.CreateInstance(viewType, new FrameLayout(context));
                //throw new NotImplementedException();
                //return (View)Activator.CreateInstance(viewType, root);
                else
                    return (View)Activator.CreateInstance(viewType, context);
            }
            catch (TargetInvocationException ex)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new TargetInvocationException("View likely didn't have the correct constructor.", ex);
            }
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