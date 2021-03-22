using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp
{
    public static class VxViewExtensions
    {
        //public static UIElement Render(this VxView view)
        //{
        //    if (view is VxTextBlock textBlock)
        //    {
        //        return new TextBlock()
        //        {
        //            Text = textBlock.Text()
        //        };
        //    }

        //    else if (view is VxGrid grid)
        //    {
        //        var nativeGrid = new Grid()
        //        {

        //        };

        //        foreach (var child in grid.Children())
        //        {
        //            nativeGrid.Children.Add(child.Render());
        //        }

        //        return nativeGrid;
        //    }

        //    else if (view is VxTextBox tb)
        //    {
        //        return new TextBox()
        //        {
        //            Header = tb.Header()
        //        };
        //    }

        //    else if (view is VxButton button)
        //    {
        //        var b = new Button()
        //        {
        //            Content = button.Text()
        //        };

        //        b.Click += delegate { button.Click()(); };

        //        return b;
        //    }

        //    else if (view is VxComponent component)
        //    {
        //        return component.Render().Render();
        //    }

        //    return new TextBlock()
        //    {
        //        Text = "Unknown"
        //    };
        //}
    }
}
