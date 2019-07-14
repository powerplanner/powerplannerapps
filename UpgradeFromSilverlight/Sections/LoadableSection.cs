using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UpgradeFromSilverlight.Model;

namespace UpgradeFromSilverlight.Sections
{
    public interface ILoadableSection
    {
        bool IsLoaded { get; }

        /// <summary>
        /// Should be called before adding to lists so that parent is set on item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parent"></param>
        void AssignToSection(BaseItemWP item, BaseItemWP parent);

        string FileName { get; set; }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.LoadableSections")]
    public abstract class LoadableSection<T> : ILoadableSection, IEquatable<LoadableSection<T>> where T : new()
    {
        public AccountSection AccountSection { get; internal set; }

        [DataMember]
        public string FileName { get; set; }

        //private static HashSet<LoadableSection> _loadedSections = new HashSet<LoadableSection>();

        private T _value;
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    Load();
                }

                return _value;
            }

            protected set { _value = value; loaded(value); }
        }

        public bool IsLoaded { get { return _value != null; } }

        /// <summary>
        /// If already loaded, returns false. Otherwise loads and returns true.
        /// </summary>
        public bool Load()
        {
            if (IsLoaded)
                return false;

            T val = IMyStorage.Load<T>(FileName);

            //if (val == null)
            //    val = new T();

            Value = val;
            return true;
        }

        /// <summary>
        /// This should initialize the AllItems dictionary
        /// </summary>
        /// <param name="value"></param>
        protected abstract void loaded(T value);


        public LoadableSection(string fileName, AccountSection accountSection)
        {
            FileName = fileName;

            AccountSection = accountSection;
            //LoadedSections.Add(this);
        }

        protected void loaded(BaseItemWP item, BaseItemWP parent)
        {
            AssignToSection(item, parent);
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }

        public bool Equals(LoadableSection<T> other)
        {
            return FileName.Equals(other.FileName);
        }


        public void AssignToSection(BaseItemWP item, BaseItemWP parent)
        {
            item.Parent = parent;
            item.AccountSection = parent.AccountSection;
            item.Section = this;

            AccountSection.AddLoadedItem(item);
        }
    }
}
