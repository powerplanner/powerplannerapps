using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using UpgradeFromSilverlight.Model;

namespace UpgradeFromSilverlight.Sections
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.LoadableSections")]
    public class SemesterSection : LoadableSection<SemesterWP>
    {
        public YearWP Year { get; internal set; }
        public Guid SemesterIdentifier { get; private set; }

        private AccountSection _accountSection;


        public SemesterSection(AccountSection accountSection, string fileName, Guid semesterIdentifier)
            : base(fileName, accountSection)
        {
            _accountSection = accountSection;
            SemesterIdentifier = semesterIdentifier;
        }

        public SemesterSection(AccountSection accountSection, Guid localAccountId, SemesterWP semester)
            : this(accountSection, Files.SEMESTER_FILE(localAccountId, semester.Identifier), semester.Identifier)
        {
            Year = (YearWP)semester.Parent;

            //automatically calls loaded
            Value = semester;
        }

        protected override void loaded(SemesterWP value)
        {
            Debug.WriteLine("Semester " + value.Name + " loaded");

            if (Year == null)
            {
                if (value.YearIdentifier == Guid.Empty)
                    return;

                Year = _accountSection.Value.School.Years.FirstOrDefault(i => i.Identifier == value.YearIdentifier);

                if (Year == null)
                    return;
            }

            loaded(value, Year);

            foreach (ClassWP c in value.Classes)
            {
                loaded(c, value);

                foreach (HomeworkWP h in c.Homework)
                    loaded(h, c);

                foreach (ExamWP e in c.Exams)
                    loaded(e, c);

                foreach (ScheduleWP s in c.Schedules)
                    loaded(s, c);
            }

            foreach (TaskWP t in value.Tasks)
                loaded(t, value);
        }
    }
}
