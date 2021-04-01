using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IVxLinearLayout
    {
        VxView[] Children { set; }

        VxOrientation Orientation { set; }
    }

    public class VxLinearLayout : VxView, IVxLinearLayout
    {
        public VxView[] Children
        {
            get => GetProperty<VxView[]>();
            set => SetProperty(value);
        }
        public VxOrientation Orientation
        {
            get => GetProperty<VxOrientation>();
            set => SetProperty(value);
        }
    }

    public static class VxLinearLayoutExtensions
    {
        public static T Children<T>(this T linearLayout, params VxView[] value) where T : VxLinearLayout
        {
            linearLayout.Children = value;
            return linearLayout;
        }

        public static T Orientation<T>(this T linearLayout, VxOrientation value) where T : VxLinearLayout
        {
            linearLayout.Orientation = value;
            return linearLayout;
        }
    }
}
