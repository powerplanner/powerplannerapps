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
    public class CancelAndDelete : AppBarButton
    {
        public event EventHandler OnCancel, OnDelete;

        public CancelAndDelete()
        {
            Icon = new SymbolIcon(Symbol.More);
            IsCompact = true;

            base.Click += EditAndDelete_Click;
        }

        void EditAndDelete_Click(object sender, RoutedEventArgs e)
        {
            show(this);
        }

        private static PopupMenu _menu;
        private static CancelAndDelete _callingButton;

        private static void show(CancelAndDelete button)
        {
            if (_menu == null)
            {
                _menu = new PopupMenu();
                _menu.Commands.Add(new UICommand(LocalizedResources.GetString("MenuItemCancel"), onCancelClicked));
                _menu.Commands.Add(new UICommand(LocalizedResources.GetString("MenuItemDelete"), onDeleteClicked));
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
