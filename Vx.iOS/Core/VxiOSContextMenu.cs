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

        private static UIMenu CreateMenu(IEnumerable<IMenuItem> contextMenuItems)
        {
            UIMenuElement[] actions = CreateMenuElements(contextMenuItems);
            return UIMenu.Create(actions);
        }

        private static UIMenuElement[] CreateMenuElements(IEnumerable<IMenuItem> contextMenuItems)
        {
            if (contextMenuItems.OfType<MenuSeparator>().Any())
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

        private static IEnumerable<IMenuItem[]> SeparatedIntoGroups(IEnumerable<IMenuItem> flatList)
        {
            List<IMenuItem> currGroup = new List<IMenuItem>();
            foreach (var item in flatList)
            {
                if (item is MenuSeparator)
                {
                    if (currGroup.Count > 0)
                    {
                        yield return currGroup.ToArray();
                    }
                    currGroup = new List<IMenuItem>();
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

        private static UIMenuElement[] CreateNonSeparatedMenuElements(IEnumerable<IMenuItem> contextMenuItems)
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

        private static UIMenuElement CreateMenuElement(IMenuItem item)
        {
            if (item is MenuItem cmItem)
            {
                if (cmItem.SubItems != null && cmItem.SubItems.Any(i => i != null))
                {
                    return UIMenu.Create(cmItem.Text, cmItem.Glyph.GlyphToUIImage(), UIMenuIdentifier.None, default(UIMenuOptions), CreateMenuElements(cmItem.SubItems.Where(i => i != null)));
                }
                var a = UIAction.Create(cmItem.Text, cmItem.Glyph.GlyphToUIImage(), null, _ => cmItem.Click?.Invoke());
                if (cmItem.Style == MenuItemStyle.Destructive)
                {
                    a.Attributes = UIMenuElementAttributes.Destructive;
                }
                return a;
            }
            else if (item is MenuRadioItem rItem)
            {
                return UIAction.Create(rItem.Text, rItem.IsChecked ? UIImage.GetSystemImage("checkmark") : null, null, _ => rItem.Click?.Invoke());
            }

            return null;
        }
    }
}

