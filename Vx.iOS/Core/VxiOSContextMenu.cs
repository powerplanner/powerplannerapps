using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using Vx.Views;

namespace Vx.iOS
{
    public static class VxiOSContextMenu
    {
        public static UIMenu CreateMenu(ContextMenu menu)
        {
            return CreateMenu(menu.Items);
        }

        private static UIMenu CreateMenu(IEnumerable<IContextMenuItem> contextMenuItems)
        {
            UIMenuElement[] actions = CreateMenuElements(contextMenuItems);
            return UIMenu.Create(actions);
        }

        private static UIMenuElement[] CreateMenuElements(IEnumerable<IContextMenuItem> contextMenuItems)
        {
            if (contextMenuItems.OfType<ContextMenuSeparator>().Any())
            {
                List<UIMenuElement> groupedActions = new List<UIMenuElement>();
                foreach (var g in SeparatedIntoGroups(contextMenuItems))
                {
                    groupedActions.Add(UIMenu.Create("", null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, CreateNonSeparatedMenuElements(g)));
                }
                return groupedActions.ToArray();
            }
            else
            {
                return CreateNonSeparatedMenuElements(contextMenuItems);
            }
        }

        private static IEnumerable<IContextMenuItem[]> SeparatedIntoGroups(IEnumerable<IContextMenuItem> flatList)
        {
            List<IContextMenuItem> currGroup = new List<IContextMenuItem>();
            foreach (var item in flatList)
            {
                if (item is ContextMenuSeparator)
                {
                    if (currGroup.Count > 0)
                    {
                        yield return currGroup.ToArray();
                    }
                    currGroup = new List<IContextMenuItem>();
                }
                else
                {
                    currGroup.Add(item);
                }
            }
            if (currGroup.Count > 0)
            {
                yield return currGroup.ToArray();
            }
        }

        private static UIMenuElement[] CreateNonSeparatedMenuElements(IEnumerable<IContextMenuItem> contextMenuItems)
        {
            var actions = new List<UIMenuElement>();

            foreach (var item in contextMenuItems)
            {
                var el = CreateMenuElement(item);
                if (el != null)
                {
                    actions.Add(el);
                }
            }

            return actions.ToArray();
        }

        private static UIMenuElement CreateMenuElement(IContextMenuItem item)
        {
            if (item is ContextMenuItem cmItem)
            {
                var a = UIAction.Create(cmItem.Text, cmItem.Glyph.GlyphToUIImage(), null, _ => cmItem.Click?.Invoke());
                if (cmItem.Style == ContextMenuItemStyle.Destructive)
                {
                    a.Attributes = UIMenuElementAttributes.Destructive;
                }
                return a;
            }
            else if (item is ContextMenuRadioItem rItem)
            {
                return UIAction.Create(rItem.Text, rItem.IsChecked ? UIImage.GetSystemImage("checkmark") : null, null, _ => rItem.Click?.Invoke());
            }
            else if (item is ContextMenuSubItem cmItemGroup)
            {
                return UIMenu.Create(cmItemGroup.Text, cmItemGroup.Glyph.GlyphToUIImage(), UIMenuIdentifier.None, default(UIMenuOptions), CreateMenuElements(cmItemGroup.Items));
            }

            return null;
        }
    }
}

