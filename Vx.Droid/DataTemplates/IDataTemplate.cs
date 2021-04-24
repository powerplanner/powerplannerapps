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

namespace InterfacesDroid.DataTemplates
{
    public interface IDataTemplate
    {
        View CreateView(object dataContext, ViewGroup root);
    }
}