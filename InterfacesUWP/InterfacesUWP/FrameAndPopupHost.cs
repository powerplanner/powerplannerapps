using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUniversal;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InterfacesUWP
{
    public class FrameAndPopupHost : Grid
    {
        public Frame Frame { get; private set; }

        public FrameAndPopupHost() : this(new Frame()) { }

        public FrameAndPopupHost(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            Frame = frame;
            _popupsGrid = new Grid();

            base.Children.Add(Frame);
            base.Children.Add(_popupsGrid);
        }

        private Grid _popupsGrid;

        public UIElementCollection Popups
        {
            get { return _popupsGrid.Children; }
        }

        /// <summary>
        /// Creates a popup, adding it to the popups collection - it's instantly visible too.
        /// </summary>
        /// <returns></returns>
        public PopupInsideFrame CreatePopup()
        {
            var popup = new PopupInsideFrame(this);
            Popups.Add(popup);
            return popup;
        }
    }

    public class PopupInsideFrame : ContentControl
    {
        public event EventHandler Closed;

        public FrameAndPopupHost Host { get; private set; }

        public bool IsClosed
        {
            get
            {
                return !Host.Popups.Contains(this);
            }
        }

        internal PopupInsideFrame(FrameAndPopupHost host)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            Host = host;

            base.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            base.VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        /// <summary>
        /// Removes the popup from the view. If you want to temporarily hide, change the Visibility property.
        /// </summary>
        public void Close()
        {
            Host.Popups.Remove(this);

            if (Closed != null)
                Closed(this, new EventArgs());
        }
    }

    public class PopupFrame : Frame
    {
        public event EventHandler RequestClose;

        public PopupFrame()
        {
            SystemNavigationManagerEnhanced.GetForCurrentView().BackRequested += PopupFrame_BackRequested;
        }

        private void PopupFrame_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            if (CanGoBack)
            {
                e.Handled = true;

                GoBack();
            }
        }

        /// <summary>
        /// Always returns true, since always can go back (closing the popup counts as going back)
        /// </summary>
        public new bool CanGoBack
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Goes back, or closes the popup if can't go back
        /// </summary>
        public new void GoBack()
        {
            SystemNavigationManagerEnhanced.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;

            if (base.CanGoBack)
                base.GoBack();

            else
                ClosePopup();
        }

        /// <summary>
        /// Closes the entire popup/frame
        /// </summary>
        public void ClosePopup()
        {
            if (RequestClose != null)
                RequestClose(this, new EventArgs());

            SystemNavigationManagerEnhanced.GetForCurrentView().BackRequested -= PopupFrame_BackRequested;
        }
    }
}
