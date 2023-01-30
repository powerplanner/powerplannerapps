using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerSending;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class DeleteAccountViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public AccountDataItem Account { get; private set; }
        private VxState<bool> _isDeleting = new VxState<bool>();

        public DeleteAccountViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_DeleteAccountPage.Title");
            Account = account;
        }

        protected override View Render()
        {
            bool isEnabled = !_isDeleting.Value;

            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = PowerPlannerResources.GetString("Settings_DeleteAccountPage_Description.Text"),
                    FontSize = Theme.Current.TitleFontSize,
                    Margin = new Thickness(0, 0, 0, 24)
                },

                Account.IsOnlineAccount ? new CheckBox
                {
                    Text = PowerPlannerResources.GetString("Settings_DeleteAccountPage_CheckBoxDeleteOnlineToo.Content"),
                    IsChecked = VxValue.Create(DeleteOnlineAccountToo, v => DeleteOnlineAccountToo = v),
                    Margin = new Thickness(0, 0, 0, 12),
                    IsEnabled = isEnabled
                } : null,

                new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_DeleteAccountPage_ButtonConfirmDelete.Content"),
                    Click = () => _ = DeleteAsync(),
                    IsEnabled = isEnabled
                }
            );
        }

        private bool _deleteOnlineAccountToo;
        public bool DeleteOnlineAccountToo
        {
            get { return _deleteOnlineAccountToo; }
            set { SetProperty(ref _deleteOnlineAccountToo, value, nameof(DeleteOnlineAccountToo)); }
        }

        /// <summary>
        /// This permanently deletes without any confirmation.
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task DeleteAsync()
        {
            _isDeleting.Value = true;

            try
            {
                if (Account.IsOnlineAccount)
                {
                    if (DeleteOnlineAccountToo)
                    {
                        try
                        {
                            DeleteAccountResponse resp = await Account.PostAuthenticatedAsync<DeleteAccountRequest, DeleteAccountResponse>(Website.ClientApiUrl + "deleteaccountmodern", new DeleteAccountRequest());

                            if (resp.Error != null)
                            {
                                await new PortableMessageDialog(resp.Error, PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync();
                            }

                            else
                            {
                                await deleteAndFinish();
                            }
                        }

                        catch { await new PortableMessageDialog(PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_UnknownErrorDeletingOnline"), PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync(); }
                    }

                    //otherwise just remove device
                    else
                    {
                        //no need to check whether delete device succeeded
                        try { var dontWait = Account.PostAuthenticatedAsync<DeleteDevicesRequest, DeleteDevicesResponse>(Website.ClientApiUrl + "deletedevicesmodern", new DeleteDevicesRequest() { DeviceIdsToDelete = new List<int>() { Account.DeviceId } }); }

                        catch { }

                        await deleteAndFinish();
                    }
                }

                else
                {
                    await deleteAndFinish();
                }
            }
            finally
            {
                _isDeleting.Value = false;
            }
        }

        private async System.Threading.Tasks.Task deleteAndFinish()
        {
            await AccountsManager.Delete(Account.LocalAccountId);
            RemoveViewModel();
        }
    }
}
