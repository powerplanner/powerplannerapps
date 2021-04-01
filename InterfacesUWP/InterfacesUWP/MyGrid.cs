using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace InterfacesUWP
{
    public class MyGrid : Grid
    {
        public event EventHandler MouseDownChanged, MouseOverChanged;

        public bool IsMouseDown { get; private set; }
        public bool IsMouseOver { get; private set; }

        public MyGrid()
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
            ClearMouseDownAndMouseOver(e);
        }

        private void ClearMouseDownAndMouseOver(PointerRoutedEventArgs e)
        {
            bool changedMouseDown = false;

            if (IsMouseDown)
            {
                IsMouseDown = false;
                changedMouseDown = true;
            }

            bool changedMouseOver = false;

            if (IsMouseOver)
            {
                IsMouseOver = false;
                changedMouseOver = true;
            }

            // We have to set both properties first before sending the changed event, since both of these changed at the same time. Then we'll send the changed event

            if (changedMouseDown)
                TriggerMouseDownChanged(e);

            if (changedMouseOver)
                TriggerMouseOverChanged(e);
        }

        private void TriggerMouseDownChanged(PointerRoutedEventArgs e)
        {
            OnMouseDownChanged(e);

            if (MouseDownChanged != null)
                MouseDownChanged(this, new EventArgs());
        }

        private void TriggerMouseOverChanged(PointerRoutedEventArgs e)
        {
            OnMouseOverChanged(e);

            if (MouseOverChanged != null)
                MouseOverChanged(this, new EventArgs());
        }

        void MyGrid_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ClearMouseDownAndMouseOver(e);
        }

        void MyGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ClearMouseDownAndMouseOver(e);
        }

        void MyGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (IsMouseOver)
                return;

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
            if (IsMouseDown)
                return;

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
