using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace InterfacesUWP
{
    public class MyPage : Page
    {
        private bool _useAutomaticBackButton;
        /// <summary>
        /// Gets or sets the property that determines whether the back button is automatically enabled. If true, the page will check if there's a back stack entry available and display the back button.
        /// </summary>
        public bool UseAutomaticBackButton
        {
            get { return _useAutomaticBackButton; }
            set
            {
                _useAutomaticBackButton = value;
            }
        }

        public static Page Current
        {
            get
            {
                if (Window.Current != null && Window.Current.Content is Frame)
                    return (Window.Current.Content as Frame).Content as Page;

                return null;
            }
        }

        /// <summary>
        /// Called when this page receives its first ever OnNavigatedTo event. Use this to initialize data.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLoaded(NavigationEventArgs e)
        {
            // nothing
        }

        private bool _isLoaded = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!_isLoaded || e.NavigationMode == NavigationMode.New)
            {
                _isLoaded = true;
                this.OnLoaded(e);
            }

            var navManager = SystemNavigationManagerEnhanced.GetForCurrentView();
            navManager.BackRequested += NavManager_BackRequested;

            if (UseAutomaticBackButton)
            {
                UpdateAutoAppViewBackButtonVisibility();
            }
        }

        /// <summary>
        /// Checks if there's back stack entries. If so, enables the back button. Otherwise, hides it.
        /// </summary>
        protected void UpdateAutoAppViewBackButtonVisibility()
        {
            this.AppViewBackButtonVisibility = base.Frame.BackStackDepth > 0 ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        protected AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get
            {
                return SystemNavigationManagerEnhanced.GetForCurrentView().AppViewBackButtonVisibility;
            }

            set
            {
                SystemNavigationManagerEnhanced.GetForCurrentView().AppViewBackButtonVisibility = value;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            var navManager = SystemNavigationManagerEnhanced.GetForCurrentView();
            navManager.BackRequested -= NavManager_BackRequested;

            if (UseAutomaticBackButton)
            {
                navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void NavManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            this.OnBackRequested(sender, e);
        }

        protected virtual void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled)
                return;

            if (UseAutomaticBackButton)
            {
                if (base.Frame.CanGoBack)
                {
                    base.Frame.GoBack();
                    e.Handled = true;
                }
            }
        }
    }
}
