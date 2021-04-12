using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;
using Xamarin.Forms;

namespace BareMvvm.Core.App
{
    public abstract class NativeFormsApplication : Application
    {
        public NativeFormsApplication()
        {


            // Initialize the app
            PortableApp.InitializeAsync((PortableApp)Activator.CreateInstance(GetPortableAppType()));
        }

        public abstract Type GetPortableAppType();
    }
}
