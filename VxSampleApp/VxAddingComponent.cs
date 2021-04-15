using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx;
using Vx.Views;

namespace VxSampleApp
{
    public class VxAddingComponent : VxComponent
    {
        private VxState<string[]> _items = new VxState<string[]>(new string[0]);
        protected override View Render()
        {
            var sl = new LinearLayout();

            foreach (var item in _items.Value)
            {
                sl.Children.Add(new TextBlock { Text = item });
            }

            sl.Children.Add(new Button
            {
                Text = "Add item",
                Click = () => _items.Value = _items.Value.Concat(new string[] { "Item " + (_items.Value.Length + 1) }).ToArray()
            });

            if (_items.Value.Length > 0)
            {
                sl.Children.Add(new Button
                {
                    Text = "Remove item",
                    Click = () => _items.Value = _items.Value.Take(_items.Value.Length - 1).ToArray()
                });
            }

            return sl;
        }
    }
}
