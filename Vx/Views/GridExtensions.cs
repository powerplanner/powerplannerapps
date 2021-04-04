using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public static class GridExtensions
    {
        public static T Row<T>(this T view, int value) where T : BindableObject
        {
            Grid.SetRow(view, value);
            return view;
        }

        public static T Column<T>(this T view, int value) where T : BindableObject
        {
            Grid.SetColumn(view, value);
            return view;
        }

        public static T RowSpan<T>(this T view, int value) where T : BindableObject
        {
            Grid.SetRowSpan(view, value);
            return view;
        }

        public static T ColumnSpan<T>(this T view, int value) where T : BindableObject
        {
            Grid.SetColumnSpan(view, value);
            return view;
        }
    }
}
