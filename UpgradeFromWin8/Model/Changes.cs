using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class Changes
    {
        [DataMember]
        public Dictionary<Guid, ChangedItem> _changedItems = new Dictionary<Guid, ChangedItem>();


        /// <summary>
        /// Removes all the items that have been sent.
        /// </summary>
        public void ReturnFromSync()
        {
            List<Guid> toRemove = new List<Guid>();

            foreach (var pair in _changedItems)
                if (pair.Value.HasBeenSent)
                    toRemove.Add(pair.Key);

            foreach (Guid id in toRemove)
                _changedItems.Remove(id);
        }

        /// <summary>
        /// Adds to Updated and removes from Deleted
        /// </summary>
        /// <param name="item"></param>
        public void Add(BaseItemWin item)
        {
            ChangedItem index = grabIndex(item);

            index.SetUpdated();
        }

        private ChangedItem grabIndex(BaseItemWin item)
        {
            return grabIndex(item.Identifier);
        }

        private ChangedItem grabIndex(Guid identifier)
        {
            ChangedItem index = null;

            _changedItems.TryGetValue(identifier, out index);

            if (index == null)
            {
                index = new ChangedItem();
                _changedItems[identifier] = index;
            }

            return index;
        }

        /// <summary>
        /// Adds to Deleted, and removes from Updated
        /// </summary>
        /// <param name="item"></param>
        public void Delete(Guid identifier, DateTime timeDeleted)
        {
            ChangedItem index = grabIndex(identifier);

            index.SetDeleted(timeDeleted);
        }

        /// <summary>
        /// Clears EVERYTHING
        /// </summary>
        public void ClearAll()
        {
            _changedItems.Clear();
        }

        /// <summary>
        /// Returns all the updated items
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public UpdatedItems Serialize(AccountWin account)
        {
            UpdatedItems answer = new UpdatedItems();

            //foreach (var pair in _changedItems)
            //{
            //    if (pair.Value.IsUpdated)
            //    {
            //        //mark it sent
            //        pair.Value.HasBeenSent = true;

            //        //find its item
            //        BaseItemWin entity = account.Find(pair.Key);
            //        if (entity == null)
            //            continue;

            //        //add the serialized item to the list
            //        answer.Add(entity.Serialize());
            //    }
            //}

            return answer;
        }

        //public IEnumerable<BaseItem> Serialize(AccountWin account, int offset)
        //{
        //    List<BaseItem> answer = new List<BaseItem>();

        //    foreach (var pair in _changedItems)
        //    {
        //        if (pair.Value.IsUpdated)
        //        {
        //            //mark it sent
        //            pair.Value.HasBeenSent = true;

        //            //find its item
        //            BaseItemWP entity = account.AccountSection.Find(pair.Key);
        //            if (entity == null)
        //                continue;

        //            //add the serialized item to the list
        //            answer.Add(entity.Serialize(offset));
        //        }
        //    }

        //    return answer;
        //}

        public Dictionary<Guid, DateTime> DeletedItemsAsDictionary()
        {
            Dictionary<Guid, DateTime> answer = new Dictionary<Guid, DateTime>();

            foreach (var pair in _changedItems)
            {
                if (pair.Value.IsDeleted)
                {
                    //mark sent
                    pair.Value.HasBeenSent = true;

                    //add to dictionary
                    answer.Add(pair.Key, pair.Value.DeletedOn);
                }
            }

            return answer;
        }
    }
}
