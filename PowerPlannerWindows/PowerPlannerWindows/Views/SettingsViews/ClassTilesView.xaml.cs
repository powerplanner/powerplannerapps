using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPlannerUWP.ViewModel.Settings;
using InterfacesUWP.Views;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassTilesView : ViewHostGeneric
    {
        public new ClassTilesViewModel ViewModel
        {
            get { return base.ViewModel as ClassTilesViewModel; }
            set { base.ViewModel = value; }
        }

        public ClassTilesView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            if (ViewModel.HasSemester)
            {
                if (ViewModel.HasClasses)
                {
                    ListViewClasses.Visibility = Visibility.Visible;
                }
                else
                {
                    TextBlockNoClasses.Visibility = Visibility.Visible;
                }
            }
            else
            {
                TextBlockNoSemester.Visibility = Visibility.Visible;
            }
        }

        private void ListViewClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClassTilesViewModel.ClassAndPinnedStatus item = ListViewClasses.SelectedItem as ClassTilesViewModel.ClassAndPinnedStatus;
            if (item == null)
                return;

            ListViewClasses.SelectedIndex = -1;

            ViewModel.SelectClass(item);
        }
    }
}
