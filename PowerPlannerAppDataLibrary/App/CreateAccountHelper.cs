using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.App
{
    public static class CreateAccountHelper
    {
        public static Action AlertUsernameExistsLocally = delegate { ShowMessage("Username already exists on this device.", "Username exists"); };
        public static Action AlertInvalidUsername = delegate { ShowMessage("Your username is invalid.", "Invalid username"); };
        public static Action AlertUsernameEmpty = delegate { ShowMessage("You must provide a username!", "No username"); };

        public static async Task<AccountDataItem> CreateAccountLocally(string username, string localToken, string token, long accountId, int deviceId, bool needsInitialSync)
        {
            try
            {
                AccountDataItem account = account = await AccountsManager.CreateAccount(username, localToken, token, accountId, deviceId, true, true, true, needsInitialSync);

                return account;
            }

            catch (AccountsManager.UsernameExistsLocallyException)
            {
                AlertUsernameExistsLocally?.Invoke();
            }

            catch (AccountsManager.UsernameInvalidException)
            {
                AlertInvalidUsername?.Invoke();
            }

            catch (AccountsManager.UsernameWasEmptyException)
            {
                AlertUsernameEmpty?.Invoke();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex, "UnknownCreateAccountLocallyError");
                ShowMessage("Unknown error occurred. Your error has been sent to the developer.\n\n" + ex.ToString(), "Error");
            }

            return null;
        }

        private static async void ShowMessage(string message, string title)
        {
            await new PortableMessageDialog(message, title).ShowAsync();
        }
    }
}
