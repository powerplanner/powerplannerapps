using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromSilverlight.Sections
{
    public static class Files
    {
        /// 
        ///  With this method, if I delete a semester, I have to manually locate all children and delete them
        ///  Data/
        ///     School.dat
        ///     Semesters/
        ///         jf90j-j9f2-j-j23f-9.dat
        ///         9203aw9-f09ja--j9awe.dat
        ///     Grades/
        ///         cj92j9032-j-9jj-32-f.dat (less used things in classes, like grades and grade scales)
        ///         jf-9aejsf90-ja-9sefj.dat
        ///         fja0-9jf-dsj-9-j-9sdf.dat
        ///         jf9-dj-9sadjfj9-sdfj.dat
        ///         jfd9-sj-jdfs-9j9-djsf.dat
        ///         
        /// 
        ///   With this method, if I move a semester, I need to move the entire folder
        ///   Data/
        ///     School.dat
        ///     jf90-2j9fj2-9j-jf92/ (year folder)
        ///         jw90f3j29092-jf2/ (semester folder)
        ///             Semester.dat (contains semester, classes, homework, exams, schedules)
        ///             aweio-aj9923-j9-f23/ (class folder)
        ///                 Grades.dat
        ///             j902j320j--32j9-fj-3-j2/ (class folder)
        ///             
        ///         ewj90j-j9wj-jwj9-a1eg/ (semester folder)
        ///             Semester.dat
        /// 

        public static readonly string ACCOUNTS_FOLDER = "AccountsWP/";
        public static string ACCOUNT_FOLDER(Guid localAccountId)
        {
            return ACCOUNTS_FOLDER + localAccountId + "/";
        }

        public static string DATA_FOLDER(Guid localAccountId)
        {
            return ACCOUNT_FOLDER(localAccountId) + "Data/";
        }

        public static string ACCOUNT_FILE(Guid localAccountId)
        {
            return DATA_FOLDER(localAccountId) + "Account.dat";
        }

        public static string ACCOUNT_DATABASE_FILE(Guid localAccountId)
        {
            return DATA_FOLDER(localAccountId) + "AccountDatabase.sdf";
        }

        public static string PARTIAL_CHANGES_FILE(Guid localAccountId)
        {
            return ACCOUNT_FOLDER(localAccountId) + "PartialChanges.json";
        }

        public static string SEMESTERS_FOLDER(Guid localAccountId)
        {
            return DATA_FOLDER(localAccountId) + "Semesters/";
        }

        public static string SEMESTER_FILE(Guid localAccountId, Guid semesterIdentifier)
        {
            return SEMESTERS_FOLDER(localAccountId) + semesterIdentifier + ".dat";
        }

        public static string GRADES_FOLDER(Guid localAccountId)
        {
            return DATA_FOLDER(localAccountId) + "Grades/";
        }

        public static string GRADE_FILE(Guid localAccountId, Guid classIdentifier)
        {
            return GRADES_FOLDER(localAccountId) + classIdentifier + ".dat";
        }



        public static string IMAGES_FOLDER(Guid localAccountId)
        {
            return ACCOUNT_FOLDER(localAccountId) + "Images/";
        }

        public static string IMAGE_FILE_NAME(string fileName, Guid login)
        {
            return IMAGES_FOLDER(login) + fileName;
        }
        
    }
}
