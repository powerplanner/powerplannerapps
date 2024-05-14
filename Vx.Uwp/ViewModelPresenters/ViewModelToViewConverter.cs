using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Reflection;

namespace InterfacesUWP.ViewModelPresenters
{
    public class ViewModelToViewConverter : IValueConverter
    {
        private static Dictionary<Type, Type> ViewModelToViewMappings = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> GenericViewModelToViewMappings = new Dictionary<Type, Type>();

        public static void AddMapping(Type viewModelType, Type viewType)
        {
            ViewModelToViewMappings[viewModelType] = viewType;
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

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (value is BaseViewModel)
            {
                var baseViewModel = value as BaseViewModel;
                var cached = baseViewModel.GetNativeView();
                if (cached != null)
                {
                    return cached;
                }
            }

            Type viewType;

            object view;

            if (ViewModelToViewMappings.TryGetValue(value.GetType(), out viewType))
            {
                view = Activator.CreateInstance(viewType);
            }

            else if (TryFindGeneric(value.GetType(), out Type genericViewType))
            {
                view = Activator.CreateInstance(genericViewType);
            }

            else if (value is PagedViewModelWithPopups)
            {
                view = new PagedViewModelWithPopupsPresenter();
            }

            else if (value is PagedViewModel)
            {
                view = new PagedViewModelPresenter();
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
            var viewModelProperty = view.GetType().GetRuntimeProperties().FirstOrDefault(i => i.Name.Equals("ViewModel"));
            if (viewModelProperty == null)
            {
                throw new InvalidOperationException("View must have a ViewModel property");
            }

            // And set the property
            try
            {
                viewModelProperty.SetValue(view, value);
            }
            catch (Exception ex)
            {
                ToolsPortable.ExceptionHelper.OnHandledExceptionOccurred?.Invoke(ex);
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
#endif
            }

            // And return the view
            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
