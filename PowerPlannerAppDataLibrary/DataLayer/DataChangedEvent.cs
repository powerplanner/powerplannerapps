using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class DataChangedEvent
    {
        public bool WasLocalChanges { get; private set; }

        public Guid LocalAccountId { get; private set; }

        private List<BaseDataItem> _newItems;
        public IEnumerable<BaseDataItem> NewItems
        {
            get { return _newItems; }
        }

        private List<BaseDataItem> _editedItems;
        public IEnumerable<BaseDataItem> EditedItems
        {
            get { return _editedItems; }
        }

        private DeletedItems _deletedItems;
        public DeletedItems DeletedItems
        {
            get { return _deletedItems; }
        }

        public DataChangedEvent(Guid localAccountId, IEnumerable<BaseDataItem> newItems, IEnumerable<BaseDataItem> editedItems, DeletedItems deletedItems, bool wasLocalChanges)
        {
            LocalAccountId = localAccountId;
            _newItems = newItems.ToList();
            _editedItems = editedItems.ToList();
            _deletedItems = deletedItems;
            WasLocalChanges = wasLocalChanges;
        }

        public void Merge(DataChangedEvent newerEvent)
        {
            // Previously new or edited items might have been deleted, so make sure they're removed
            foreach (var del in newerEvent.DeletedItems)
            {
                // Try remove from new items
                if (!RemoveMatching(_newItems, del))
                {
                    // Otherwise try remove from edited items
                    RemoveMatching(_editedItems, del);
                }
            }

            // Add the deletes
            _deletedItems.Merge(newerEvent._deletedItems);

            // Merge the edits
            foreach (var edited in newerEvent.EditedItems)
            {
                ProcessMergingEditedItem(edited);
            }

            // And then add the new items (never will have conflicts since item can only be added once)
            _newItems.AddRange(newerEvent._newItems);
        }

        private void ProcessMergingEditedItem(BaseDataItem editedItem)
        {
            // If we already have this as a new item, then the item replaces it
            if (MergeIntoList(_newItems, editedItem))
                return;

            // Otherwise if we already have this as an edited item, then the new item replaces it
            if (MergeIntoList(_editedItems, editedItem))
                return;

            // Otherwise, we add it to edited items
            _editedItems.Add(editedItem);
        }

        private static bool MergeIntoList(List<BaseDataItem> listToMergeInto, BaseDataItem itemToMerge)
        {
            for (int i = 0; i < listToMergeInto.Count; i++)
            {
                // If we already have this item
                if (listToMergeInto[i].Identifier == itemToMerge.Identifier)
                {
                    // Replace it with the item to merge
                    listToMergeInto[i] = itemToMerge;
                    return true;
                }
            }

            return false;
        }

        private static bool RemoveMatching(List<BaseDataItem> listToRemoveFrom, Guid identifier)
        {
            for (int i = 0; i < listToRemoveFrom.Count; i++)
                if (listToRemoveFrom[i].Identifier == identifier)
                {
                    listToRemoveFrom.RemoveAt(i);
                    return true;
                }

            return false;
        }
    }
}
