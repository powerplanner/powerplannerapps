using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.App;

namespace InterfacesiOS.ViewModelPresenters
{
    public static class ViewModelToViewConverter
    {
        private static Dictionary<Type, Type> ViewModelToViewMappings = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> GenericViewModelToViewMappings = new Dictionary<Type, Type>();

        public static void AddMapping(Type viewModelType, Type viewType)
        {
            ViewModelToViewMappings[viewModelType] = viewType;
        }

        public static void AddGenericMapping(Type genericViewModelType, Type genericViewType)
        {
            GenericViewModelToViewMappings[genericViewModelType] = genericViewType;
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

        public static UIViewController Convert(BaseViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            var cachedView = viewModel.GetNativeView();
            if (cachedView is UIViewController)
            {
                // If there's already an existing cached native view (like for paging scenarios)
                // then use that
                return cachedView as UIViewController;
            }

            Type viewType;

            UIViewController view;

            if (ViewModelToViewMappings.TryGetValue(viewModel.GetType(), out viewType))
            {
                view = Activator.CreateInstance(viewType) as UIViewController;
                if (view == null)
                {
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#endif
                    throw new InvalidOperationException("Created view instance must be of type UIViewController");
                }
            }

            else if (TryFindGeneric(viewModel.GetType(), out Type genericViewType))
            {
                view = Activator.CreateInstance(genericViewType) as UIViewController;
                if (view == null)
                {
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#endif
                    throw new InvalidOperationException("Created view instance must be of type UIViewController");
                }
            }

            else if (viewModel is PagedViewModelWithPopups)
            {
                view = new PagedViewModelWithPopupsPresenter();
            }

            else if (viewModel is PagedViewModel)
            {
                view = new PagedViewModelPresenter();
            }

            else
            {
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
#endif
                throw new NotImplementedException("ViewModel type was unknown: " + viewModel.GetType());
            }

            // And assign the native view to the view model
            viewModel.SetNativeView(view);

            // Get the ViewModel property
            var viewModelProperty = view.GetType().GetRuntimeProperties().FirstOrDefault(i => i.Name.Equals("ViewModel"));
            if (viewModelProperty != null)
            {
                // And set the property
                viewModelProperty.SetValue(view, viewModel);
            }

            // And return the view
            return view;
        }
    }
}