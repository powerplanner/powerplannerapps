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

            Title = ViewModel.Title?.ToUpper();

            if (ViewModel.Commands != null)
            {
                foreach (var c in ViewModel.Commands)
                {
                    var nativeButton = new AppBarButton()
                    {
                        Label = c.Text,
                        Icon = new SymbolIcon(c.Glyph.ToUwpSymbol())
                    };
                    nativeButton.Click += delegate { c.Action(); };
                    PrimaryCommands.Add(nativeButton);
                }
            }

            MainContent = ViewModel.Render();
        }
    }
}
