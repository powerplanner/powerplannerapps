using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesUWP.Views;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PushSettingsView : ViewHostGeneric
    {
        public new PushSettingsViewModel ViewModel
        {
            get { return base.ViewModel as PushSettingsViewModel; }
            set { base.ViewModel = value; }
        }

        public PushSettingsView()
        {
            this.InitializeComponent();
        }
    }
}
