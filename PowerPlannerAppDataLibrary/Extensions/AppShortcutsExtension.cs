using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class AppShortcutsExtension
    {
        public static AppShortcutsExtension Current { get; set; }

        public abstract Task UpdateAsync(bool showAddItemShortcuts);
    }
}
