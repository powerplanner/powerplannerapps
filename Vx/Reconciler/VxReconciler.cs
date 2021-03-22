using System;
using System.Collections.Generic;
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
