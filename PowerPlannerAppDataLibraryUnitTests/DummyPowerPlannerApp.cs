using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibraryUnitTests
{
    public class DummyPowerPlannerApp : PowerPlannerApp
    {
        public static new DummyPowerPlannerApp Current => PowerPlannerApp.Current as DummyPowerPlannerApp;

        public async Task LaunchAsync()
        {
            MainAppWindow mainAppWindow;

            // If no windows, need to register window
            mainAppWindow = PowerPlannerApp.Current.Windows.OfType<MainAppWindow>().FirstOrDefault();
            if (mainAppWindow == null)
            {
                // This configures the view models, does NOT call Activate yet
                var nativeWindow = new DummyPowerPlannerWindow();
                mainAppWindow = new MainAppWindow();
                await PowerPlannerApp.Current.RegisterWindowAsync(mainAppWindow, nativeWindow);

                if (PowerPlannerApp.Current.Windows.Count > 1)
                {
                    throw new Exception("There are more than 1 windows registered");
                }
            }
        }

        public static async Task InitializeAndLaunchAsync()
        {
            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new DummyDispatcher(); };

            // Register message dialog
            PortableMessageDialog.Extension = (messageDialog) => { DummyMessageDialog.Show(messageDialog); return Task.FromResult(true); };

            // Initialize the app
            await PowerPlannerApp.InitializeAsync((PowerPlannerApp)Activator.CreateInstance(typeof(DummyPowerPlannerApp)));

            await Current.LaunchAsync();

            await Current.GetMainWindowViewModel().HandleNormalLaunchActivation();
        }
    }
}
