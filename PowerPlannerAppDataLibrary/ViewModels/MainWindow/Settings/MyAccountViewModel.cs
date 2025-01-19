using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class MyAccountViewModel : PopupComponentViewModel
    {
        private static DateTime _timeLastConfirmed = DateTime.MinValue;
        private static Guid _lastConfrimedAccountId;

        [VxSubscribe]
        public AccountDataItem CurrentAccount { get; private set; }

        private VxState<string> _email = new VxState<string>(R.S("String_Loading"));
        private VxState<bool?> _emailVerified = new VxState<bool?>(null);

        private VxState<bool?> _sendingVerification = new VxState<bool?>(null);

        private MyAccountViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_MyAccount_Header.Text");
        }

        protected override async void Initialize()
        {
            ChangeEmailViewModel.EmailChanged += new WeakEventHandler<string>(ChangeEmailViewModel_EmailChanged).Handler;

            if (CurrentAccount.IsOnlineAccount)
            {
                try
                {
                    var resp = await ChangeEmailViewModel.GetEmailAsync(CurrentAccount);
                    _email.Value = resp.Item1;
                    _emailVerified.Value = resp.Item2;
                }
                catch (Exception ex)
                {
                    _email.Value = ex.Message;
                }
            }
        }

        private void ChangeEmailViewModel_EmailChanged(object sender, string e)
        {
            try
            {
                _email.Value = e;
                _emailVerified.Value = false;
                _sendingVerification.Value = false;
            }
            catch { }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = R.S("TextBox_Username.Header") + ":",
                            FontWeight = FontWeights.Bold,
                            WrapText = false
                        },

                        new TextBlock
                        {
                            Text = CurrentAccount.Username,
                            IsTextSelectionEnabled = true,
                            Margin = new Thickness(4, 0, 0, 0)
                        }.LinearLayoutWeight(1),

                    }
                },

                CurrentAccount.IsOnlineAccount ? new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = R.S("Settings_ChangeEmailPage_TextBoxEmail.Header") + ":",
                            FontWeight = FontWeights.Bold,
                            WrapText = false
                        },

                        new TextBlock
                        {
                            Text = _email.Value,
                            IsTextSelectionEnabled = true,
                            Margin = new Thickness(4, 0, 0, 0)
                        }.LinearLayoutWeight(1),

                    }
                } : null,

                (_emailVerified.Value != null && _emailVerified.Value.Value == false) ? new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = R.S("Settings_MyAccount_UnverifiedHeader"), // "Unverified:"
                            FontWeight = FontWeights.Bold,
                            TextColor = Color.Red,
                            WrapText = false,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0,0,6,0)
                        },

                        _sendingVerification.Value != null ? (View)new TextBlock
                        {
                            Text = _sendingVerification.Value.Value ? R.S("Settings_MyAccount_SendingVerification") : R.S("Settings_MyAccount_SentVerification"),
                            WrapText = false
                        } : new TextButton
                        {
                            Text = R.S("Settings_MyAccount_ResendVerification"), // "Resend verification email."
                            Click = ResendVerificationEmail,
                            IsEnabled = _sendingVerification.Value == null
                        }
                    }
                } : null,

                new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_ButtonLogOut.Content"),
                    Click = LogOut,
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_ButtonChangeUsername.Content"),
                    Click = ChangeUsername,
                    Margin = new Thickness(0, 12, 0, 0)
                },

                new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_ButtonChangePassword.Content"),
                    Click = ChangePassword,
                    Margin = new Thickness(0, 12, 0, 0)
                },

                CurrentAccount.IsOnlineAccount ? new Button
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_ButtonChangeEmail.Content"),
                    Click = ChangeEmail,
                    Margin = new Thickness(0, 12, 0, 0)
                } : null,

                new CheckBox
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_CheckBoxRememberUsername.Content"),
                    IsChecked = VxValue.Create(RememberUsername, v => RememberUsername = v),
                    Margin = new Thickness(0, 24, 0, 0)
                },

                new CheckBox
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_CheckBoxRememberPassword.Content"),
                    IsChecked = VxValue.Create(RememberPassword, v => RememberPassword = v),
                    IsEnabled = CurrentAccount.IsRememberPasswordPossible,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                new CheckBox
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_CheckBoxAutoLogin.Content"),
                    IsChecked = VxValue.Create(AutoLogin, v => AutoLogin = v),
                    IsEnabled = CurrentAccount.IsAutoLoginPossible,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                CurrentAccount.IsOnlineAccount ? null : new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_ButtonConvertToOnline.Content"),
                    Margin = new Thickness(0, 24, 0, 0),
                    Click = ConvertToOnline
                },

                new AccentButton
                {
                    Text = PowerPlannerResources.GetString("Settings_MyAccount_ButtonDeleteAccount.Content"),
                    Margin = new Thickness(0, CurrentAccount.IsOnlineAccount ? 24 : 12, 0, 0),
                    Click = PromptConfirmDelete
                }
            );
        }

        private async void ResendVerificationEmail()
        {
            _sendingVerification.Value = true;

            try
            {
                var response = await CurrentAccount.PostAuthenticatedAsync<PartialLoginRequest, PlainResponse>(
                    Website.ClientApiUrl + "resendverificationemail",
                    new PartialLoginRequest());

                if (response.Error != null)
                {
                    new PortableMessageDialog(response.Error, "Error").Show();
                    _sendingVerification.Value = null;
                }
                else
                {
                    _sendingVerification.Value = false;
                }
            }
            catch (Exception ex)
            {
                new PortableMessageDialog(ex.Message, "Error").Show();
                _sendingVerification.Value = null;
            }
        }

        public static MyAccountViewModel Load(BaseViewModel parent)
        {
            MainWindowViewModel windowViewModel = parent.FindAncestor<MainWindowViewModel>();

            if (windowViewModel == null)
            {
                throw new NullReferenceException("Could not find MainWindowViewModel ancestor");
            }

            if (windowViewModel.CurrentAccount == null)
            {
                throw new InvalidOperationException("There's no current account.");
            }

            return new MyAccountViewModel(parent)
            {
                CurrentAccount = windowViewModel.CurrentAccount,
                _rememberUsername = windowViewModel.CurrentAccount.RememberUsername,
                _rememberPassword = windowViewModel.CurrentAccount.RememberPassword,
                _autoLogin = windowViewModel.CurrentAccount.AutoLogin
            };
        }

        private bool _rememberUsername;
        public bool RememberUsername
        {
            get { return _rememberUsername; }
            set
            {
                if (_rememberUsername != value)
                {
                    _rememberUsername = value;
                    OnPropertyChanged(nameof(RememberUsername));
                }

                if (CurrentAccount.RememberUsername != value)
                {
                    CurrentAccount.RememberUsername = value;
                    SaveChanges();
                }
            }
        }

        private bool _rememberPassword;
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set
            {
                if (_rememberPassword != value)
                {
                    _rememberPassword = value;
                    OnPropertyChanged(nameof(RememberPassword));
                }

                if (CurrentAccount.RememberPassword != value)
                {
                    CurrentAccount.RememberPassword = value;
                    SaveChanges();
                }
            }
        }

        private bool _autoLogin;
        public bool AutoLogin
        {
            get { return _autoLogin; }
            set
            {
                if (_autoLogin != value)
                {
                    _autoLogin = value;
                    OnPropertyChanged(nameof(AutoLogin));
                }

                if (CurrentAccount.AutoLogin != value)
                {
                    CurrentAccount.AutoLogin = value;
                    SaveChanges();
                }
            }
        }

        private async void SaveChanges()
        {
            try
            {
                await AccountsManager.Save(CurrentAccount);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public async void LogOut()
        {
            AccountsManager.SetLastLoginIdentifier(Guid.Empty);
            await FindAncestor<MainWindowViewModel>().SetCurrentAccount(null);
        }

        public void ChangeUsername()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ChangeUsernameViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ChangePassword()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ChangePasswordViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ChangeEmail()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ChangeEmailViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void PromptConfirmDelete()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new DeleteAccountViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ConvertToOnline()
        {
            ConfirmIdentityAndThen(delegate
            {
                ShowPopup(new ConvertToOnlineViewModel(GetPopupViewModelHost(), CurrentAccount));
            });
        }

        public void ConfirmIdentityAndThen(Action action)
        {
            if (!NeedsConfirmIdentity())
            {
                action.Invoke();
                return;
            }

            var confirmViewModel = new ConfirmIdentityViewModel(GetPopupViewModelHost(), CurrentAccount);
            confirmViewModel.OnIdentityConfirmed += delegate { _lastConfrimedAccountId = CurrentAccount.LocalAccountId; _timeLastConfirmed = DateTime.Now; action.Invoke(); };
            ShowPopup(confirmViewModel);
        }

        private bool NeedsConfirmIdentity()
        {
            // If we've loaded whether email is verified, and it is NOT verified, we can allow the user to change their details without first confirming
            if (_emailVerified.Value != null && _emailVerified.Value == false)
            {
                return false;
            }

            if (_lastConfrimedAccountId == CurrentAccount.LocalAccountId && _timeLastConfirmed.AddMinutes(10) > DateTime.Now)
            {
                return false;
            }

            return true;
        }

        public async System.Threading.Tasks.Task DeleteAccount(bool deleteOnlineToo)
        {
            if (CurrentAccount.IsOnlineAccount)
            {
                if (deleteOnlineToo)
                {
                    try
                    {
                        DeleteAccountResponse resp = await CurrentAccount.PostAuthenticatedAsync<DeleteAccountRequest, DeleteAccountResponse>(Website.ClientApiUrl + "deleteaccountmodern", new DeleteAccountRequest());

                        if (resp.Error != null)
                        {
                            await new PortableMessageDialog(resp.Error, PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync();
                        }

                        else
                        {
                            deleteAndFinish(CurrentAccount);
                        }
                    }

                    catch { await new PortableMessageDialog(PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_UnknownErrorDeletingOnline"), PowerPlannerResources.GetString("Settings_DeleteAccountPage_Errors_ErrorDeletingHeader")).ShowAsync(); }

                    finally { }
                }

                //otherwise just remove device
                else
                {
                    //no need to check whether delete device succeeded
                    try { var dontWait = CurrentAccount.PostAuthenticatedAsync<DeleteDevicesRequest, DeleteDevicesResponse>(Website.ClientApiUrl + "deletedevicesmodern", new DeleteDevicesRequest() { DeviceIdsToDelete = new List<int>() { CurrentAccount.DeviceId } }); }

                    catch { }

                    deleteAndFinish(CurrentAccount);
                }
            }

            else
            {
                deleteAndFinish(CurrentAccount);
            }
        }

        private async void deleteAndFinish(AccountDataItem account)
        {
            await AccountsManager.Delete(account.LocalAccountId);
        }
    }
}
