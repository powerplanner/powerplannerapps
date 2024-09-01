using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public enum ImageUploadOptions
    {
        /// <summary>
        /// This means "Only use networks not marked as limited/low data networks"... Cellular might be allowed.
        /// </summary>
        WifiOnly,

        /// <summary>
        /// This means "Use all networks, even low data / data capped ones"
        /// </summary>
        Always,

        Never
    }
}
