using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromSilverlight
{
    public static class IMyStorage
    {
        private static IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

        private static readonly string BACKUP_FOLDER = "MyUniqueBackupFolder/";


        /// <summary>
        /// Loads the object. If file does not exist, returns default(T). If failed to deserialize, returns default(T). Will try loading from backup before returning default(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public static T Load<T>(string fileName, params Type[] knownTypes)
        {
            try
            {
                if (FileExists(fileName)) //load the file
                    return load<T>(fileName, knownTypes);
            }

            catch (Exception e)
            {
                Debug.WriteLine("Failed loading/deserializing Silverlight file: " + e.ToString());
            }

            //if the file wasn't found, or if an error was thrown while loading, it'll load the backup
            try
            {
                if (FileExists(BACKUP_FOLDER + fileName))
                {
                    //then load the file copied from backup
                    return load<T>(fileName, knownTypes);
                }
            }

            catch { }

            return default(T);
        }

        public static bool FileExists(string fileName)
        {
            return store.FileExists(fileName);
        }

        private static T load<T>(string fileName, params Type[] knownTypes)
        {
            using (IsolatedStorageFileStream stream = store.OpenFile(fileName, System.IO.FileMode.Open))
            {
                return (T)new DataContractSerializer(typeof(T), knownTypes).ReadObject(stream);
            }
        }

        /// <summary>
        /// Returns the file names, or an empty array if not found.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string[] GetFileNames(string searchPattern)
        {
            try
            {
                return store.GetFileNames(searchPattern);
            }

            catch { return new string[0]; }
        }

        public static Stream LoadStream(string fileName)
        {
            try
            {
                if (FileExists(fileName)) //load the file
                    return loadStream(fileName);
            }

            catch
            {
            }

            //if the file wasn't found, or if an error was thrown while loading, it'll load the backup
            try
            {
                if (FileExists(BACKUP_FOLDER + fileName))
                {
                    //then load the backup file
                    return loadStream(BACKUP_FOLDER + fileName);
                }
            }

            catch { }

            return null;
        }

        private static Stream loadStream(string fileName)
        {
            return store.OpenFile(fileName, System.IO.FileMode.Open);
        }

        public static void DeleteDirectory(string folderToRemove)
        {
            try
            {
                //get all folders inside it                Folder/*
                string[] folders = store.GetDirectoryNames(folderToRemove + "*");
                for (int i = 0; i < folders.Length; i++)
                    DeleteDirectory(folderToRemove + folders[i] + "/");

                //get all files inside it
                string[] files = store.GetFileNames(folderToRemove + "*");
                for (int i = 0; i < files.Length; i++)
                    store.DeleteFile(folderToRemove + files[i]);

                //finally, delete this directory once it's empty
                store.DeleteDirectory(folderToRemove.TrimEnd('/'));
            }

            catch
            {
                Debug.WriteLine("Failed to delete directory");
            }
        }
    }
}
