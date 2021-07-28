using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.MenuButtons
{
    public class CancelAndDeleteBlue : Button
    {
        public event EventHandler OnCancel, OnDelete;

        public CancelAndDeleteBlue()
        {
            base.Style = Application.Current.Resources["PowerPlannerBlueButtonStyle"] as Style;
            base.Content = new SymbolIcon(Symbol.More);

            base.Click += EditAndDelete_Click;
        }

        void EditAndDelete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            show(this);
        }

        private static PopupMenu _menu;
        private static CancelAndDeleteBlue _callingButton;

        private static void show(CancelAndDeleteBlue button)
        {
            if (_menu == null)
            {
                _menu = new PopupMenu();
                _menu.Commands.Add(new UICommand("cancel", onCancelClicked));
                _menu.Commands.Add(new UICommand("delete", onDeleteClicked));
            }

            _callingButton = button;

            Point p = button.TransformToVisual(null).TransformPoint(new Point());

            //sometimes this ShowAsync method throws an exception
            try
            {
                var dontWait = _menu.ShowAsync(new Point(p.X, p.Y + button.ActualHeight));
            }

            catch { }
        }

        private static void onCancelClicked(IUICommand cmd)
        {
            if (_callingButton == null)
                return;

            if (_callingButton.OnCancel != null)
                _callingButton.OnCancel(_callingButton, null);
        }

        private static void onDeleteClicked(IUICommand cmd)
        {
            if (_callingButton == null)
                return;

            if (_callingButton.OnDelete != null)
                _callingButton.OnDelete(_callingButton, null);
        }
    }
}
