using PowerPlannerAppDataLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp;
using Windows.UI.Xaml.Controls;

namespace PowerPlannerUWP.Views
{
    public class PopupComponentView : PopupViewHostGeneric
    {
        public new PopupComponentViewModel ViewModel
        {
            get => base.ViewModel as PopupComponentViewModel;
            set => base.ViewModel = value;
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var primaryCommand = ViewModel.PrimaryCommand;
            if (primaryCommand != null)
            {
                var nativeButton = new Windows.UI.Xaml.Controls.AppBarButton()
                {
                    Label = primaryCommand.Text,
                    Icon = new SymbolIcon(Symbol.Accept)
                };
                nativeButton.Click += NativeButton_Click;
                PrimaryCommands.Add(nativeButton);
            }

            MainContent = ViewModel.Render();
        }

        private void NativeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.PrimaryCommand.Action?.Invoke();
        }
    }
}
