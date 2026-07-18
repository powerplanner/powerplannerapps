using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.DataLayer;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SyncOptionsViewModel : PopupComponentViewModel
    {
        public AccountDataItem Account { get; private set; }

        public SyncOptionsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = R.S("Settings_SyncOptions_Title");
            Account = MainScreenViewModel.CurrentAccount;
        }

        protected override void RegisterPropertySubscriptions()
        {
            base.RegisterPropertySubscriptions();
            Subscribe(Account);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(new Thickness(0, Theme.Current.PageMargin - 12, 0, Theme.Current.PageMargin - 12),
                RenderOption(
                    MaterialDesign.MaterialDesignIcons.Upload,
                    PowerPlannerResources.GetString("Settings_SyncOptions_ItemImageUploadOptions_DisplayName"),
                    ImageUploadOptionsViewModel.OptionToString(Account.ImageUploadOption),
                    OpenImageUploadOptions),

                RenderOption(
                    MaterialDesign.MaterialDesignIcons.CloudSync,
                    PowerPlannerResources.GetString("Settings_SyncOptions_ItemPushNotifications_DisplayName"),
                    BoolToEnabledStringConverter.Convert(!Account.IsPushDisabled),
                    OpenPushSettings)
            );
        }

        private View RenderOption(string icon, string title, string subtitle, Action action)
        {
            return SettingsListViewModel.RenderOption(icon, title, subtitle, action);
        }

        private void OpenImageUploadOptions()
        {
            ShowViewModel(parent => new ImageUploadOptionsViewModel(parent));
        }

        private void OpenPushSettings()
        {
            ShowViewModel(parent => new PushSettingsViewModel(parent));
        }

        private void ShowViewModel(Func<BaseViewModel, BaseViewModel> createViewModel)
        {
            ShowPopup(createViewModel(this));
        }
    }
}
