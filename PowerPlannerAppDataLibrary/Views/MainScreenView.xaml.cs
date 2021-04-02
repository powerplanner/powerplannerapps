using BareMvvm.Forms.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerPlannerAppDataLibrary.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainScreenView : ViewModelView
    {
        public MainScreenView()
        {
            InitializeComponent();
        }

        private void ViewModelView_SizeChanged(object sender, EventArgs e)
        {
            if (base.Width > 550)
            {
                if (FullSizeSideMenuContainer.Content == null)
                {
                    SwitchToFullSize();
                }
            }
            else
            {
                if (CompactBottomMenuContainer.Content == null)
                {
                    SwitchToCompact();
                }
            }
        }

        private void SwitchToFullSize()
        {
            CompactBottomMenuContainer.Content = null;

            FullSizeSideMenuContainer.Content = new MainScreenSideMenu();
        }

        private void SwitchToCompact()
        {
            FullSizeSideMenuContainer.Content = null;
        }
    }
}