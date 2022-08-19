using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Droid.Views;
using Vx.Views;

namespace Vx.Droid
{
    public static class VxDroidExtensions
    {
        private static List<Tuple<Func<Vx.Views.View, bool>, Func<Vx.Views.View, NativeView>>> _customViews = new List<Tuple<Func<Vx.Views.View, bool>, Func<Vx.Views.View, NativeView>>>();

        public static void RegisterCustomView(Func<Vx.Views.View, bool> predicate, Func<Vx.Views.View, NativeView> create)
        {
            _customViews.Add(new Tuple<Func<Vx.Views.View, bool>, Func<Vx.Views.View, NativeView>>(predicate, create));
        }

        /// <summary>
        /// Parent app must initialize this
        /// </summary>
        public static Context ApplicationContext { get; set; }

        static VxDroidExtensions()
        {
            Theme.Current = new VxDroidTheme();
            VxPlatform.Current = Platform.Android;

            NativeView.CreateNativeView = view =>
            {
                foreach (var customView in _customViews)
                {
                    if (customView.Item1(view))
                    {
                        return customView.Item2(view);
                    }
                }

                if (view is Vx.Views.VxComponent c)
                {
                    return new DroidVxComponent(c);
                }

                if (view is Vx.Views.TextBlock)
                {
                    return new DroidTextBlock();
                }

                if (view is Vx.Views.LinearLayout)
                {
                    return new DroidLinearLayout();
                }

                if (view is Vx.Views.Button)
                {
                    if (view is Vx.Views.TextButton)
                    {
                        return new DroidTextButton();
                    }

                    if (view is Vx.Views.AccentButton)
                    {
                        return new DroidAccentButton();
                    }

                    return new DroidButton();
                }

                if (view is Vx.Views.TextBox)
                {
                    if (view is Vx.Views.MultilineTextBox)
                    {
                        return new DroidMultilineTextBox();
                    }

                    return new DroidTextBox();
                }

                if (view is Vx.Views.FontIcon)
                {
                    return new DroidFontIcon();
                }

                if (view is Vx.Views.ListItemButton)
                {
                    return new DroidListItemButton();
                }

                if (view is Vx.Views.TransparentContentButton)
                {
                    return new DroidTransparentContentButton();
                }

                if (view is Vx.Views.ScrollView)
                {
                    return new DroidScrollView();
                }

                if (view is Vx.Views.NumberTextBox)
                {
                    return new DroidNumberTextBox();
                }

                if (view is Vx.Views.Switch)
                {
                    return new DroidSwitch();
                }

                if (view is Vx.Views.CheckBox)
                {
                    return new DroidCheckBox();
                }

                if (view is Vx.Views.Border)
                {
                    return new DroidBorder();
                }

                if (view is Vx.Views.ComboBox)
                {
                    return new DroidComboBox();
                }

                if (view is Vx.Views.TimePicker)
                {
                    return new DroidTimePicker();
                }

                if (view is Vx.Views.DatePicker)
                {
                    return new DroidDatePicker();
                }

                if (view is Vx.Views.ColorPicker)
                {
                    return new DroidColorPicker();
                }

                if (view is Vx.Views.SlideView)
                {
                    return new DroidSlideView();
                }

                if (view is Vx.Views.FrameLayout)
                {
                    return new DroidFrameLayout();
                }

                if (view is Vx.Views.ListView)
                {
                    return new DroidListView();
                }

                if (view is Vx.Views.FloatingActionButton)
                {
                    return new DroidFloatingActionButton();
                }

#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new NotImplementedException("Unknown view. Droid hasn't implemented this.");
            };

            NativeView.ShowContextMenu = ShowContextMenu;
        }

        private static void ShowContextMenu(Vx.Views.ContextMenu contextMenu, Vx.Views.View view)
        {
            var menu = new PopupMenu(ApplicationContext, view.NativeView.View as Android.Views.View);

            var contextMenuItems = contextMenu.Items.OfType<ContextMenuItem>().ToArray();

            var menuItems = new List<IMenuItem>();
            foreach (var item in contextMenuItems)
            {
                menuItems.Add(menu.Menu.Add(item.Text));
            }

            menu.MenuItemClick += (e, args) =>
            {
                int index = menuItems.IndexOf(args.Item);
                contextMenuItems[index].Click?.Invoke();
            };

            menu.Show();
        }

        public static Android.Views.View Render(this VxComponent component)
        {
            if (component.NativeComponent != null)
            {
                return component.NativeComponent as DroidNativeComponent;
            }

            var nativeComponent = new DroidNativeComponent(ApplicationContext, component);
            component.InitializeForDisplay(nativeComponent);
            return nativeComponent;
        }

        internal static Android.Views.View CreateDroidView(this Vx.Views.View view, Vx.Views.View parentView)
        {
            return view.CreateNativeView(parentView).View as Android.Views.View;
        }

        internal static Android.Graphics.Color ToDroid(this Color color)
        {
            return new Android.Graphics.Color(color.ToArgb());
        }

        internal static GravityFlags ToDroid(this HorizontalAlignment horizontalAlignment, int width)
        {
            // If MatchParent, using gravity flags just messes things up
            if (width == ViewGroup.LayoutParams.MatchParent)
            {
                return (GravityFlags)(-1);
            }

            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    return GravityFlags.Start;

                case HorizontalAlignment.Center:
                    return GravityFlags.CenterHorizontal;

                case HorizontalAlignment.Right:
                    return GravityFlags.End;

                default:
                    return GravityFlags.FillHorizontal;
            }
        }

        internal static GravityFlags ToDroid(this VerticalAlignment verticalAlignment, int height)
        {
            // If MatchParent, using gravity flags just messes things up
            if (height == ViewGroup.LayoutParams.MatchParent)
            {
                return (GravityFlags)(-1);
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Center:
                    return GravityFlags.CenterVertical;

                case VerticalAlignment.Bottom:
                    return GravityFlags.Bottom;

                case VerticalAlignment.Stretch:
                    return GravityFlags.FillVertical;

                default:
                    return GravityFlags.Top;
            }
        }
    }
}