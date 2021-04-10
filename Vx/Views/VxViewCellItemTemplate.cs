using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    internal class VxViewCellItemTemplate<T> : DataTemplate
    {
        public VxViewCellItemTemplate(Func<T, View> render) : base(() => LoadTemplate(render))
        {
        }

        private static ViewCell LoadTemplate(Func<T, View> render)
        {
            return new ViewCell
            {
                View = new ViewCellComponent(render)
                {
                    IsRootComponent = true
                }
            };
        }

        private class ViewCellComponent : VxBindingComponent<T>
        {
            private Func<T, View> _render;
            public ViewCellComponent(Func<T, View> render)
            {
                _render = render;
            }

            protected override View Render()
            {
                return _render(BindingContext);
            }
        }
    }

    internal class VxViewCellItemTemplateComponent<T, V> : DataTemplate where V : VxBindingComponent<T>
    {
        public VxViewCellItemTemplateComponent() : base(() => LoadTemplate())
        {
        }

        private static ViewCell LoadTemplate()
        {
            var comp = Activator.CreateInstance<V>();
            comp.IsRootComponent = true;

            return new ViewCell
            {
                View = comp
            };
        }
    }
}
