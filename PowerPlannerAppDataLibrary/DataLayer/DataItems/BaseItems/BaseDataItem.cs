using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.IO;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public class DataItemProperty
    {
        public BaseDataItem.SyncPropertyNames[] AffectedSyncPropertyNames { get; private set; }

        private DataItemProperty() { }

        public static DataItemProperty Register(params BaseDataItem.SyncPropertyNames[] affectedSyncPropertyNames)
        {
            return new DataItemProperty()
            {
                AffectedSyncPropertyNames = affectedSyncPropertyNames
            };
        }
    }

    [DataContract]
    public abstract class BaseDataItem : IComparable, IComparable<BaseDataItem>, IEquatable<BaseDataItem>
    {
        private const string IDENTIFIER = "Identifier";
        private const string DATE_CREATED = "DateCreated";
        private const string UPDATED = "Updated";

        [SQLite.Ignore]
        public AccountDataItem Account { get; internal set; }

        private Dictionary<DataItemProperty, object> _propertyValues = new Dictionary<DataItemProperty, object>();
        
        internal object GetValue(DataItemProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            object value;

            if (_propertyValues.TryGetValue(property, out value))
                return value;

            return null;
        }

        internal T GetValue<T>(DataItemProperty property, T defaultValue = default(T))
        {
            if (property == null)
                throw new ArgumentNullException("property");

            object value = GetValue(property);

            if (value is T)
                return (T)value;

            return defaultValue;
        }

        internal void SetValue(DataItemProperty property, object value)
        {
            if (property == null)
            {
                return;
            }

            _propertyValues[property] = value;
        }

        public bool IsNewItem()
        {
            return Identifier == Guid.Empty;
        }

        public BaseDataItem Clone()
        {
            return (BaseDataItem)this.MemberwiseClone();
        }

        private static Dictionary<Type, DataItemProperty[]> _cachedProperties = new Dictionary<Type, DataItemProperty[]>();
        
        internal DataItemProperty[] GetProperties()
        {
            /// The properties are static readonly properties on the class. So if an instance of the class already exists, that means the properties 
            /// have already been assigned and will never change. Thus, we can load them once for each type and then simply cache them for
            /// future lookups.

            DataItemProperty[] properties;

            Type type = this.GetType();

            // If the properties have already been loaded for this type
            if (_cachedProperties.TryGetValue(type, out properties))
                return properties;

            // Otherwise load the properties for this type
            properties = type.GetTypeInfo().DeclaredFields.Where(f => f.IsStatic && f.FieldType == typeof(DataItemProperty)).Select(f => f.GetValue(null)).OfType<DataItemProperty>().ToArray();

            // And then cache the properties for this type
            _cachedProperties[type] = properties;

            // And then return them
            return properties;
        }

        /// <summary>
        /// Gets a collection of properties that have already been set on the item
        /// </summary>
        /// <returns></returns>
        internal DataItemProperty[] GetSetProperties()
        {
            return _propertyValues.Keys.ToArray();
        }

        /// <summary>
        /// Applies changes from the item, and tracks which properties actually changed. Properties that weren't set on the From item are ignored.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        internal HashSet<SyncPropertyNames> ImportChanges(BaseDataItem from)
        {
            // Updated is always changed, we always copy it, we always sync it up too
            this.Updated = from.Updated;

            // Construct a list for our answer of sync properties that have changes
            HashSet<SyncPropertyNames> changedSyncProperties = new HashSet<SyncPropertyNames>();

            // Get all the properties that have been set on the item
            DataItemProperty[] setProperties = from.GetSetProperties();
            
            // And copy each property to this item
            foreach (var property in setProperties)
            {
                object currValue = this.GetValue(property);
                object newValue = from.GetValue(property);

                // If the objects are equal, no need to make a change
                if (object.Equals(currValue, newValue))
                    continue;

                // Otherwise, make the change
                this.SetValue(property, newValue);

                // And also flag that these sync properties have changed
                foreach (var syncPropertyName in property.AffectedSyncPropertyNames)
                    changedSyncProperties.Add(syncPropertyName);
            }

            // And return our answer of changed sync properties
            return changedSyncProperties;
        }

        public static string SerializeToString(object obj)
        {
            if (obj == null)
                return null;

            using (StringWriter writer = new StringWriter())
            {
                new JsonSerializer().Serialize(writer, obj);

                writer.Flush();

                return writer.ToString();
            }
        }

        public static T DeserializeFromString<T>(string str)
        {
            if (str == null)
                return default(T);

            using (StringReader reader = new StringReader(str))
            {
                try
                {
                    return (T)new JsonSerializer().Deserialize(reader, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed deserialize string: " + str, ex);
                }
            }
        }

        public enum DataPropertyNames
        {
            //Identifier always sent
            DateCreated,
            //Updated always sent
            UpperIdentifier,
            SecondUpperIdentifier,
            Name,
            Details,
            RawImageNames,
            Date,
            EndTime,
            Reminder,
            PercentComplete,

            OverriddenGrade,

            //class
            CourseNumber,
            Credits,
            ShouldAverageGradeTotals,
            DoesRoundGradesUp,
            RawColor,
            Position,
            DoesCountTowardGPA,
            GradeScales,
            GPA,

            //grade
            GradeReceived,
            GradeTotal,
            IsDropped,
            IndividualWeight,

            //schedule
            DayOfWeek,
            StartTime,
            //EndTime already listed
            Room,
            ScheduleType,
            ScheduleWeek,
            LocationLatitude,
            LocationLongitude,

            //semester
            Start,
            End,

            //teacher
            PhoneNumbers,
            EmailAddresses,
            PostalAddresses,
            OfficeLocations,

            //weight category
            WeightValue,

            //used if item was created, send up everything
            All
        }

        public enum SyncPropertyNames
        {
            // WARNING: Do NOT rename these enum values. Sync API's currently rely on the exact string translation of these enums.

            //Identifier always sent
            DateCreated,
            //Updated always sent
            UpperIdentifier,
            SecondUpperIdentifier,
            Name,
            Details,
            ImageNames,
            Date,
            EndTime,
            Reminder,
            PercentComplete,
            WeightCategoryIdentifier,

            //class
            CourseNumber,
            Credits,
            ShouldAverageGradeTotals,
            DoesRoundGradesUp,
            Color,
            Position,
            GradeScales,
            GPA,
            OverriddenGPA,
            OverriddenGrade,
            StartDate,
            EndDate,
            GpaType,
            PassingGrade,

            //grade (and also task/event)
            GradeReceived,
            GradeTotal,
            IsDropped,
            IndividualWeight,

            //schedule
            DayOfWeek,
            StartTime,
            //EndTime already listed
            Room,
            ScheduleType,
            ScheduleWeek,
            LocationLatitude,
            LocationLongitude,

            //semester
            Start,
            End,
            OverriddenCredits,
            // OverriddenGPA already listed

            //year
            // OverriddenCredits already listed
            // OverriddenGPA already listed

            //teacher
            PhoneNumbers,
            EmailAddresses,
            PostalAddresses,
            OfficeLocations,

            //weight category
            WeightValue,

            //used if item was created, send up everything
            All,

            // Used if item was deleted, send it as an item to delete
            Deleted,

            MegaItemType
        }

        public abstract ItemType ItemType { get; }

        public class HeirarchyComparer : IComparer<BaseDataItem>
        {
            private int value(BaseDataItem item)
            {
                if (item is DataItemYear || item is DataItemTeacher || item is DataItemClassAttribute || item is DataItemClassSubject)
                    return 0;

                if (item is DataItemSemester)
                    return 1;

                if (item is DataItemClass)
                    return 2;

                if (item is DataItemMegaItem)
                {
                    if ((item as DataItemMegaItem).IsUnderClass())
                        return 3;
                    else
                        return 2;
                }

                if (
#pragma warning disable 612, 618
                    item is DataItemHomework || item is DataItemExam
#pragma warning restore 612, 618
                    || item is DataItemWeightCategory || item is DataItemSchedule || item is DataItemClassAttributeUnderClass || item is DataItemClassSubjectUnderClass)
                    return 3;

                if (item is DataItemGrade || item is DataItemTeacherUnderSchedule)
                    return 4;

                throw new NotImplementedException();
            }

            public int Compare(BaseDataItem x, BaseDataItem y)
            {
                return value(x).CompareTo(value(y));
            }
        }

        public bool IsTopParent()
        {
            return this is DataItemYear || this is DataItemTeacher || this is DataItemClassAttribute || this is DataItemClassSubject;
        }

        [PrimaryKey, Indexed(Name="Index_Identifier")]
        [Column(IDENTIFIER)]
        public Guid Identifier { get; set; }

        private DateTime _dateCreated = SqlDate.MinValue;
        [Column(DATE_CREATED)]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { _dateCreated = EnsureUtc(value); }
        }

        private DateTime _updated = SqlDate.MinValue;
        [Column(UPDATED)]
        public DateTime Updated
        {
            get { return _updated; }
            set { _updated = EnsureUtc(value); }
        }

        private DateTime EnsureUtc(DateTime original)
        {
            if (original.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(original, DateTimeKind.Utc);

            return original;
        }


        [Obsolete("This shouldn't be used anymore. Sync should be sending up specific property values. And the UI code should be constructing new DataItems and assigning their changes to those. But I'll keep this here right now just in case.", true)]
        public BaseItem Serialize()
        {
            BaseItem into = serialize();

            into.Identifier = Identifier;

            into.Updated = Updated;
            into.DateCreated = DateCreated;

            return into;
        }

        public Dictionary<string, object> SerializeToDictionary()
        {
            Dictionary<string, object> answer = new Dictionary<string, object>()
            {
                { "Identifier", Identifier },
                { "Updated", Updated },
                { "DateCreated", DateCreated },
                { "ItemType", ItemType }
            };

            serialize(answer);

            return answer;
        }

        protected abstract void serialize(Dictionary<string, object> into);

        protected abstract BaseItem serialize();

        /// <summary>
        /// Offset has already been taken into consideration by the BaseItem.
        /// </summary>
        /// <param name="item"></param>
        public void Deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Updated = item.Updated;
            DateCreated = item.DateCreated; //we only send up DateCreated when item is locally created, it can't change after that

            deserialize(item, changedProperties);
        }

        protected abstract void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties);

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <summary>
        /// Some classes can implement this, so that when called it'll recalculate its own item
        /// </summary>
        public virtual void Calculate()
        {
            //nothing
        }


        public enum ChangeType { Add, Remove, Edited }


        public virtual int CompareTo(object obj)
        {
            if (obj is BaseDataItem)
                return CompareTo(obj as BaseDataItem);

            return 0;
        }

        /// <summary>
        /// Things with earlier creation dates go first
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(BaseDataItem other)
        {
            return DateCreated.CompareTo(other.DateCreated);
        }

        //public BaseItemWin Find(Guid identifier)
        //{
        //    IEnumerable<BaseItemWin> children = GetChildren();

        //    //see if it's one of the immediate children
        //    foreach (BaseItemWin item in children)
        //        if (item.Identifier.Equals(identifier))
        //            return item;

        //    //otherwise have each child look for the item
        //    foreach (BaseItemWin item in children)
        //    {
        //        BaseItemWin found = item.Find(identifier);
        //        if (found != null)
        //            return found;
        //    }

        //    //otherwise not found
        //    return null;
        //}

        public virtual object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.DateCreated:
                    return DateCreated;
            }

            throw new ArgumentException("Property wasn't found: " + p.ToString());
        }

        public bool Equals(BaseDataItem other)
        {
            return Identifier == other.Identifier;
        }
    }
}
