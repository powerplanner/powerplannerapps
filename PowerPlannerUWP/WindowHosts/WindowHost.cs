using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PowerPlannerUWP.WindowHosts
{
    public abstract class WindowHost : IDisposable
    {
        //private const string WINDOW_HOST_PROPERTY = "WindowHost";

        public ViewLifetimeControl View { get; private set; }

        protected abstract Frame GenerateRootFrame();
        
        private Task<bool> _currShowTask;

        /// <summary>
        /// Creates and shows the window (or switches focus to it if already shown)
        /// </summary>
        public Task<bool> Show()
        {
            lock (this)
            {
                if (_currShowTask == null)
                    _currShowTask = ShowTask();

                return _currShowTask;
            }
        }

        private async Task<bool> ShowTask()
        {
            try
            {
                if (View == null)
                {
                    View = await ViewLifetimeControl.CreateForCurrentView();

                    bool status = true;

                    //on that view's thread, we need to assign the frame, activate it, and obtain its view ID
                    await View.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Frame frame = GenerateRootFrame();
                        if (frame == null)
                        {
                            status = false;
                            return;
                        }

                        Window.Current.Content = frame;

                        Window.Current.Activate();

                        // Prevent view from closing while switching to it
                        View.StartViewInUse();
                    });

                    if (!status)
                        return false;

                    //then on the current (calling) thread, we can show the view
                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(View.Id);

                    // Signal that switching has completed, letting the view close
                    View.StopViewInUse();

                    //View.CoreWindow.CustomProperties[WINDOW_HOST_PROPERTY] = this;
                    return true;
                }

                else
                {
                    await ApplicationViewSwitcher.SwitchAsync(View.Id);
                    return true;
                }
            }

            finally
            {
                lock (this)
                {
                    _currShowTask = null;
                }
            }
        }

        public static WindowHost GetCurrentWindowHost()
        {
            throw new NotImplementedException();
            //return Window.Current.CoreWindow.CustomProperties[WINDOW_HOST_PROPERTY] as WindowHost;
        }

        /// <summary>
        /// Closes the window. Can be called from any thread.
        /// </summary>
        public async void Close()
        {
            if (View == null)
                return;
            
            try
            {
                await View.Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                {
                    Window.Current.Close();
                    //View.CoreWindow.Close();
                });
            }

            catch { }
        }

        public virtual void Dispose()
        {
            View = null;
            _currShowTask = null;
        }
    }
}
