using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpTransparentContentButton : UwpView<Vx.Views.TransparentContentButton, TransparentButton>
    {
        private ToolTip _toolTip;

        public UwpTransparentContentButton()
        {
            View.Click += View_Click;
            View.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.TransparentContentButton oldView, Vx.Views.TransparentContentButton newView)
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

    public class TransparentButton : Button
    {
        public TransparentButton()
        {
            DefaultStyleKey = typeof(TransparentButton);
        }
    }
}
