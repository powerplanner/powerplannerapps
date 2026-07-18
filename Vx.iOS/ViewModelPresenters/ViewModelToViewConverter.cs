using System;
using System.Collections.Generic;
using UIKit;
using BareMvvm.Core.ViewModels;

namespace InterfacesiOS.ViewModelPresenters
{
    public static class ViewModelToViewConverter
    {
        private static readonly Dictionary<Type, Func<UIViewController>> ViewModelToViewMappings = new Dictionary<Type, Func<UIViewController>>();
        private static readonly Dictionary<Type, Func<UIViewController>> GenericViewModelToViewMappings = new Dictionary<Type, Func<UIViewController>>();

        public static void AddMapping(Type viewModelType, Func<UIViewController> createView)
        {
            ViewModelToViewMappings[viewModelType] = createView;
        }

        public static void AddGenericMapping(Type genericViewModelType, Func<UIViewController> createView)
        {
            GenericViewModelToViewMappings[genericViewModelType] = createView;
        }

        private static bool TryFindGeneric(Type viewModelType, out Func<UIViewController> createView)
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

            UIViewController view;

            if (ViewModelToViewMappings.TryGetValue(viewModel.GetType(), out Func<UIViewController> createView))
            {
                view = createView();
            }

            else if (TryFindGeneric(viewModel.GetType(), out Func<UIViewController> createGenericView))
            {
                view = createGenericView();
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

            if (view is not IViewModelHost viewModelHost)
            {
                throw new InvalidOperationException("Mapped view must implement IViewModelHost.");
            }

            viewModel.SetNativeView(view);
            viewModelHost.ViewModel = viewModel;
            return view;
        }

        public static BaseViewModel GetViewModelFromView(UIViewController view)
        {
            return (view as IViewModelHost)?.ViewModel;
        }
    }
}