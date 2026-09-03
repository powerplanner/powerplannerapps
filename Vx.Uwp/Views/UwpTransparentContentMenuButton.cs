using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    internal class UwpTransparentContentMenuButton : UwpView<TransparentContentMenuButton, TransparentButton>
    {
        private ToolTip _toolTip;

        public UwpTransparentContentMenuButton()
        {
            View.Click += View_Click;
            View.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            var menuItems = VxView?.Menu;
            if (menuItems != null && menuItems.Count > 0)
            {
                var cm = new ContextMenu();
                cm.Items.AddRange(menuItems);
                cm.Show(VxViewRef);
            }
        }

        protected override void ApplyProperties(TransparentContentMenuButton oldView, TransparentContentMenuButton newView)
        {
            base.ApplyProperties(oldView, newView);

            VxReconciler.Reconcile(oldView?.Content, newView.Content, view => View.Content = view?.CreateFrameworkElement());

            AutomationProperties.SetName(View, newView.AltText ?? newView.TooltipText ?? "");

            if (newView.TooltipText != null)
            {
                if (_toolTip == null)
                {
                    _toolTip = new ToolTip()
                    {
                        Content = newView.TooltipText
                    };

                    ToolTipService.SetToolTip(View, _toolTip);
                }
                else
                {
                    _toolTip.Content = newView.TooltipText;
                }
            }
            else
            {
                if (_toolTip != null)
                {
                    ToolTipService.SetToolTip(View, null);
                    _toolTip = null;
                }
            }
        }
    }
}
