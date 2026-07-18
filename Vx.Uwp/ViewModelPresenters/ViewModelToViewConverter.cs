using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace InterfacesUWP.ViewModelPresenters
{
    public partial class ViewModelToViewConverter : IValueConverter
    {
        private static Dictionary<Type, Func<object, object>> ViewModelToViewMappings = new Dictionary<Type, Func<object, object>>();
        private static Dictionary<Type, Func<object, object>> GenericViewModelToViewMappings = new Dictionary<Type, Func<object, object>>();

        public static void AddMapping(Type viewModelType, Func<object, object> createView)
        {
            ViewModelToViewMappings[viewModelType] = createView;
        }

        public static void AddGenericMapping(Type viewModelType, Func<object, object> createView)
        {
            GenericViewModelToViewMappings[viewModelType] = createView;
        }

        private static bool TryFindGeneric(Type viewModelType, out Func<object, object> createView)
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

            object view;

            if (ViewModelToViewMappings.TryGetValue(value.GetType(), out Func<object, object> createView))
            {
                view = createView(value);
            }

            else if (TryFindGeneric(value.GetType(), out Func<object, object> createGenericView))
            {
                view = createGenericView(value);
            }

            else if (value is PagedViewModelWithPopups pagedViewModelWithPopups)
            {
                view = new PagedViewModelWithPopupsPresenter { ViewModel = pagedViewModelWithPopups };
            }

            else if (value is PagedViewModel pagedViewModel)
            {
                view = new PagedViewModelPresenter { ViewModel = pagedViewModel };
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

            // And return the view
            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
