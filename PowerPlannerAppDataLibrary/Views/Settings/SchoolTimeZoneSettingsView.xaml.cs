using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PowerPlannerAppDataLibrary.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchoolTimeZoneSettingsView : ContentView
    {
        public SchoolTimeZoneSettingsViewModel ViewModel
        {
            get => BindingContext as SchoolTimeZoneSettingsViewModel;
            set => BindingContext = value;
        }

        public SchoolTimeZoneSettingsView()
        {
            InitializeComponent();
        }

        private void ButtonSave_Clicked(object sender, EventArgs e)
        {
            ViewModel.Save();
        }
    }
}