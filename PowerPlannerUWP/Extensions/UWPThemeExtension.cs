using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;

namespace PowerPlannerUWP.Extensions
{
    internal class UWPThemeExtension : ThemeExtension
    {
        public override async void Relaunch()
        {
            try
            {
                await CoreApplication.RequestRestartAsync("--updatedTheme");
            }
            catch { }
        }
    }
}
