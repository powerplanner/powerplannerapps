using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using System.Reflection;
using PowerPlannerAppDataLibrary.DataLayer;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    internal class ViewItemPropertyAttribute : Attribute
    {

    }

    public interface IViewItemChildrenHelper
    {
        IEnumerable<BaseViewItem> GetChildren();

        bool HandleChanges(DataChangedEvent e);

        bool FilterAndAddChildren(IEnumerable<BaseDataItem> potentialChildren);

        Type GetViewItemType();
        Type GetDataItemType();
    }

    public class ViewItemChildrenHelper<D, V> : IViewItemChildrenHelper where D : BaseDataItem where V : BaseViewItem
    {
        public IEnumerable<V> Children { get; private set; }

        public Func<D, bool> IsChild { get; internal set; }

        public Func<D, V> CreateChildMethod { get; private set; }

        public Action<V> AddChildMethod { get; private set; }

        public Action<V> RemoveChildMethod { get; private set; }
        
        public ViewItemChildrenHelper(Func<D, bool> isChild, Action<V> addMethod, Action<V> removeMethod, Func<D, V> createChildMethod, IEnumerable<V> children)
        {
            CreateChildMethod = createChildMethod;
            AddChildMethod = addMethod;
            RemoveChildMethod = removeMethod;
            Children = children;
            IsChild = isChild;
        }

        public bool HandleChanges(DataChangedEvent e)
        {
            bool changed = false;

            List<V> toRemove = new List<V>();
            List<V> toReSort = new List<V>();

            foreach (V child in Children)
            {
                // If it was deleted, then we mark it for remove
                if (e.DeletedItems.Contains(child.Identifier))
                    toRemove.Add(child);

                else
                {
                    D edited = e.EditedItems.OfType<D>().FirstOrDefault(i => i.Identifier == child.Identifier);

                    // If it was edited
                    if (edited != null)
                    {
                        // If it's still a child, we'll need to re-sort it
                        if (IsChild.Invoke(edited))
                            toReSort.Add(child);

                        // Otherwise it's no longer a child, so remove it from this parent
                        else
                            toRemove.Add(child);
                    }
                }
            }

            if (toRemove.Count > 0 || toReSort.Count > 0)
                changed = true;

            // Now remove all that need removing
            foreach (V item in toRemove)
                RemoveChildMethod.Invoke(item);

            // And re-sort all that need re-sorting
            foreach (V item in toReSort)
            {
                // First remove
                RemoveChildMethod.Invoke(item);

                // Then re-add
                AddChildMethod.Invoke(item);
            }


            // And also add items that currently aren't children, but were edited and may be new children
            if (FilterAndAddChildren(e.EditedItems.Where(i => !Children.Any(c => c.Identifier == i.Identifier))))
                changed = true;

            // And now add the new items
            if (FilterAndAddChildren(e.NewItems))
                changed = true;

            return changed;
        }

        public bool FilterAndAddChildren(IEnumerable<BaseDataItem> potentialChildren)
        {
            bool changed = false;

            foreach (D item in potentialChildren.OfType<D>())
            {
                if (IsChild.Invoke(item))
                {
                    var createdChild = CreateChildMethod.Invoke(item);

                    if (createdChild != null)
                    {
                        AddChildMethod.Invoke(createdChild);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        public IEnumerable<BaseViewItem> GetChildren()
        {
            return Children;
        }

        public Type GetViewItemType()
        {
            return typeof(V);
        }

        public Type GetDataItemType()
        {
            return typeof(D);
        }
    }

    public abstract class BaseViewItem : BaseViewItemWithoutData, IComparable, IComparable<BaseViewItem>
    {
        public BaseDataItem DataItem { get; private set; }

        public AccountDataItem Account
        {
            get { return DataItem?.Account; }
        }

        public bool HasData
        {
            get { return DataItem != null; }
        }

        public BaseViewItem(Guid identifier)
        {
            Identifier = identifier;
        }

        public BaseViewItem(BaseDataItem dataItem)
        {
            if (dataItem != null)
            {
                DataItem = dataItem;

                Identifier = dataItem.Identifier;
                DateCreated = dataItem.DateCreated;

                PopulateFromDataItemOverride(dataItem);
            }
        }

        public void PopulateFromDataItem(BaseDataItem dataItem)
        {
            if (HasData)
            {
                DataItem = dataItem;
                PopulateFromDataItemOverride(dataItem);
            }
        }

        protected virtual void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            Updated = dataItem.Updated;
        }

        public Guid Identifier { get; private set; }

        public DateTime DateCreated { get; set; }

        public DateTime Updated { get; set; }
        

        public virtual int CompareTo(BaseViewItem other)
        {
            return this.DateCreated.CompareTo(other.DateCreated);
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is BaseViewItem)
                return CompareTo(obj as BaseViewItem);

            return 0;
        }
    }
}
