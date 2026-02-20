using BareMvvm.Core.App;
using BareMvvm.Core.Windows;
using InterfacesUWP.Extensions;
using InterfacesUWP.ViewModelPresenters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Extensions;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.System.UserProfile;

namespace InterfacesUWP.App
{
    public abstract class NativeUwpApplication : Application
    {
        public static CultureInfo OriginalCultureInfo => CultureOverride.OriginalCultureInfo;

        public NativeUwpApplication()
        {
            // Capture the true device culture before any override is applied.
            // On UWP, if PrimaryLanguageOverride was set in a previous session,
            // CultureInfo.CurrentUICulture is already the overridden culture at startup,
            // so we must get the actual device language from GlobalizationPreferences.
            try
            {
                var deviceLanguage = GlobalizationPreferences.Languages.FirstOrDefault();
                if (!string.IsNullOrEmpty(deviceLanguage))
                {
                    CultureOverride.SetOriginalCultureInfo(new CultureInfo(deviceLanguage));
                }
            }
            catch { }

            // Set culture info based on app language overrides
            try
            {
                if (ApplicationLanguages.PrimaryLanguageOverride != null && ApplicationLanguages.PrimaryLanguageOverride.Length > 0)
                {
                    CultureOverride.ApplyCultureOverride(ApplicationLanguages.PrimaryLanguageOverride);
                }
            }
            catch { }

            // Register the view model to view mappings
            foreach (var mapping in GetViewModelToViewMappings())
            {
                ViewModelToViewConverter.AddMapping(mapping.Key, mapping.Value);
            }

            foreach (var mapping in GetGenericViewModelToViewMappings())
            {
                ViewModelToViewConverter.AddGenericMapping(mapping.Key, mapping.Value);
            }

            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new UwpDispatcher(); };

            // Register message dialog
            PortableMessageDialog.Extension = UwpMessageDialog.ShowAsync;

            PortableLocalizedResources.CultureExtension = GetCultureInfo;

            // Initialize the app
            PortableApp.InitializeAsync((PortableApp)Activator.CreateInstance(GetPortableAppType()));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            OnLaunchedOrActivated(args);
        }

        private async void MyOnLaunchedOrActivated(IActivatedEventArgs args)
        {
            try
            {
                await PortableApp.InitializeTask;
                await OnLaunchedOrActivated(args);
            }

            catch
            {
            }
        }

        protected abstract Task OnLaunchedOrActivated(IActivatedEventArgs args);

        public abstract Dictionary<Type, Type> GetViewModelToViewMappings();

        public abstract Dictionary<Type, Type> GetGenericViewModelToViewMappings();

        public abstract Type GetPortableAppType();

        private CultureInfo GetCultureInfo()
        {
            return CultureInfo.CurrentCulture;
        }
    }
}
