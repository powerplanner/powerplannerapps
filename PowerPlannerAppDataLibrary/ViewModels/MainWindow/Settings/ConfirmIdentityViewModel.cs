﻿using BareMvvm.Core.ViewModels;
using PowerPlannerAppAuthLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
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
        public override bool ImportantForAutofill => true;

        private AccountDataItem _currAccount;
        public event EventHandler OnIdentityConfirmed;
        public event EventHandler ActionIncorrectPassword;

        public bool ShowForgotPassword { get; private set; }

        public ConfirmIdentityViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            _currAccount = account;

            if (_currAccount == null)
            {
                throw new InvalidOperationException("There's no current account.");
            }

            ShowForgotPassword = account.IsOnlineAccount;
        }

        private string _password = "";

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, nameof(Password)); }
        }

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

        public void ForgotPassword()
        {
            ShowPopup(new ResetPasswordViewModel(GetPopupViewModelHost(), _currAccount.Username));
        }
    }
}
