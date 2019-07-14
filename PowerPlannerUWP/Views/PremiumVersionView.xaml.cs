using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PremiumVersionView : PopupViewHostGeneric
    {
        public new PremiumVersionViewModel ViewModel
        {
            get { return base.ViewModel as PremiumVersionViewModel; }
            set { base.ViewModel = value; }
        }

        public PremiumVersionView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            if (ViewModel.ContextualMessage != null)
            {
                TextBlockInitialMessage.Text = ViewModel.ContextualMessage;
                TextBlockInitialMessage.Visibility = Visibility.Visible;
            }

            else
            {
                TextBlockInitialMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonUpgrade_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PromptPurchase();
        }
    }
}
