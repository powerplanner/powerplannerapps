using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public static class VxViewGroupExtensions
    {
        public static ViewGroup VxChildren(this ViewGroup viewGroup, params View[] views)
        {
            foreach (var view in views)
            {
                viewGroup.AddView(view);
            }
            return viewGroup;
        }
    }

    public static class VxButtonExtensions
    {
        public static Button VxClick(this Button button, EventHandler action)
        {
            button.Click += action;
            return button;
        }
    }

    public static class VxSpinnerExtensions
    {
        /// <summary>
        /// Sets the currently selected item
        /// </summary>
        /// <param name="spinner"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Spinner VxSelection(this Spinner spinner, int index)
        {
            spinner.SetSelection(index);
            return spinner;
        }

        public static Spinner VxItemSelected(this Spinner spinner, EventHandler<AdapterView.ItemSelectedEventArgs> handler)
        {
            spinner.ItemSelected += handler;
            return spinner;
        }
    }
}