using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public class VxItemTemplate<T> : DataTemplate
    {
        public VxItemTemplate(Func<T, View> render) : base(() => LoadTemplate(render))
        {
        }

        private static View LoadTemplate(Func<T, View> render)
        {
            return new ViewCellComponent(render)
            {
                IsRootComponent = true
            };
        }

        private class ViewCellComponent : VxComponent
        {
            private Func<T, View> _render;
            public ViewCellComponent(Func<T, View> render)
            {
                _render = render;
            }

            protected override bool IsDependentOnBindingContext => true;

            protected override View Render()
            {
                return _render((T)BindingContext);
            }
        }
    }
}
