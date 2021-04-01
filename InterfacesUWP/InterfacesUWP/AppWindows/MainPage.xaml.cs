using BareMvvm.Forms.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Xamarin.Forms.Xaml;

namespace InterfacesUWP.AppWindows
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            LoadApplication(Activator.CreateInstance(FormsApp.Current.GetAppShellType()) as Application);
        }
    }
}