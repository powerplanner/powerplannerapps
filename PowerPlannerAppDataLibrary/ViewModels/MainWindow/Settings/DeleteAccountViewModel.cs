using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerSending;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class DeleteAccountViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public AccountDataItem Account { get; private set; }

        public DeleteAccountViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
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
            if (Account.IsOnlineAccount)
            {
                if (DeleteOnlineAccountToo)
                {
                    try
                    {
                        DeleteAccountResponse resp = await Account.PostAuthenticatedAsync<DeleteAccountRequest, DeleteAccountResponse>(Website.URL + "deleteaccountmodern", new DeleteAccountRequest());

                        if (resp.Error != null)
                        {
                            await new PortableMessageDialog(resp.Error, PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync();
                        }

                        else
                        {
                            deleteAndFinish();
                        }
                    }

                    catch { await new PortableMessageDialog(PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_UnknownErrorDeletingOnline"), PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync(); }
                }

                //otherwise just remove device
                else
                {
                    //no need to check whether delete device succeeded
                    try { var dontWait = Account.PostAuthenticatedAsync<DeleteDevicesRequest, DeleteDevicesResponse>(Website.URL + "deletedevicesmodern", new DeleteDevicesRequest() { DeviceIdsToDelete = new List<int>() { Account.DeviceId } }); }

                    catch { }

                    deleteAndFinish();
                }
            }

            else
            {
                deleteAndFinish();
            }
        }

        private async void deleteAndFinish()
        {
            await AccountsManager.Delete(Account.LocalAccountId);
            RemoveViewModel();
        }
    }
}
