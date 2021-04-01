using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace InterfacesUWP
{
    public class SystemNavigationManagerEnhanced
    {
        public SystemNavigationManager OriginalObject { get; private set; }

        private SystemNavigationManagerEnhanced(SystemNavigationManager original)
        {
            OriginalObject = original;

            original.BackRequested += Original_BackRequested;
        }

        private void Original_BackRequested(object sender, BackRequestedEventArgs e)
        {
            foreach (var d in _backRequestedEvents.ToArray()) // Copy to array since this invocation may cause a removal
            {
                if (e.Handled)
                    return;

                d.DynamicInvoke(sender, e);
            }
        }

        public AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get
            {
                return OriginalObject.AppViewBackButtonVisibility;
            }

            set { OriginalObject.AppViewBackButtonVisibility = value; }
        }

        private List<EventHandler<BackRequestedEventArgs>> _backRequestedEvents = new List<EventHandler<BackRequestedEventArgs>>();

        /// <summary>
        /// Sends events in opposite order... Last one to register is the first one to receive an event. Also, if event is handled, any below it don't receive the event
        /// </summary>
        public event EventHandler<BackRequestedEventArgs> BackRequested
        {
            add
            {
                _backRequestedEvents.Remove(value);

                _backRequestedEvents.Insert(0, value);
            }

            remove
            {
                _backRequestedEvents.Remove(value);
            }
        }

        private static Dictionary<SystemNavigationManager, SystemNavigationManagerEnhanced> _cached = new Dictionary<SystemNavigationManager, SystemNavigationManagerEnhanced>();

        public static SystemNavigationManagerEnhanced GetForCurrentView()
        {
            SystemNavigationManagerEnhanced answer;

            var original = SystemNavigationManager.GetForCurrentView();

            if (!_cached.TryGetValue(original, out answer))
            {
                answer = new SystemNavigationManagerEnhanced(original);

                _cached[original] = answer;
            }

            return answer;
        }
    }
}
