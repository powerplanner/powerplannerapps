using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    [DataContract(Namespace = "")]
    public class ChangedPropertiesOfDataItem
    {
        public enum ChangeType { New, Edited, Deleted }

        [DataMember]
        public Guid Identifier { get; set; }
        
        [DataMember]
        public Dictionary<BaseDataItem.SyncPropertyNames, bool> _changedProperties = new Dictionary<BaseDataItem.SyncPropertyNames, bool>();
        

        public ChangeType Type
        {
            get
            {
                if (_changedProperties.ContainsKey(BaseDataItem.SyncPropertyNames.Deleted))
                    return ChangeType.Deleted;

                if (_changedProperties.ContainsKey(BaseDataItem.SyncPropertyNames.All))
                    return ChangeType.New;

                return ChangeType.Edited;
            }
        }

        public bool IsEmpty()
        {
            return _changedProperties.Count == 0;
        }

        public void SetNew()
        {
            // If it was deleted
            if (Type == ChangeType.Deleted)
            {
                // Remove that deleted flag
                _changedProperties.Remove(BaseDataItem.SyncPropertyNames.Deleted);
            }

            _changedProperties.Clear();
            _changedProperties[BaseDataItem.SyncPropertyNames.All] = false;
        }

        public void SetEdited(IEnumerable<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            // If it was deleted
            if (Type == ChangeType.Deleted)
            {
                // Remove that deleted flag
                _changedProperties.Remove(BaseDataItem.SyncPropertyNames.Deleted);
            }

            // If adding nothing, do nothing
            if (!changedProperties.Any())
                return;

            // If already has All, we still need to write these properties since All might currently be syncing and finish, but these edited properties need to sync afterwards

            // Cannot add deleted property through this method
            if (changedProperties.Contains(BaseDataItem.SyncPropertyNames.Deleted))
                throw new ArgumentException("The Deleted property cannot be added through here");

            // Cannot add All property through this method
            if (changedProperties.Contains(BaseDataItem.SyncPropertyNames.All))
                throw new ArgumentException("The All property cannot be added through here");

            
            // Add each one
            foreach (var p in changedProperties)
                _changedProperties[p] = false;
        }

        /// <summary>
        /// Returns true if change was actually made. Returns false if already deleted and thus no change made.
        /// </summary>
        /// <returns></returns>
        public void SetDeleted()
        {
            _changedProperties.Clear();
            _changedProperties[BaseDataItem.SyncPropertyNames.Deleted] = false;
        }

        /// <summary>
        /// Returns true if made changes, otherwise false
        /// </summary>
        /// <returns></returns>
        internal bool ClearSyncing()
        {
            BaseDataItem.SyncPropertyNames[] toClear = _changedProperties.Where(i => i.Value).Select(i => i.Key).ToArray();
            
            foreach (var p in toClear)
                _changedProperties.Remove(p);

            return toClear.Length > 0;
        }

        internal bool NeedsClearSyncing()
        {
            return _changedProperties.Any(i => i.Value);
        }

        /// <summary>
        /// Marks all values as sent
        /// </summary>
        internal void MarkSent()
        {
            var toSetAsSent = _changedProperties.Where(i => !i.Value).Select(i => i.Key).ToArray();

            foreach (var p in toSetAsSent)
                _changedProperties[p] = true;
        }

        public BaseDataItem.SyncPropertyNames[] GetEditedProperties()
        {
            return _changedProperties.Keys.ToArray();
        }
    }
}
