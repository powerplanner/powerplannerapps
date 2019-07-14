using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class LocalizationExtension
    {
        public static LocalizationExtension Current { get; set; }

        public abstract string GetString(string id);

        public string GetStringAsLowercaseWithParameters(string id, params object[] insertParams)
        {
            string str = GetString(id);

            str = str.ToLower();
            str = string.Format(str, insertParams);

            return str;
        }
    }
}
