using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace InterfacesUWP
{
    public class UIGenerator
    {
        private static Dictionary<IRenderable, LinkedList<UIElement>> _renderedObjects = new Dictionary<IRenderable, LinkedList<UIElement>>();

        public static UIElement Render(IRenderable renderable)
        {
            UIElement el = renderable.Render();

            if (_renderedObjects.ContainsKey(renderable))
                _renderedObjects[renderable].AddLast(el);
            else
                _renderedObjects[renderable] = new LinkedList<UIElement>(new UIElement[] { el });

            return el;
        }

        public static List<UIElement> GetRenderedElements(IRenderable renderable)
        {
            LinkedList<UIElement> list = null;

            if (_renderedObjects.TryGetValue(renderable, out list))
                return list.ToList();

            return new List<UIElement>();
        }

        public static void RemoveRenderedElement(IRenderable renderable, UIElement el)
        {
            _renderedObjects[renderable].Remove(el);
        }
    }
}
