using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class SemesterWin : BaseItemWithGPACreditsWin, IGPACredits
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Semester; }
        }

        public SemesterWin()
        {
            Classes = new MyObservableList<ClassWin>();
            Tasks = new MyObservableList<TaskWin>();
        }

        private bool _needsRecalc;

        private DateTime _start;
        [DataMember]
        public DateTime Start
        {
            get { return _start; }
            set { SetProperty(ref _start, DateTime.SpecifyKind(value, DateTimeKind.Utc), "Start"); }
        }

        private DateTime _end;
        [DataMember]
        public DateTime End
        {
            get { return _end; }
            set { SetProperty(ref _end, DateTime.SpecifyKind(value, DateTimeKind.Utc), "End"); }
        }

        public string SubTitle
        {
            get
            {
                string str = "Credits: ";

                if (Credits == -1)
                    str += "NONE";
                else
                    str += Credits.ToString("0.#");

                if (Classes.Count > 0)
                    str += " -";

                for (int i = 0; i < Classes.Count; i++)
                    str += ' ' + Classes[i].Name + ",";

                return str.TrimEnd(',');
            }
        }

        #region DisplayLists

        [DataMember]
        public MyObservableList<ClassWin> Classes { get; set; }

        private MyObservableList<HomeworkWin> _homework;
        public MyObservableList<HomeworkWin> Homework
        {
            get
            {
                if (_homework == null)
                {
                    _homework = new MyObservableList<HomeworkWin>();
                    _homework.InsertSorted(Classes, "Homework");
                }

                return _homework;

                //if (_homework == null)
                //{
                //    _homework = new MyObservableList<HomeworkWin>();
                //    _homework.InsertSorted(Classes, "Homework");
                //}

                //return _homework;
            }
        }

        public bool IsHomeworkLoaded { get { return _homework != null; } }

        private MyObservableList<ExamWin> _exams;
        public MyObservableList<ExamWin> Exams
        {
            get
            {
                if (_exams == null)
                {
                    _exams = new MyObservableList<ExamWin>();
                    _exams.InsertSorted(Classes, "Exams");
                }

                return _exams;
            }
        }

        public bool IsExamsLoaded { get { return _exams != null; } }

        [DataMember]
        public MyObservableList<TaskWin> Tasks { get; set; }
        
        

        #endregion

        
        

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Start.ToString()] = Start;
            into[PropertyNames.End.ToString()] = End;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            Semester into = new Semester()
            {
                Start = Start,
                End = End
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.Start:
                    return Start;

                case PropertyNames.End:
                    return End;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Semester i = item as Semester;

            if (changedProperties != null)
            {
                if (Start != i.Start.ToUniversalTime())
                    changedProperties.Add(PropertyNames.Start);

                if (End != i.End.ToUniversalTime())
                    changedProperties.Add(PropertyNames.End);
            }

            Start = i.Start.ToUniversalTime();
            End = i.End.ToUniversalTime();

            base.deserialize(i, changedProperties);
        }

        protected override IEnumerable getGradedChildren()
        {
            return Classes;
        }
        

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return new IEnumerableLinker<BaseItemWin>(Classes, Tasks);
        }

        internal override void Delete(bool permanent)
        {
            base.Delete(permanent);
        }

        internal override void Add(BaseItemWin item)
        {
            if (item is ClassWin)
                Classes.InsertSorted(item as ClassWin);

            else if (item is TaskWin)
                Tasks.InsertSorted(item as TaskWin);

            else
                throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWin item)
        {
            if (item is ClassWin)
                Classes.Remove(item as ClassWin);

            else if (item is TaskWin)
                Tasks.Remove(item as TaskWin);

            else
                throw new NotImplementedException("Object being removed from Semester wasn't any of the supported types.");
        }
    }
}
