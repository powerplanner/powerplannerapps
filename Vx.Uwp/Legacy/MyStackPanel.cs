using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace InterfacesUWP
{
    public class MyStackPanel : StackPanel
    {
        public event EventHandler MouseDownChanged, MouseOverChanged;

        public bool IsMouseDown { get; private set; }
        public bool IsMouseOver { get; private set; }

        public MyStackPanel()
        {
            base.PointerPressed += MyGrid_PointerPressed;
            base.PointerReleased += MyGrid_PointerReleased;

            base.PointerEntered += MyGrid_PointerEntered;
            base.PointerExited += MyGrid_PointerExited;

            base.PointerCanceled += MyGrid_PointerCanceled;

            base.PointerCaptureLost += MyGrid_PointerCaptureLost;
        }

        void MyGrid_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MyGrid_PointerReleased(sender, e);
        }

        void MyGrid_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!IsMouseDown)
                return;

            IsMouseDown = false;

            OnMouseDownChanged(e);

            if (MouseDownChanged != null)
                MouseDownChanged(this, new EventArgs());
        }

        void MyGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MyGrid_PointerReleased(sender, e);

            IsMouseOver = false;

            OnMouseOverChanged(e);

            if (MouseOverChanged != null)
                MouseOverChanged(this, new EventArgs());
        }

        void MyGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IsMouseOver = true;

            OnMouseOverChanged(e);

            if (MouseOverChanged != null)
                MouseOverChanged(this, new EventArgs());
        }

        void MyGrid_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!IsMouseDown)
                return;

            IsMouseDown = false;

            OnMouseDownChanged(e);

            if (MouseDownChanged != null)
                MouseDownChanged(this, new EventArgs());
        }

        void MyGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IsMouseDown = true;

            OnMouseDownChanged(e);

            if (MouseDownChanged != null)
                MouseDownChanged(this, new EventArgs());
        }

        protected virtual void OnMouseDownChanged(PointerRoutedEventArgs e)
        {

        }

        protected virtual void OnMouseOverChanged(PointerRoutedEventArgs e) { }
    }
}
