using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SoundEffectsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;
        private VxState<bool> _isOn;
        private VxState<bool> _saving = new VxState<bool>();

        public SoundEffectsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_Sound.Title").ToUpper();
            _account = MainScreenViewModel.CurrentAccount;
            _isOn = new VxState<bool>(_account.IsSoundEffectsEnabled);
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("Settings_SoundEffects_Description.Text")
                        },

                        new Switch
                        {
                            Title = PowerPlannerResources.GetString("Settings_MainPage_Sound.Title"),
                            IsOn = VxValue.Create(_isOn.Value, IsOnChanged),
                            IsEnabled = !_saving.Value,
                            Margin = new Thickness(0, 24, 0, 0)
                        }
                    }
                }
            };
        }

        private async void IsOnChanged(bool isOn)
        {
            _isOn.Value = isOn;

            if (_account.IsSoundEffectsEnabled == isOn)
            {
                return;
            }

            _saving.Value = true;

            try
            {
                _account.IsSoundEffectsEnabled = isOn;
                await AccountsManager.Save(_account);
            }
            catch
            {
                // Revert the change
                _account.IsSoundEffectsEnabled = !isOn;
                _isOn.Value = !isOn;
            }
            finally
            {
                _saving.Value = false;
            }
        }
    }
}
