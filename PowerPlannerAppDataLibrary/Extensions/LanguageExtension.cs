using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class LanguageExtension
    {
        public static LanguageExtension Current { get; set; }

        public abstract string GetLanguageOverrideCode();

        public abstract void SetLanguageOverrideCode(string code);
    }
}
