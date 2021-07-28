using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarIntegrationClassesView : ViewHostGeneric
    {
        public new CalendarIntegrationClassesViewModel ViewModel
        {
            get { return base.ViewModel as CalendarIntegrationClassesViewModel; }
            set { base.ViewModel = value; }
        }

        public CalendarIntegrationClassesView()
        {
            this.InitializeComponent();
        }
    }
}
