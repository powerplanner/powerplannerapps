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
    public class CustomDataTemplate : IDataTemplate
    {
        public Func<ViewGroup, object, View> CreateViewAction { get; private set; }

        public CustomDataTemplate(Func<ViewGroup, object, View> createViewAction)
        {
            CreateViewAction = createViewAction;
        }

        public View CreateView(object dataContext, ViewGroup root)
        {
            return CreateViewAction(root, dataContext);
        }
    }

    public class CustomDataTemplate<TItemType> : IDataTemplate
    {
        public Func<ViewGroup, TItemType, View> CreateViewAction { get; private set; }

        public CustomDataTemplate(Func<ViewGroup, TItemType, View> createViewAction)
        {
            CreateViewAction = createViewAction;
        }

        public View CreateView(object dataContext, ViewGroup root)
        {
            TItemType casted = default(TItemType);

            if (dataContext != null)
                casted = (TItemType)dataContext;

            return CreateViewAction(root, casted);
        }
    }
}