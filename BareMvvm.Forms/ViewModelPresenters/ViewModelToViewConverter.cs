using BareMvvm.Core.ViewModels;
using BareMvvm.Forms.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace BareMvvm.Forms.ViewModelPresenters
{
    public class ViewModelToViewConverter : IValueConverter
    {
        public static Func<object, View> NativeViewWrapper { get; set; }

        public static Dictionary<Type, Type> ViewModelToViewMappings = new Dictionary<Type, Type>();

        public static void AddMapping(Type viewModelType, Type viewType)
        {
            ViewModelToViewMappings[viewModelType] = viewType;
        }

        public static View Convert(BaseViewModel viewModel)
        {
            if (viewModel is FormsViewViewModel formsViewViewModel)
            {
                return formsViewViewModel.View;
            }

            // If we've actually implemented the Render method
            var renderMethod = viewModel.GetType().GetMethod("Render", BindingFlags.Instance | BindingFlags.NonPublic);
            if (renderMethod.DeclaringType != typeof(Vx.Views.VxComponent))
            {
                viewModel.PrepareComponentForDisplay();
                return viewModel;
            }

            var cached = viewModel.GetCachedNativeView() as View;
            if (cached != null)
            {
                // If there's already an existing cached native view (like for paging scenarios)
                // then use that
                return cached;
            }

            View view;
            bool wasNativeView = false;

            if (ViewModelToViewMappings.TryGetValue(viewModel.GetType(), out Type viewType))
            {
                object rawView = Activator.CreateInstance(viewType);

                viewModel.SetOriginalNativeView(rawView);

                if (rawView is View formsView)
                {
                    view = formsView;
                    var rootProp = viewType.GetProperty("IsRootComponent");
                    if (rootProp != null)
                    {
                        rootProp.SetValue(formsView, true);
                    }
                }

                else
                {
                    view = NativeViewWrapper(rawView);
                    wasNativeView = true;

                    // Get the ViewModel property
                    var nativeViewModelProperty = rawView.GetType().GetRuntimeProperties().FirstOrDefault(i => i.Name.Equals("ViewModel"));
                    if (nativeViewModelProperty == null)
                    {
                        throw new InvalidOperationException("View must have a ViewModel property");
                    }

                    // And set the property
                    nativeViewModelProperty.SetValue(rawView, viewModel);
                }
            }

            else if (viewModel is PagedViewModelWithPopups)
            {
                view = new PagedViewModelWithPopupsPresenter();
                viewModel.SetOriginalNativeView(view);
            }

            else if (viewModel is PagedViewModel)
            {
                view = new PagedViewModelPresenter();
                viewModel.SetOriginalNativeView(view);
            }

            else
            {
                throw new NotImplementedException("ViewModel type was unknown: " + viewModel.GetType());
            }

            // And assign the native view to the view model
            viewModel.SetCachedNativeView(view);

            if (!wasNativeView)
            {
                // Get the ViewModel property
                var viewModelProperty = view.GetType().GetRuntimeProperties().FirstOrDefault(i => i.Name.Equals("ViewModel"));
                if (viewModelProperty == null)
                {
                    throw new InvalidOperationException("View must have a ViewModel property");
                }

                // And set the property
                viewModelProperty.SetValue(view, viewModel);
            }

            // And return the view
            return view;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BaseViewModel viewModel)
            {
                return Convert(viewModel);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
