using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.NativeViews
{
    public class VxUwpGrid : VxUwpNativeView<VxGrid, Grid>, IVxGrid
    {
        public VxUwpGrid(VxGrid grid) : base(grid) { }

        public VxView[] Children { set => SetListOfViewsOnCollection(value, NativeView.Children); }
        public VxRowDefinition[] RowDefinitions
        {
            set
            {
                NativeView.RowDefinitions.Clear();

                if (value != null)
                {
                    foreach (var def in value)
                    {
                        NativeView.RowDefinitions.Add(new RowDefinition()
                        {
                            Height = Convert(def.Height)
                        });
                    }
                }
            }
        }

        public VxColumnDefinition[] ColumnDefinitions
        {
            set
            {
                NativeView.ColumnDefinitions.Clear();

                if (value != null)
                {
                    foreach (var def in value)
                    {
                        NativeView.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = Convert(def.Width)
                        });
                    }
                }
            }
        }

        private static Windows.UI.Xaml.GridLength Convert(VxGridLength vxGridLength)
        {
            Windows.UI.Xaml.GridUnitType gridUnitType;
            switch (vxGridLength.GridUnitType)
            {
                case VxGridUnitType.Auto:
                    gridUnitType = Windows.UI.Xaml.GridUnitType.Auto;
                    break;

                case VxGridUnitType.Pixel:
                    gridUnitType = Windows.UI.Xaml.GridUnitType.Pixel;
                    break;

                case VxGridUnitType.Star:
                    gridUnitType = Windows.UI.Xaml.GridUnitType.Star;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return new Windows.UI.Xaml.GridLength(vxGridLength.Value, gridUnitType);
        }
    }
}
