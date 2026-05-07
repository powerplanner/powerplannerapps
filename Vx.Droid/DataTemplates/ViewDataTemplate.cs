using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class ViewDataTemplate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TView> : IDataTemplate where TView : View
    {
        [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "TView has DynamicallyAccessedMembers annotation preserving public constructors and properties.")]
        [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "TView has DynamicallyAccessedMembers annotation preserving public constructors and properties.")]
        public View CreateView(object dataContext, ViewGroup root)
        {
            View view = (View)Activator.CreateInstance(typeof(TView), args: new object[] { root.Context });

            var dataContextProperty = view.GetType().GetProperty("DataContext");
            if (dataContextProperty != null)
            {
                dataContextProperty.SetValue(view, dataContextProperty);
            }

            return view;
        }
    }
}