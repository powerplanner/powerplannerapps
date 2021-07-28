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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
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
