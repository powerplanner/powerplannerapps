using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model
{
    /// <summary>
    /// Using JSON to serialize will make this take up very little space. PropertyName enums will be numbers. It'll be very minimal.
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public class PartialChanges
    {
        [DataMember]
        public Dictionary<Guid, BaseItemWin.PropertyNames[]> _rawChanges;

        public Dictionary<Guid, List<ChangedProperty>> Changes = new Dictionary<Guid, List<ChangedProperty>>();

        [DataMember]
        public Guid[] _rawDeletes;

        [OnSerializing]
        public void _onSerializing(StreamingContext c)
        {
            _rawDeletes = Deletes.Keys.ToArray();

            _rawChanges = new Dictionary<Guid, BaseItemWin.PropertyNames[]>();
            foreach (var pair in Changes)
                _rawChanges[pair.Key] = pair.Value.Select(i => i.PropertyName).ToArray();
        }

        [OnSerialized]
        public void _onSerialized(StreamingContext c)
        {
            //free up memory
            _rawDeletes = null;
            _rawChanges = null;
        }

        [OnDeserialized]
        public void _onDeserialized(StreamingContext c)
        {
            Deletes = new Dictionary<Guid, bool>();

            if (_rawDeletes == null)
                return;

            for (int i = 0; i < _rawDeletes.Length; i++)
                Deletes[_rawDeletes[i]] = false;

            //free up memory
            _rawDeletes = null;



            Changes = new Dictionary<Guid, List<ChangedProperty>>();

            if (_rawChanges == null)
                return;

            foreach (var pair in _rawChanges)
                Changes[pair.Key] = pair.Value.Select(i => new ChangedProperty(i)).ToList();
        }

        /// <summary>
        /// Value of bool represents HasBeenSent, which isn't stored on disk since it's useless to store
        /// </summary>
        public Dictionary<Guid, bool> Deletes = new Dictionary<Guid, bool>();

        /// <summary>
        /// Will remove any sent items. Returns true if it made changes, or false if it didn't do anything.
        /// </summary>
        public bool RemoveSentItems(List<SyncResponse.UpdateError> updateErrors)
        {
            bool changed = false;

            List<Guid> changesToRemove = new List<Guid>();

            //skip the changes that had an update error, they should be tried again
            foreach (var pair in Changes.Where(i => !updateErrors.Select(e => e.Identifier).Contains(i.Key)))
            {
                //if all the changes have been sent, then that item is done and can be removed
                if (pair.Value.All(i => i.HasBeenSent))
                {
                    changed = true;
                    changesToRemove.Add(pair.Key);
                }

                //otherwise, we can remove the changed properties that were already sent
                else
                {
                    for (int i = 0; i < pair.Value.Count; i++)
                        if (pair.Value[i].HasBeenSent)
                        {
                            changed = true;
                            pair.Value.RemoveAt(i);
                            i--;
                        }
                }
            }

            //remove the changes that were entirely sent
            foreach (Guid id in changesToRemove)
                Changes.Remove(id);

            //for the errors that said the item didn't exist, we can have it try sending up the full item
            foreach (Guid identifier in updateErrors.Where(i => i.SqlErrorNumber == 999).Select(i => i.Identifier))
            {
                changed = true;
                New(identifier); //if it was actually deleted, the sync response would have locally deleted it so this change will be ignored. This handles the case where sync claimed an item previously inserted, but for some reason it didn't insert! Theoretically this could never happen... but just in case...
            }

            //remove the deletes that were already sent
            foreach (Guid id in Deletes.Where(i => i.Value).Select(i => i.Key).ToArray())
            {
                Deletes.Remove(id);
                changed = true;
            }

            return changed;
        }

        public void Delete(Guid identifier)
        {
            Deletes[identifier] = false;

            Changes.Remove(identifier);
        }

        public void Change(Guid identifier, BaseItemWin.PropertyNames[] properties)
        {
            List<ChangedProperty> existing;

            Changes.TryGetValue(identifier, out existing);

            if (existing == null)
            {
                existing = new List<ChangedProperty>();
                Changes[identifier] = existing;
            }

            //mark matched existing properties as not sent
            for (int i = 0; i < existing.Count; i++)
                if (properties.Contains(existing[i].PropertyName))
                    existing[i].HasBeenSent = false;

            //and add any non-existing properties
            for (int i = 0; i < properties.Length; i++)
                if (!existing.Any(x => x.PropertyName == properties[i]))
                    existing.Add(new ChangedProperty(properties[i]));
        }

        public void New(Guid guid)
        {
            Changes[guid] = new List<ChangedProperty>() { new ChangedProperty(BaseItemWin.PropertyNames.All) };
        }

        /// <summary>
        /// As long as Json.NET is used, it'll serialize dictionaries correctly. DataContractJson formats them as Key: xx Value: xx, which is incorrect.
        /// </summary>
        /// <param name="accountWin"></param>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, object>> GetUpdates(AccountWin accountWin)
        {
            List<Dictionary<string, object>> answer = new List<Dictionary<string, object>>();

            foreach (var pair in Changes)
            {
                Dictionary<string, object> item = getItemUpdates(accountWin, pair.Key, pair.Value);

                if (item != null)
                    answer.Add(item);
            }

            return answer;
        }

        public IEnumerable<Guid> GetDeletes()
        {
            Guid[] toDelete = Deletes.Keys.ToArray();

            //mark them all as sent
            for (int i = 0; i < toDelete.Length; i++)
                Deletes[toDelete[i]] = true;

            return toDelete;
        }

        private Dictionary<string, object> getItemUpdates(AccountWin account, Guid identifier, List<ChangedProperty> changedProperties)
        {
            BaseItemWin item = account.Find(identifier);

            if (item == null)
                return null;


            if (changedProperties.Any(i => i.PropertyName == BaseItemWin.PropertyNames.All))
            {
                foreach (ChangedProperty p in changedProperties)
                    p.HasBeenSent = true;

                return item.SerializeToDictionary();
            }


            Dictionary<string, object> answer = new Dictionary<string, object>();

            answer["Identifier"] = identifier;
            answer["Updated"] = item.Updated;

            foreach (ChangedProperty p in changedProperties)
            {
                answer[p.PropertyName.ToString()] = item.GetPropertyValue(p.PropertyName);
                p.HasBeenSent = true;
            }

            return answer;
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public class ChangedProperty
    {
        [DataMember]
        public BaseItemWin.PropertyNames PropertyName;

        public bool HasBeenSent;

        public ChangedProperty(BaseItemWin.PropertyNames properyName)
        {
            PropertyName = properyName;
        }
    }
}
