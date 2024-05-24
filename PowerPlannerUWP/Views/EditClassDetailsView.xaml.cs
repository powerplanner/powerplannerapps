using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerUWP.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class EditClassDetailsView : PopupViewHostGeneric
    {
        public new EditClassDetailsViewModel ViewModel
        {
            get { return base.ViewModel as EditClassDetailsViewModel; }
            set { base.ViewModel = value; }
        }

        public EditClassDetailsView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            PrimaryCommands.Add(PowerPlannerAppDataLibrary.ViewModels.PopupCommand.Save(ViewModel.Save));
        }

        private void Popup_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (KeyPressedHelpers.IsCtrlKeyPressed())
            {
                // Ctrl+Enter or Ctrl+S will save the popup
                if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.S)
                {
                    e.Handled = true;
                    ViewModel.Save();
                }
            }
        }
    }
}
