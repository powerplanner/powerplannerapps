using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareMvvm.Core.App
{
    public class PortableApp
    {
        public static PortableApp Current { get; private set; }

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

        public List<PortableAppWindow> Windows { get; private set; } = new List<PortableAppWindow>();

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
    }
}
