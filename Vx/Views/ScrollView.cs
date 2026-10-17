using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public class ScrollView : View
    {
        public ScrollView() { }

        public ScrollView(View content)
        {
            Content = content;
        }

        public View Content { get; set; }

        public bool HorizontalScrollEnabled { get; set; }

        public bool ZoomEnabled { get; set; }
    }
}
