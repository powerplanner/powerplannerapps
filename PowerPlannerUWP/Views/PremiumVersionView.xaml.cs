using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using Windows.UI.Xaml;

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
