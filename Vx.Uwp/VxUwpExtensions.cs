﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp.Views;
using Vx.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Vx.Uwp
{
    public static class VxUwpExtensions
    {
        static VxUwpExtensions()
        {
            Theme.Current = new VxUwpTheme();
            VxPlatform.Current = Platform.Uwp;

            NativeView.CreateNativeView = view =>
            {
                if (view is Vx.Views.VxComponent c)
                {
                    return new UwpVxComponent(c);
                }

                if (view is Vx.Views.TextBlock)
                {
                    return new UwpTextBlock();
                }

                if (view is Vx.Views.LinearLayout)
                {
                    return new UwpLinearLayout();
                }

                if (view is Vx.Views.ListItemButton)
                {
                    return new UwpListItemButton();
                }

                if (view is Vx.Views.TransparentContentButton)
                {
                    return new UwpTransparentContentButton();
                }

                if (view is Vx.Views.Button)
                {
                    if (view is Vx.Views.TextButton)
                    {
                        return new UwpTextButton();
                    }
                    else if (view is Vx.Views.AccentButton)
                    {
                        return new UwpAccentButton();
                    }
                    else if (view is Vx.Views.DestructiveButton)
                    {
                        return new UwpDestructiveButton();
                    }
                    return new UwpButton();
                }

                if (view is Vx.Views.TextBox)
                {
                    if (view is Vx.Views.PasswordBox)
                    {
                        return new UwpPasswordBox();
                    }

                    return new UwpTextBox();
                }

                if (view is Vx.Views.FontIcon)
                {
                    return new UwpFontIcon();
                }

                if (view is Vx.Views.ScrollView)
                {
                    return new UwpScrollView();
                }

                if (view is Vx.Views.NumberTextBox)
                {
                    return new UwpNumberTextBox();
                }

                if (view is Vx.Views.Switch)
                {
                    return new UwpSwitch();
                }

                if (view is Vx.Views.CheckBox)
                {
                    return new UwpCheckBox();
                }

                if (view is Vx.Views.TimePicker)
                {
                    if (view is Vx.Views.EndTimePicker)
                    {
                        return new UwpEndTimePicker();
                    }

                    return new UwpTimePicker();
                }

                if (view is Vx.Views.ComboBox)
                {
                    return new UwpComboBox();
                }

                if (view is Vx.Views.Border)
                {
                    return new UwpBorder();
                }

                if (view is Vx.Views.DatePicker)
                {
                    return new UwpDatePicker();
                }

#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
#endif
                throw new NotImplementedException("Unknown view. UWP hasn't implemented this.");
            };

            NativeView.ShowContextMenu = ShowContextMenu;
        }

        private static void ShowContextMenu(ContextMenu contextMenu, View view)
        {
            var menuFlyout = new MenuFlyout();

            foreach (var item in contextMenu.Items)
            {
                var flyoutItem = new MenuFlyoutItem()
                {
                    Text = item.Text
                };

                flyoutItem.Click += delegate
                {
                    item.Click?.Invoke();
                };

                menuFlyout.Items.Add(flyoutItem);
            }

            menuFlyout.ShowAt(view.NativeView.View as FrameworkElement);
        }

        public static FrameworkElement Render(this VxComponent component)
        {
            if (component.NativeComponent != null)
            {
                return component.NativeComponent as UwpNativeComponent;
            }

            var nativeComponent = new UwpNativeComponent(component);
            component.InitializeForDisplay(nativeComponent);
            return nativeComponent;
        }

        internal static FrameworkElement CreateFrameworkElement(this View view)
        {
            if (view == null)
            {
                return null;
            }

            return view.CreateNativeView(null).View as FrameworkElement;
        }

        internal static Windows.UI.Color ToUwpColor(this System.Drawing.Color color)
        {
            return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        internal static SolidColorBrush ToUwpBrush(this System.Drawing.Color color)
        {
            return new SolidColorBrush(color.ToUwpColor());
        }

        internal static Windows.UI.Text.FontWeight ToUwp(this FontWeights fontWeight)
        {
            switch (fontWeight)
            {
                case FontWeights.Bold:
                    return Windows.UI.Text.FontWeights.Bold;

                case FontWeights.SemiBold:
                    return Windows.UI.Text.FontWeights.SemiBold;

                case FontWeights.SemiLight:
                    return Windows.UI.Text.FontWeights.SemiLight;

                default:
                    return Windows.UI.Text.FontWeights.Normal;
            }
        }

        internal static Windows.UI.Xaml.HorizontalAlignment ToUwp(this Vx.Views.HorizontalAlignment horizontalAlignment)
        {
            switch (horizontalAlignment)
            {
                case Vx.Views.HorizontalAlignment.Left:
                    return Windows.UI.Xaml.HorizontalAlignment.Left;

                case Vx.Views.HorizontalAlignment.Center:
                    return Windows.UI.Xaml.HorizontalAlignment.Center;

                case Vx.Views.HorizontalAlignment.Right:
                    return Windows.UI.Xaml.HorizontalAlignment.Right;

                default:
                    return Windows.UI.Xaml.HorizontalAlignment.Stretch;
            }
        }

        internal static TextAlignment ToUwpTextAlignment(this Vx.Views.HorizontalAlignment horizontalAlignment)
        {
            switch (horizontalAlignment)
            {
                case Vx.Views.HorizontalAlignment.Left:
                    return TextAlignment.Start;

                case Vx.Views.HorizontalAlignment.Center:
                    return TextAlignment.Center;

                case Vx.Views.HorizontalAlignment.Right:
                    return TextAlignment.End;

                default:
                    return TextAlignment.Start;
            }
        }

        internal static Windows.UI.Xaml.VerticalAlignment ToUwp(this Vx.Views.VerticalAlignment verticalAlignment)
        {
            switch (verticalAlignment)
            {
                case Vx.Views.VerticalAlignment.Center:
                    return Windows.UI.Xaml.VerticalAlignment.Center;

                case Vx.Views.VerticalAlignment.Stretch:
                    return Windows.UI.Xaml.VerticalAlignment.Stretch;

                case Vx.Views.VerticalAlignment.Bottom:
                    return Windows.UI.Xaml.VerticalAlignment.Bottom;

                default:
                    return Windows.UI.Xaml.VerticalAlignment.Top;
            }
        }

        internal static Windows.UI.Xaml.Thickness ToUwp(this Vx.Views.Thickness thickness)
        {
            return new Windows.UI.Xaml.Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }

        public static Symbol ToUwpSymbol(this string glyph)
        {
            switch (glyph)
            {
                case MaterialDesign.MaterialDesignIcons.Check:
                    return Symbol.Accept;

                case MaterialDesign.MaterialDesignIcons.Save:
                    return Symbol.Save;

                case MaterialDesign.MaterialDesignIcons.Delete:
                    return Symbol.Delete;

                default:
                    return Symbol.Refresh;
            }
        }
    }
}
