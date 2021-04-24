using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;

namespace InterfacesDroid.DataTemplates
{
    public class ResourceDataTemplate : IDataTemplate
    {
        public int ResourceId { get; private set; }

        public ResourceDataTemplate(int resourceId)
        {
            ResourceId = resourceId;
        }

        public View CreateView(object dataContext, ViewGroup root)
        {
            return new InflatedViewWithBinding(ResourceId, root)
            {
                DataContext = dataContext
            };
        }
    }
}