using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UpgradeFromSilverlight.Model;

namespace UpgradeFromSilverlight.Sections
{
    public class AccountSection : LoadableSection<AccountWP>
    {
        [Flags]
        public enum FindType
        {
            None = 0,
            Semesters = 1,
            Grades = 2,
            All = 3
        }

        private static Dictionary<Guid, AccountSection> _initializedSchools = new Dictionary<Guid, AccountSection>();

        /// <summary>
        /// Will always returned initialized section, unless . Ensures there's only one section per each account.
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        public static AccountSection Get(Guid localAccountId)
        {
            if (localAccountId == Guid.Empty)
                throw new ArgumentException("localAccountId cannot be an empty GUID.");

            AccountSection school;

            if (_initializedSchools.TryGetValue(localAccountId, out school))
                return school;

            school = new AccountSection(localAccountId);
            _initializedSchools[localAccountId] = school;
            return school;
        }

        public static AccountSection Create(AccountWP account)
        {
            AccountSection section = new AccountSection(account.LocalId);
            section.Value = account;
            _initializedSchools[account.LocalId] = section;
            return section;
        }

        public GradesSection GetGradesSection(ClassWP c)
        {
            string fileName = Files.GRADE_FILE(_localAccountId, c.Identifier);
            GradesSection section = GradesSections.FirstOrDefault(i => i.FileName.Equals(fileName));
            
            if (section == null)
            {
                section = new GradesSection(this, fileName, c);
                GradesSections.Add(section);
            }

            return section;
        }

        private Dictionary<Guid, BaseItemWP> _loadedItems = new Dictionary<Guid, BaseItemWP>();

        private Guid _localAccountId;

        private AccountSection(Guid localAccountId)
            : base(Files.ACCOUNT_FILE(localAccountId), null)
        {
            _localAccountId = localAccountId;
            AccountSection = this;
        }

        public void AddLoadedItem(BaseItemWP item)
        {
            _loadedItems[item.Identifier] = item;
        }

        public void RemoveLoadedItem(BaseItemWP item)
        {
            _loadedItems.Remove(item.Identifier);
        }

        private HashSet<SemesterSection> _semesterSections;
        public HashSet<SemesterSection> SemesterSections
        {
            get
            {
                if (_semesterSections == null)
                {
                    _semesterSections = new HashSet<SemesterSection>();
                    string folder = Files.SEMESTERS_FOLDER(_localAccountId);

                    foreach (string semesterFile in IMyStorage.GetFileNames(folder))
                    {
                        _semesterSections.Add(
                            new SemesterSection(this, folder + semesterFile, Guid.Parse(semesterFile.Substring(0, semesterFile.Length - ".dat".Length)))
                        );
                    }
                }

                return _semesterSections;
            }
        }

        public SemesterWP GetSemesterOrNull(Guid identifier)
        {
            SemesterSection section = SemesterSections.FirstOrDefault(i => i.SemesterIdentifier == identifier);
            if (section != null)
                return section.Value;

            return null;
        }

        private HashSet<GradesSection> _gradesSections;
        public HashSet<GradesSection> GradesSections
        {
            get
            {
                if (_gradesSections == null)
                {
                    _gradesSections = new HashSet<GradesSection>();
                    string folder = Files.GRADES_FOLDER(_localAccountId);

                    foreach (string file in IMyStorage.GetFileNames(folder))
                    {
                        Guid classId = Guid.Parse(file.Substring(0, file.Length - ".dat".Length));

                        ClassWP c = Find(classId, FindType.Semesters) as ClassWP;

                        if (c != null)
                        {
                            GradesSection section = new GradesSection(this, folder + file, c);
                            _gradesSections.Add(section);
                        }
                    }

                    //foreach (SemesterWP s in SemesterSections.Select(i => i.Value))
                    //{
                    //    foreach (ClassWP c in s.Classes)
                    //        c.LoadGradesSection();
                    //}
                }

                return _gradesSections;
            }
        }

        /// <summary>
        /// When this is called, parent of semester should be initialized
        /// </summary>
        /// <param name="semester"></param>
        public void MakeSureHasSection(SemesterWP semester)
        {
            string fileName = Files.SEMESTER_FILE(_localAccountId, semester.Identifier);

            //if there's no section for it yet
            if (SemesterSections.Where(i => i.FileName.Equals(fileName)).FirstOrDefault() == null)
            {
                createSemesterSection(semester);
            }
        }

        private void createSemesterSection(SemesterWP semester)
        {
            SemesterSection section = new SemesterSection(this, _localAccountId, semester);

            if (_semesterSections != null)
                _semesterSections.Add(section);
        }

        protected override void loaded(AccountWP value)
        {
            Debug.WriteLine("Account " + value.Username + " loaded");

            value.AccountSection = this;

            value.School.AccountSection = this;
            value.School.LocalAccountId = _localAccountId;
            value.School.Section = this;

            foreach (YearWP year in value.School.Years)
            {
                loaded(year, value.School);
            }
        }

        public BaseItemWP FindFromLoaded(Guid identifier)
        {
            BaseItemWP found;
            _loadedItems.TryGetValue(identifier, out found);
            return found;
        }

        /// <summary>
        /// Looks for and returns the matching item. Will load additional sections if not found.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public BaseItemWP Find(Guid identifier, FindType type = FindType.All)
        {
            if (identifier == Guid.Empty)
                return null;

            BaseItemWP found = FindFromLoaded(identifier);
            if (found != null)
                return found;

            //make sure everything's loaded
            if (type.HasFlag(FindType.Semesters))
                foreach (SemesterSection s in SemesterSections)
                    s.Load();

            if (type.HasFlag(FindType.Grades))
                foreach (GradesSection g in GradesSections)
                    g.Load();

            //try again
            return FindFromLoaded(identifier);
        }

    }
}
