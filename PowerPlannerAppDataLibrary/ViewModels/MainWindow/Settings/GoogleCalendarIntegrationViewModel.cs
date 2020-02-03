using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class GoogleCalendarIntegrationViewModel : BaseViewModel
    {
        public const string Url = Website.ROOT_URL + "googlecalendar";

        public long AccountId => Account.AccountId;
        public string Username => Account.Username;
        public string Session => Account.Token;

        public const string CookieAccountId = "AccountId";
        public const string CookieUsername = "Username";
        public const string CookieSession = "Session";

        public IEnumerable<KeyValuePair<string, string>> GetCookies()
        {
            return new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(CookieAccountId, AccountId.ToString()),
                new KeyValuePair<string, string>(CookieUsername, Username),
                new KeyValuePair<string, string>(CookieSession, Session)
            };
        }

        public AccountDataItem Account { get; private set; }

        public GoogleCalendarIntegrationViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;
        }
    }
}
