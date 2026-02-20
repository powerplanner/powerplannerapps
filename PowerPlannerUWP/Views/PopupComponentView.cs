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
    public partial class PopupComponentView : PopupViewHostGeneric
    {
        public new PopupComponentViewModel ViewModel
        {
            get => base.ViewModel as PopupComponentViewModel;
            set => base.ViewModel = value;
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateTitle();
            UpdateCommands();
            UpdateSecondaryCommands();

            MainContent = ViewModel.Render();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Commands):
                    UpdateCommands();
                    break;

                case nameof(ViewModel.SecondaryCommands):
                    UpdateSecondaryCommands();
                    break;

                case nameof(ViewModel.Title):
                    UpdateTitle();
                    break;
            }
        }

        private void UpdateCommands()
        {
            PrimaryCommands.Clear();
            if (ViewModel.Commands != null)
            {
                foreach (var c in ViewModel.Commands.Where(i => i != null))
                {
                    PrimaryCommands.Add(c);
                }
            }
        }

        private void UpdateSecondaryCommands()
        {
            SecondaryCommands.Clear();

            if (ViewModel.SecondaryCommands != null)
            {
                foreach (var c in ViewModel.SecondaryCommands.Where(i => i != null))
                {
                    SecondaryCommands.Add(c);
                }
            }
        }

        private void UpdateTitle()
        {
            Title = ViewModel.Title?.ToUpper();
        }
    }
}
