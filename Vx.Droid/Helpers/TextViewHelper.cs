using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Helpers
{
    public static class TextViewHelper
    {
        public static bool GetStrikethrough(this TextView tv)
        {
            return tv.PaintFlags.HasFlag(PaintFlags.StrikeThruText);
        }

        public static void SetStrikethrough(this TextView tv, bool strikethrough)
        {
            if (strikethrough)
            {
                tv.PaintFlags = tv.PaintFlags | PaintFlags.StrikeThruText;
            }
            else
            {
                tv.PaintFlags = tv.PaintFlags & (~PaintFlags.StrikeThruText);
            }
        }
    }
}