using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace InterfacesDroid.Views
{
    public class MyTextView : TextView
    {
        public MyTextView(Context context) : base(context)
        {
        }

        public MyTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public MyTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public MyTextView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected MyTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public Color TextColor
        {
            get { return new Color(base.TextColors.DefaultColor); }
            set { base.SetTextColor(value); }
        }
    }
}