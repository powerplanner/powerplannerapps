using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UpgradeFromSilverlight.Sections;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseItemWP : IComparable, IComparable<BaseItemWP>
    {
        public enum PropertyNames
        {
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

            //class
            CourseNumber,
            Credits,
            ShouldAverageGradeTotals,
            DoesRoundGradesUp,
            Color,
            Position,
            DoesCountTowardGPA,
            GradeScales,

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

        public abstract ItemType ItemType { get; }

        internal AccountSection AccountSection;

        public ILoadableSection Section { get; internal set; }

        public BaseItemWP() { }

        public BaseItemWP(AccountSection accountSection)
        {
            AccountSection = accountSection;
        }

        public class HeirarchyComparer : IComparer<BaseItemWP>
        {
            private int value(BaseItemWP item)
            {
                if (item is YearWP || item is TeacherWP || item is ClassAttributeWP || item is ClassSubjectWP)
                    return 0;

                if (item is SemesterWP)
                    return 1;

                if (item is TaskWP || item is ClassWP)
                    return 2;

                if (item is HomeworkWP || item is ExamWP || item is WeightCategoryWP || item is ScheduleWP || item is ClassAttributeUnderClassWP || item is ClassSubjectUnderClassWP)
                    return 3;

                if (item is GradeWP || item is TeacherUnderScheduleWP)
                    return 4;

                throw new NotImplementedException();
            }

            public int Compare(BaseItemWP x, BaseItemWP y)
            {
                return value(x).CompareTo(value(y));
            }
        }

        public bool IsTopParent()
        {
            return this is YearWP || this is TeacherWP || this is ClassAttributeWP || this is ClassSubjectWP;
        }

        public Guid LocalAccountId;

        [DataMember]
        public Guid Identifier { get; set; }

        private BaseItemWP _parent;
        public BaseItemWP Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;

                if (value != null)
                {
                    AccountSection = value.AccountSection;
                    LocalAccountId = value.LocalAccountId;
                }
            }
        }
        
        [DataMember]
        public DateTime DateCreated { get; set; }
        
        [DataMember]
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        public BaseItem Serialize(int offset)
        {
            BaseItem into = serialize(offset);

            into.Identifier = Identifier;

            if (!IsTopParent())
                into.UpperIdentifier = Parent.Identifier;

            into.Updated = Updated.AddMilliseconds(offset);
            into.DateCreated = DateCreated.AddMilliseconds(offset);

            return into;
        }

        protected abstract BaseItem serialize(int offset);

        public Dictionary<string, object> SerializeToDictionary()
        {
            Dictionary<string, object> answer = new Dictionary<string, object>()
            {
                { "Identifier", Identifier },
                { "Updated", Updated },
                { "DateCreated", DateCreated },
                { "ItemType", ItemType }
            };

            if (!IsTopParent())
                answer[PropertyNames.UpperIdentifier.ToString()] = Parent.Identifier;

            serialize(answer);

            return answer;
        }

        protected abstract void serialize(Dictionary<string, object> into);

        /// <summary>
        /// Offset has already been taken into consideration by the BaseItem.
        /// </summary>
        /// <param name="item"></param>
        public void Deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Updated = item.Updated;
            DateCreated = item.DateCreated; //we only send up DateCreated when item is locally created, it can't change after that

            deserialize(item, changedProperties);
        }

        protected abstract void deserialize(BaseItem item, List<PropertyNames> changedProperties);

        public virtual object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.DateCreated:
                    return DateCreated;

                case PropertyNames.UpperIdentifier:
                    if (Parent == null)
                        return Guid.Empty;
                    return Parent.Identifier;
            }

            throw new ArgumentException("Property wasn't found: " + p.ToString());
        }

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
            if (obj is BaseItemWP)
                return CompareTo(obj as BaseItemWP);

            return 0;
        }

        /// <summary>
        /// Things with earlier creation dates go first
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(BaseItemWP other)
        {
            return DateCreated.CompareTo(other.DateCreated);
        }

        public bool IsCompletelyLoaded { get; private set; }

        public void LoadAll()
        {
            if (IsCompletelyLoaded)
                return;

            foreach (BaseItemWP i in GetChildren())
                i.GetChildren();

            IsCompletelyLoaded = true;
        }

        //public BaseItemWP Find(Guid identifier)
        //{
        //    IEnumerable<BaseItemWP> children = GetChildren();

        //    //see if it's one of the immediate children
        //    foreach (BaseItemWP item in children)
        //        if (item.Identifier.Equals(identifier))
        //            return item;

        //    //otherwise have each child look for the item
        //    foreach (BaseItemWP item in children)
        //    {
        //        BaseItemWP found = item.Find(identifier);
        //        if (found != null)
        //            return found;
        //    }

        //    //otherwise not found
        //    return null;
        //}

        /// <summary>
        /// Returns the immediate children. Will load if necessary.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<BaseItemWP> GetChildren();

        internal abstract BaseItemWP FindFromSection(Guid identifier);

        /// <summary>
        /// Returns all children, including those multiple levels down, from this item. Items in list are ordered depth-first.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseItemWP> GetAllChildren()
        {
            List<BaseItemWP> answer = new List<BaseItemWP>();

            foreach (BaseItemWP item in GetChildren())
            {
                answer.Add(item);
                answer.AddRange(item.GetAllChildren());
            }

            return answer;
        }

        internal abstract void Remove(BaseItemWP item);

        /// <summary>
        /// If permanent is true, it will delete the entire section if necessary
        /// </summary>
        /// <param name="permanent"></param>
        internal virtual void Delete(bool permanent)
        {
            Parent.Remove(this);
        }

        internal abstract void Add(BaseItemWP item);
    }
}
