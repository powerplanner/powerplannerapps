using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.ViewModel")]
    public class Changes
    {
        [DataMember]
        public Dictionary<Guid, ChangedItem> _changedItems = new Dictionary<Guid, ChangedItem>();
        

        private ChangedItem grabIndex(BaseItemWP item)
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
        /// Returns all the updated items
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public UpdatedItems Serialize(AccountWP account, int offset)
        {
            UpdatedItems answer = new UpdatedItems();

            foreach (var pair in _changedItems)
            {
                if (pair.Value.IsUpdated)
                {
                    //mark it sent
                    pair.Value.HasBeenSent = true;

                    //find its item
                    BaseItemWP entity = account.AccountSection.Find(pair.Key);
                    if (entity == null)
                        continue;

                    //add the serialized item to the list
                    answer.Add(entity.Serialize(offset));
                }
            }

            return answer;
        }

        public Dictionary<Guid, DateTime> DeletedItemsAsDictionary(int offset)
        {
            Dictionary<Guid, DateTime> answer = new Dictionary<Guid, DateTime>();

            foreach (var pair in _changedItems)
            {
                if (pair.Value.IsDeleted)
                {
                    //mark sent
                    pair.Value.HasBeenSent = true;

                    //add to dictionary
                    answer.Add(pair.Key, pair.Value.DeletedOn.AddMilliseconds(offset));
                }
            }

            return answer;
        }
    }
}
