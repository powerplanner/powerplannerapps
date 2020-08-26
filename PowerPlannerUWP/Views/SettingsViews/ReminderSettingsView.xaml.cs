using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReminderSettingsView : ViewHostGeneric
    {
        public ReminderSettingsView()
        {
            this.InitializeComponent();

            TextBlockRemindMe.Text = PowerPlannerResources.GetString("String_RemindMe");
        }
    }
}
