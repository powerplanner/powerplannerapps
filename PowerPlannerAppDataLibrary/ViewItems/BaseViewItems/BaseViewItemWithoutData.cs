using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemWithoutData : BindableBase
    {
        private List<IViewItemChildrenHelper> _children = new List<IViewItemChildrenHelper>();

        internal void AddChildrenHelper(IViewItemChildrenHelper children)
        {
            lock (this)
            {
                _children.Add(children);
            }
        }

        internal void FilterAndAddChildren<D>(IEnumerable<D> potentialChildren) where D : BaseDataItem
        {
            // Get the children handler
            IViewItemChildrenHelper childrenHandler;
            lock (this)
            {
                childrenHandler = _children.FirstOrDefault(i => i.GetDataItemType().Equals(typeof(D)));
            }

            if (childrenHandler == null)
            {
                throw new NullReferenceException($"Children handler for child type {typeof(D)} wasn't found.");
            }
            
            // Add the children
            childrenHandler.FilterAndAddChildren(potentialChildren);
        }

        internal void UpdateIsChildMethod<D, V>(Func<D, bool> isChildMethod, D[] newPotentialChildren) where D : BaseDataItem, new() where V : BaseViewItem
        {
            // Get the children handler
            ViewItemChildrenHelper<D, V> childrenHandler;
            lock (this)
            {
                childrenHandler = _children.OfType<ViewItemChildrenHelper<D, V>>().FirstOrDefault();
            }

            if (childrenHandler == null)
                throw new NullReferenceException("Children handler for this child type didn't exist");

            childrenHandler.IsChild = isChildMethod;

            FilterAndAddChildren(newPotentialChildren);
        }

        internal virtual bool HandleDataChangedEvent(DataChangedEvent e)
        {
            bool changed = false;

            // Apply the edits to self and descendants
            if (HandleUpdatingSelfAndDescendants(e))
            {
                changed = true;
            }

            // Modify the children
            if (HandleModifyingChildrenRecursively(e))
            {
                changed = true;
            }

            if (HandleDataChangedEventAfterBase(e))
                changed = true;

            return changed;
        }

        private bool HandleUpdatingSelfAndDescendants(DataChangedEvent e)
        {
            bool changed = false;

            // First edit this item itself
            if (this is BaseViewItem && (this as BaseViewItem).HasData)
            {
                BaseViewItem baseViewItem = this as BaseViewItem;

                BaseDataItem editedItem = e.EditedItems.FirstOrDefault(i => i.Identifier == baseViewItem.Identifier);

                if (editedItem != null)
                {
                    baseViewItem.PopulateFromDataItem(editedItem);

                    changed = true;
                }
            }

            IViewItemChildrenHelper[] childrenHelpers;
            lock (this)
            {
                childrenHelpers = _children.ToArray();
            }

            // Then edit all children
            foreach (var child in childrenHelpers.SelectMany(i => i.GetChildren()))
            {
                if (child.HandleUpdatingSelfAndDescendants(e))
                    changed = true;
            }

            return changed;
        }

        private bool HandleModifyingChildrenRecursively(DataChangedEvent e)
        {
            IViewItemChildrenHelper[] childrenHelpers;
            lock (this)
            {
                childrenHelpers = _children.ToArray();
            }

            bool changed = false;

            // Make sure that children are in the right place (and add existing/new)
            foreach (var childrenHandler in childrenHelpers)
            {
                if (childrenHandler.HandleChanges(e))
                    changed = true;
            }

            // And do the same for children recursively
            foreach (var child in childrenHelpers.SelectMany(i => i.GetChildren()))
            {
                if (child.HandleModifyingChildrenRecursively(e))
                {
                    changed = true;
                }
            }

            return changed;
        }

        protected virtual bool HandleDataChangedEventAfterBase(DataChangedEvent e)
        {
            // Nothing, top-level classes can choose to implement this
            return false;
        }
    }
}
