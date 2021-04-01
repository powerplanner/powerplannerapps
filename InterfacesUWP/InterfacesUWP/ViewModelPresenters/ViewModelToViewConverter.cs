using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Reflection;
using Windows.UI.Xaml;
using Xamarin.Forms.Platform.UWP;

namespace InterfacesUWP.ViewModelPresenters
{
    public class ViewModelToViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // This gets called when a UWP native view contains another subview... so needs to return a UWP native control.

            if (value == null || !(value is BaseViewModel))
                return null;

            var viewModel = value as BaseViewModel;

            var cached = viewModel.GetCachedNativeView();
            if (cached != null)
            {
                // If there's already an existing cached native view (like for paging scenarios)
                // then use that
                return cached;
            }

            Type viewType;

            object view = null;

            if (BareMvvm.Forms.ViewModelPresenters.ViewModelToViewConverter.ViewModelToViewMappings.TryGetValue(value.GetType(), out viewType))
            {
                if (viewType.IsSubclassOf(typeof(UIElement)))
                {
                    view = Activator.CreateInstance(viewType);

                    // And assign the native view to the view model
                    viewModel.SetOriginalNativeView(view);
                    viewModel.SetCachedNativeView(view);

                    // Get the ViewModel property
                    var viewModelProperty = view.GetType().GetRuntimeProperties().FirstOrDefault(i => i.Name.Equals("ViewModel"));
                    if (viewModelProperty == null)
                    {
                        throw new InvalidOperationException("View must have a ViewModel property");
                    }

                    // And set the property
                    viewModelProperty.SetValue(view, value);
                }
            }

            if (view == null)
            {
                var formsView = BareMvvm.Forms.ViewModelPresenters.ViewModelToViewConverter.Convert(value as BaseViewModel);

                var formsPage = new Xamarin.Forms.ContentPage()
                {
                    Parent = Xamarin.Forms.Application.Current,
                    Content = formsView
                };

                view = formsPage.CreateFrameworkElement();

                viewModel.SetCachedNativeView(view);
            }

            // And assign the native view to the view model
            //if (value is BaseViewModel)
            //{
            //    (value as BaseViewModel).SetNativeView(view);
            //}

            // And return the view
            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
