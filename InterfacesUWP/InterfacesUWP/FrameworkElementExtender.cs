using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace InterfacesUWP
{
    public class FrameworkElementExtender
    {
        public FrameworkElement FrameworkElement { get; private set; }

        public bool IsLoaded { get; private set; }

        public FrameworkElementExtender(FrameworkElement el)
        {
            FrameworkElement = el;
            FrameworkElement.Loaded += FrameworkElement_Loaded;
        }

        private List<Action> _onLoadedActions;
        private void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = true;

            if (_onLoadedActions != null)
            {
                foreach (var a in _onLoadedActions)
                {
                    a();
                }
                _onLoadedActions = null;
            }
        }

        public void OnLoaded(Action action)
        {
            if (IsLoaded)
            {
                action();
            }
            else
            {
                if (_onLoadedActions == null)
                {
                    _onLoadedActions = new List<Action>();
                }
                _onLoadedActions.Add(action);
            }
        }
    }
}
