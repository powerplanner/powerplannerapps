using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.NativeViews
{
    public class VxDroidGrid : VxDroidNativeView<VxGrid, GridLayout>, IVxGrid
    {
        protected override void Initialize()
        {
            base.Initialize();
        }

        public VxView[] Children { set => SetListOfViewsOnViewGroup(value); }

        public VxRowDefinition[] RowDefinitions
        {
            set
            {
                if (value == null)
                {
                    NativeView.RowCount = 1;
                }
                else
                {
                    NativeView.RowCount = Math.Max(value.Length, 1);
                }
            }
        }

        public VxColumnDefinition[] ColumnDefinitions
        {
            set
            {
                if (value == null)
                {
                    NativeView.ColumnCount = 1;
                }
                else
                {
                    NativeView.ColumnCount = Math.Max(value.Length, 1);
                }
            }
        }
    }
}