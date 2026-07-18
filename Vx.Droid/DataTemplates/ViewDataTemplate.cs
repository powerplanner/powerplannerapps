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
    public class ViewDataTemplate<TView> : IDataTemplate where TView : View, IDataContextView
    {
        private readonly Func<Context, TView> _createView;

        public ViewDataTemplate(Func<Context, TView> createView)
        {
            _createView = createView ?? throw new ArgumentNullException(nameof(createView));
        }

        public View CreateView(object dataContext, ViewGroup root)
        {
            TView view = _createView(root.Context);
            view.DataContext = dataContext;
            return view;
        }
    }
}