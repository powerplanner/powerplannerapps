using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerUWP
{
    /// <summary>
    /// TODO: I should deprecate this!!!!
    /// </summary>
    [Obsolete]
    public class QueryStringHelper : QueryString
    {
        public const string LOCAL_ACCOUNT_ID = "localAccountId";
        public const string ACTION = "action";
        public const string IDENTIFIER = "identifier";

        public QueryStringHelper SetLocalAccountId(Guid localAccountId)
        {
            this.Set(LOCAL_ACCOUNT_ID, localAccountId.ToString());
            return this;
        }

        public QueryStringHelper SetAction(string action)
        {
            this.Set(ACTION, action);
            return this;
        }

        public QueryStringHelper SetIdentifier(Guid identifier)
        {
            this.Set(IDENTIFIER, identifier.ToString());
            return this;
        }
    }
}
