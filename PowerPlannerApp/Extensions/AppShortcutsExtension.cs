using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerApp.Extensions
{
    public abstract class AppShortcutsExtension
    {
        public static AppShortcutsExtension Current { get; set; }

        public abstract Task UpdateAsync(bool showAddItemShortcuts);
    }
}
