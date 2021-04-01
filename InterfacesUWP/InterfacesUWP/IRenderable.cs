using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace InterfacesUWP
{
    public interface IRenderable
    {
        UIElement Render();
    }
}
