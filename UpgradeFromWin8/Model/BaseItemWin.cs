using UpgradeFromWin8.Model.TopItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public abstract class BaseItemWin : BindableBase, IComparable, IComparable<BaseItemWin>
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

        public BaseItemWin() { }

        public class HeirarchyComparer : IComparer<BaseItemWin>
        {
            private int value(BaseItemWin item)
            {
                if (item is YearWin || item is TeacherWin || item is ClassAttributeWin || item is ClassSubjectWin)
                    return 0;

                if (item is SemesterWin)
                    return 1;

                if (item is TaskWin || item is ClassWin)
                    return 2;

                if (item is HomeworkWin || item is ExamWin || item is WeightCategoryWin || item is ScheduleWin || item is ClassAttributeUnderClassWin || item is ClassSubjectUnderClassWin)
                    return 3;

                if (item is GradeWin || item is TeacherUnderScheduleWin)
                    return 4;

                throw new NotImplementedException();
            }

            public int Compare(BaseItemWin x, BaseItemWin y)
            {
                return value(x).CompareTo(value(y));
            }
        }

        public bool IsTopParent()
        {
            return this is YearWin || this is TeacherWin || this is ClassAttributeWin || this is ClassSubjectWin;
        }

        [DataMember]
        public Guid Identifier { get; set; }

        private BaseItemWin _parent;
        public BaseItemWin Parent
        {
            get { return _parent; }
            set
            {
                SetProperty(ref _parent, value, "Parent");
            }
        }

        private DateTime _dateCreated;
        [DataMember]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { SetProperty(ref _dateCreated, value, "DateCreated"); }
        }

        private DateTime _updated = DateTime.UtcNow;
        [DataMember]
        public DateTime Updated
        {
            get { return _updated; }
            set { SetProperty(ref _updated, value, "Updated"); }
        }

        public BaseItem Serialize()
        {
            BaseItem into = serialize();

            into.Identifier = Identifier;

            if (!IsTopParent())
                into.UpperIdentifier = Parent.Identifier;

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

            if (!IsTopParent())
                answer[PropertyNames.UpperIdentifier.ToString()] = Parent.Identifier;

            serialize(answer);

            return answer;
        }

        protected abstract void serialize(Dictionary<string, object> into);

        protected abstract BaseItem serialize();

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
            if (obj is BaseItemWin)
                return CompareTo(obj as BaseItemWin);

            return 0;
        }

        /// <summary>
        /// Things with earlier creation dates go first
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(BaseItemWin other)
        {
            return DateCreated.CompareTo(other.DateCreated);
        }

        public bool IsCompletelyLoaded { get; private set; }

        public void LoadAll()
        {
            if (IsCompletelyLoaded)
                return;

            foreach (BaseItemWin i in GetChildren())
                i.GetChildren();

            IsCompletelyLoaded = true;
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

        /// <summary>
        /// Returns the immediate children. Will load if necessary.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<BaseItemWin> GetChildren();

        internal BaseItemWin Find(Guid identifier)
        {
            foreach (BaseItemWin child in GetChildren())
            {
                if (child.Identifier == identifier)
                    return child;

                BaseItemWin answer = child.Find(identifier);
                if (answer != null)
                    return answer;
            }

            return null;
        }

        /// <summary>
        /// Returns all children, including those multiple levels down, from this item. Items in list are ordered depth-first.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseItemWin> GetAllChildren()
        {
            List<BaseItemWin> answer = new List<BaseItemWin>();

            foreach (BaseItemWin item in GetChildren())
            {
                answer.Add(item);
                answer.AddRange(item.GetAllChildren());
            }

            return answer;
        }

        internal abstract void Remove(BaseItemWin item);

        /// <summary>
        /// If permanent is true, it will delete the entire section if necessary
        /// </summary>
        /// <param name="permanent"></param>
        internal virtual void Delete(bool permanent)
        {
            Parent.Remove(this);
        }

        internal abstract void Add(BaseItemWin item);

        internal void Initialize(BaseItemWin parent)
        {
            Parent = parent;

            foreach (BaseItemWin child in GetChildren())
                child.Initialize(this);
        }

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
    }
}
