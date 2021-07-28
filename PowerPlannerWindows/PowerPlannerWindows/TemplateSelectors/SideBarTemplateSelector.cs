using PowerPlannerAppDataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.TemplateSelectors
{
    public class SideBarTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalTemplate { get; set; }
        public DataTemplate SettingsTemplate { get; set; }
        public DataTemplate ClassesTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is NavigationManager.MainMenuSelections)
            {
                NavigationManager.MainMenuSelections selection = (NavigationManager.MainMenuSelections)item;

                if (selection == NavigationManager.MainMenuSelections.Classes)
                    return ClassesTemplate;

                return NormalTemplate;
            }

            return base.SelectTemplateCore(item);
        }
    }
}
