using BareMvvm.Core.ViewModelPresenters;
using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace BareMvvm.Core.App
{
    public abstract class PortableApp
    {
        public static PortableApp Current { get; private set; }

        public PortableApp()
        {
            // Register the view model to view mappings
            foreach (var mapping in GetViewModelToViewMappings())
            {
                ViewModelToViewConverter.AddMapping(mapping.Key, mapping.Value);
            }

            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new FormsDispatcher(); };

            // Register message dialog
            //PortableMessageDialog.Extension = UwpMessageDialog.ShowAsync;

            //PortableLocalizedResources.CultureExtension = GetCultureInfo;
        }

        public static Task InitializeTask { get; private set; }
        public static Task InitializeAsync(PortableApp extendedAppInstance)
        {
            Current = extendedAppInstance;

            InitializeTask = extendedAppInstance.InitializeAsyncOverride();
            return InitializeTask;
        }

        protected virtual Task InitializeAsyncOverride()
        {
            return Task.FromResult(true);
        }

        public async Task RegisterWindowAsync(PortableAppWindow window, INativeAppWindow nativeWindow)
        {
            // Wait till InitializeAsync finishes
            await InitializeTask;

            window.Register(nativeWindow);

            // Add the window
            Windows.Add(window);
        }

        public async Task UnregisterWindowAsync(PortableAppWindow window)
        {
            // Wait till InitializeAsync finishes
            await InitializeTask;

            window.Unregister();

            Windows.Remove(window);
        }

        public ObservableCollection<PortableAppWindow> Windows { get; private set; } = new ObservableCollection<PortableAppWindow>();

        public PortableAppWindow GetCurrentWindow()
        {
            // When we actually support multiple windows, we'll have to change this
            return Windows.FirstOrDefault();
        }

        public Task TriggerSuspending()
        {
            return OnSuspending();
        }

        protected virtual Task OnSuspending()
        {
            return Task.FromResult(true);
        }

        public abstract Dictionary<Type, Type> GetViewModelToViewMappings();

        public abstract Type GetAppShellType();
    }
}
