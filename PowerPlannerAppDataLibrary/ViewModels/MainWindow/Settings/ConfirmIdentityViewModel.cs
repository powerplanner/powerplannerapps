using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ConfirmIdentityViewModel : BaseViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        private AccountDataItem _currAccount;
        public event EventHandler OnIdentityConfirmed;
        public event EventHandler ActionIncorrectPassword;

        public ConfirmIdentityViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            _currAccount = account;

            if (_currAccount == null)
            {
                throw new InvalidOperationException("There's no current account.");
            }
        }


        public string Password { get; set; } = "";

        public void Continue()
        {
            if (PowerPlannerAuth.ValidatePasswordLocally(Password, _currAccount.Username, _currAccount.LocalToken))
            {
                OnIdentityConfirmed?.Invoke(this, new EventArgs());

                RemoveViewModel();
            }

            else
            {
                ActionIncorrectPassword?.Invoke(this, new EventArgs());
            }
        }
    }
}
