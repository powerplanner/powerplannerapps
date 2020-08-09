using PowerPlannerUWP.Views;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddYearView : PopupViewHostGeneric
    {
        public new AddYearViewModel ViewModel
        {
            get { return base.ViewModel as AddYearViewModel; }
            set { base.ViewModel = value; }
        }

        public AddYearView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            switch (ViewModel.State)
            {
                case AddYearViewModel.OperationState.Adding:
                    base.Title = LocalizedResources.GetString("AddYearPage_Title_Adding");
                    break;

                case AddYearViewModel.OperationState.Editing:
                    base.Title = LocalizedResources.GetString("AddYearPage_Title_Editing");
                    break;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void tbName_Loaded(object sender, RoutedEventArgs e)
        {
            tbName.SelectAll();
            tbName.Focus(FocusState.Programmatic);
        }

        private void tbName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ViewModel.Save();
            }
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ConfirmDelete(LocalizedResources.GetString("MessageDeleteYear_Body"), LocalizedResources.GetString("MessageDeleteYear_Title")))
            {
                ViewModel.Delete();
            }
        }
    }
}
