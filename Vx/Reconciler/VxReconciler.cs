using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Reconciler
{
    public static class VxReconciler
    {
        //public static VxReconcilerChange Reconcile(VxView oldTree, VxView newTree)
        //{
        //    if (oldTree == null && newTree == null)
        //    {

        //    }

        //    if (oldTree.GetType() == newTree.GetType())
        //    {
        //        var propertyChanges = new List<VxReconcilerPropertyChange>();

        //        foreach (var oldProp in oldTree.Properties)
        //        {
        //            if (!newTree.Properties.ContainsKey(oldProp.Key))
        //            {
        //                propertyChanges.Add(new VxReconcilerPropertyRemoved(oldProp.Key));
        //            }
        //        }
        //    }
        //}

        public static List<VxReconcilerBaseListChange> ReconcileList(IList<VxView> oldList, IList<VxView> newList)
        {
            var answer = new List<VxReconcilerBaseListChange>();

            if (oldList == null || oldList.Count == 0)
            {
                int newI = 0;
                foreach (var n in newList)
                {
                    answer.Add(new VxReconcilerInsertListItem(newI, n));
                    newI++;
                }

                return answer;
            }

            int i = 0;
            VxView oldItem;
            VxView newItem;

            if (oldList.Count < newList.Count)
            {
                oldList = new List<VxView>(oldList);
            }

            for (; i < oldList.Count; i++)
            {
                oldItem = oldList[i];
                newItem = newList.ElementAtOrDefault(i);

                if (newItem == null)
                {
                    answer.Add(new VxReconcilerRemoveListItem(i));
                }
                else if (oldItem.GetType() == newItem.GetType())
                {
                    answer.Add(new VxReconcilerUpdateListItem(i, oldItem, newItem));
                }
                else if (oldList.Count < newList.Count)
                {
                    answer.Add(new VxReconcilerInsertListItem(i, newItem));
                    oldList.Insert(i, newItem);
                }
                else
                {
                    answer.Add(new VxReconcilerReplaceListItem(i, newItem));
                }
            }

            if (oldList.Count < newList.Count)
            {
                for (; i < newList.Count; i++)
                {
                    answer.Add(new VxReconcilerInsertListItem(i, newList[i]));
                }
            }

            return answer;
        }
    }

    public class VxReconcilerBaseListChange
    {
    }

    public class VxReconcilerUpdateListItem : VxReconcilerBaseListChange, IEquatable<VxReconcilerUpdateListItem>
    {
        public int Index { get; set; }

        public VxView OldView { get; set; }

        public VxView NewView { get; set; }

        public VxReconcilerUpdateListItem(int index, VxView oldView, VxView newView)
        {
            OldView = oldView;
            NewView = newView;
        }

        public bool Equals(VxReconcilerUpdateListItem other)
        {
            return Index == other.Index && object.ReferenceEquals(OldView, other.OldView) && object.ReferenceEquals(NewView, other.NewView);
        }

        public override bool Equals(object obj)
        {
            if (obj is VxReconcilerUpdateListItem other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VxReconcilerReplaceListItem : VxReconcilerBaseListChange, IEquatable<VxReconcilerReplaceListItem>
    {
        public int Index { get; set; }

        public VxView NewView { get; set; }

        public VxReconcilerReplaceListItem(int index, VxView newView)
        {
            Index = index;
            NewView = newView;
        }

        public bool Equals(VxReconcilerReplaceListItem other)
        {
            return Index == other.Index && object.ReferenceEquals(NewView, other.NewView);
        }

        public override bool Equals(object obj)
        {
            if (obj is VxReconcilerReplaceListItem other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VxReconcilerInsertListItem : VxReconcilerBaseListChange, IEquatable<VxReconcilerInsertListItem>
    {
        public int Index { get; set; }

        public VxView NewView { get; set; }

        public VxReconcilerInsertListItem(int index, VxView newView)
        {
            Index = index;
            NewView = newView;
        }

        public bool Equals(VxReconcilerInsertListItem other)
        {
            return Index == other.Index && object.ReferenceEquals(NewView, other.NewView);
        }

        public override bool Equals(object obj)
        {
            if (obj is VxReconcilerInsertListItem other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VxReconcilerRemoveListItem : VxReconcilerBaseListChange, IEquatable<VxReconcilerRemoveListItem>
    {
        public int Index { get; set; }

        public VxReconcilerRemoveListItem(int index)
        {
            Index = index;
        }

        public bool Equals(VxReconcilerRemoveListItem other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (obj is VxReconcilerRemoveListItem other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum VxReconcilerListChangeType
    {
        Insert,
        Remove,
        Replace,
        Update
    }

    public class VxReconcilerPropertyChange
    {
        public string PropertyName { get; set; }

        public VxReconcilerPropertyChange(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public class VxReconcilerPropertySet : VxReconcilerPropertyChange
    {
        public object NewValue { get; set; }

        public VxReconcilerPropertySet(string propertyName, object newValue) : base(propertyName)
        {
            NewValue = newValue;
        }
    }

    public class VxReconcilerPropertyRemoved : VxReconcilerPropertyChange
    {
        public VxReconcilerPropertyRemoved(string propertyName) : base(propertyName)
        {
        }
    }

    public class VxReconcilerChange
    {
        public VxChangeType ChangeType { get; set; }
    }

    public enum VxChangeType
    {
        Add,
        Replace,
        Remove
    }
}
