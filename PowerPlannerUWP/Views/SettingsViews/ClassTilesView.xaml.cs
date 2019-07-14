using InterfacesUWP;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerUWPLibrary.TileHelpers;


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
