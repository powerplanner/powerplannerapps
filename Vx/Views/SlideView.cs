using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class SlideView : View
    {
        public VxValue<int> Position { get; set; }

        public Func<int, View> ItemTemplate { get; set; }
    }
}
