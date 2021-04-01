using BareMvvm.Core.App;
using BareMvvm.Forms.ViewModelPresenters;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace BareMvvm.Forms.App
{
    public abstract class FormsApp : PortableApp
    {
        public FormsApp()
        {
            // Register the view model to view mappings
            foreach (var mapping in GetViewModelToViewMappings())
            {
                ViewModelToViewConverter.AddMapping(mapping.Key, mapping.Value);
            }

            // Register the obtain dispatcher function
            PortableDispatcher.ObtainDispatcherFunction = () => { return new FormsDispatcher(); };

            // Register message dialog
            //PortableMessageDialog.Extension = UwpMessageDialog.ShowAsync;

            //PortableLocalizedResources.CultureExtension = GetCultureInfo;
        }

        public abstract Dictionary<Type, Type> GetViewModelToViewMappings();

        public abstract Type GetAppShellType();

        public static new FormsApp Current { get => PortableApp.Current as FormsApp; }
    }
}
