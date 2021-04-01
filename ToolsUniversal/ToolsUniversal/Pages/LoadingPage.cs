using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ToolsUniversal.Pages
{
    public class LoadingPage : NavigationPage
    {
        /// <summary>
        /// Will hide contents of page.
        /// </summary>
        protected void StartLoading()
        {
            if (base.Content != null)
                base.Content.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// Will make sure contents of page are displayed.
        /// </summary>
        protected void DoneLoading()
        {
            if (base.Content != null)
                base.Content.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }
}
