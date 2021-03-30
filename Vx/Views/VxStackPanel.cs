using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IVxStackPanel
    {
        VxView[] Children { set; }

        VxOrientation Orientation { set; }
    }

    public class VxStackPanel : VxView
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

    public static class VxStackPanelExtensions
    {
        public static T Children<T>(this T stackPanel, params VxView[] value) where T : VxStackPanel
        {
            stackPanel.Children = value;
            return stackPanel;
        }

        public static T Orientation<T>(this T stackPanel, VxOrientation value) where T : VxStackPanel
        {
            stackPanel.Orientation = value;
            return stackPanel;
        }
    }

    public enum VxOrientation
    {
        Vertical,
        Horizontal
    }
}
