using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ToolsUniversal
{
    public class AppSettings : IDictionary<string, object>
    {
        public static readonly AppSettings Current = new AppSettings();

        private IPropertySet _values = ApplicationData.Current.LocalSettings.CreateContainer("ToolsUniversal_AppSettings", ApplicationDataCreateDisposition.Always).Values;

        private AppSettings()
        {

        }

        public object this[string key]
        {
            get
            {
                return _values[key];
            }

            set
            {
                _values[key] = value;
            }
        }



        public void Add(string key, object value)
        {
            _values[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _values.Keys; }
        }

        public bool Remove(string key)
        {
            return _values.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _values.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return _values.Values; }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _values.Add(item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public bool IsReadOnly
        {
            get { return _values.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _values.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }
}
