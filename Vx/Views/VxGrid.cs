using System;
using System.Collections.Generic;
using System.Text;

namespace Vx.Views
{
    public interface IVxGrid
    {
        VxView[] Children { set; }

        VxRowDefinition[] RowDefinitions { set; }

        VxColumnDefinition[] ColumnDefinitions { set; }
    }

    public class VxGrid : VxView
    {
        public VxView[] Children
        {
            get => GetProperty<VxView[]>();
            set => SetProperty(value);
        }

        public VxRowDefinition[] RowDefinitions
        {
            get => GetProperty<VxRowDefinition[]>();
            set => SetProperty(value);
        }

        public VxColumnDefinition[] ColumnDefinitions
        {
            get => GetProperty<VxColumnDefinition[]>();
            set => SetProperty(value);
        }
    }

    public static class VxGridExtensions
    {
        private const string GridRowProperty = "Grid.Row";

        public static T Children<T>(this T grid, params VxView[] value) where T : VxGrid
        {
            grid.Children = value;
            return grid;
        }

        public static T RowDefinitions<T>(this T grid, params VxRowDefinition[] value) where T : VxGrid
        {
            grid.RowDefinitions = value;
            return grid;
        }

        public static T ColumnDefinitions<T>(this T grid, params VxColumnDefinition[] value) where T : VxGrid
        {
            grid.ColumnDefinitions = value;
            return grid;
        }
    }

    public class VxColumnDefinition
    {
        public VxColumnDefinition()
        {
            Width = new VxGridLength(1, VxGridUnitType.Star);
        }

        public VxColumnDefinition(VxGridLength width)
        {
            Width = width;
        }

        public VxGridLength Width { get; set; }
    }

    public class VxRowDefinition
    {
        public VxRowDefinition()
        {
            Height = new VxGridLength(1, VxGridUnitType.Star);
        }

        public VxRowDefinition(VxGridLength height)
        {
            Height = height;
        }

        public VxGridLength Height { get; set; }
    }

    public struct VxGridLength
    {
        public VxGridLength(double pixels)
        {
            Value = pixels;
            GridUnitType = VxGridUnitType.Pixel;
            IsAbsolute = true;
            IsStar = false;
            IsAuto = false;
        }
        public VxGridLength(double value, VxGridUnitType type)
        {
            Value = value;
            GridUnitType = type;
            IsAbsolute = type == VxGridUnitType.Pixel;
            IsAuto = type == VxGridUnitType.Auto;
            IsStar = type == VxGridUnitType.Star;
        }

        public static VxGridLength Auto { get; private set; } = new VxGridLength(0, VxGridUnitType.Auto);
        public static VxGridLength Star(double starValue)
        {
            return new VxGridLength(starValue, VxGridUnitType.Star);
        }
        public static VxGridLength Pixel(double pixelValue)
        {
            return new VxGridLength(pixelValue, VxGridUnitType.Pixel);
        }
        public VxGridUnitType GridUnitType { get; }
        public bool IsAbsolute { get; }
        public bool IsAuto { get; }
        public bool IsStar { get; }
        public double Value { get; }
    }

    public enum VxGridUnitType
    {
        Auto = 0,
        Pixel = 1,
        Star = 2
    }
}
