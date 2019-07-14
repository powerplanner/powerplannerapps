using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromSilverlight.Model
{
    /// <summary>
    /// Using JSON to serialize will make this take up very little space. PropertyName enums will be numbers. It'll be very minimal.
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP")]
    public class PartialChanges
    {
        [DataMember]
        public Dictionary<Guid, BaseItemWP.PropertyNames[]> _rawChanges;

        public Dictionary<Guid, List<ChangedProperty>> Changes = new Dictionary<Guid, List<ChangedProperty>>();

        [DataMember]
        public Guid[] _rawDeletes;

        [OnSerializing]
        public void _onSerializing(StreamingContext c)
        {
            _rawDeletes = Deletes.Keys.ToArray();

            _rawChanges = new Dictionary<Guid, BaseItemWP.PropertyNames[]>();
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


        public void Delete(Guid identifier)
        {
            Deletes[identifier] = false;

            Changes.Remove(identifier);
        }

        public void New(Guid guid)
        {
            Changes[guid] = new List<ChangedProperty>() { new ChangedProperty(BaseItemWP.PropertyNames.All) };
        }

        /// <summary>
        /// As long as Json.NET is used, it'll serialize dictionaries correctly. DataContractJson formats them as Key: xx Value: xx, which is incorrect.
        /// </summary>
        /// <param name="accountWin"></param>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, object>> GetUpdates(AccountWP accountWin)
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

        private Dictionary<string, object> getItemUpdates(AccountWP account, Guid identifier, List<ChangedProperty> changedProperties)
        {
            BaseItemWP item = account.Find(identifier);

            if (item == null)
                return null;


            if (changedProperties.Any(i => i.PropertyName == BaseItemWP.PropertyNames.All))
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

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP")]
    public class ChangedProperty
    {
        [DataMember]
        public BaseItemWP.PropertyNames PropertyName;

        public bool HasBeenSent;

        public ChangedProperty(BaseItemWP.PropertyNames properyName)
        {
            PropertyName = properyName;
        }
    }
}
