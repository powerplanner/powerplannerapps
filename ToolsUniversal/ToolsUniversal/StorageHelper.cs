using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace ToolsUniversal
{
    public class StorageHelper
    {
        private static readonly StorageFolder LOCAL_FOLDER = ApplicationData.Current.LocalFolder;

        public static async Task<StorageFolder> CreateFolder(string fileName)
        {
            return await LOCAL_FOLDER.CreateFolderAsync(fileName, CreationCollisionOption.OpenIfExists);
        }


        #region FilesAndFolders

        public static async Task<StorageFolder> GetFolder(string path)
        {
            return await getFolder(path, ApplicationData.Current.LocalFolder);
        }

        private static async Task<StorageFolder> getFolder(string path, StorageFolder lastFolder)
        {
            int index = path.IndexOf('/');

            if (index == -1)
                return lastFolder;

            try
            {
                return await getFolder(path.Substring(index + 1), await lastFolder.GetFolderAsync(path.Substring(0, index + 1)));
            }

            catch { return null; }
        }

        private static async Task<StorageFile> getFile(string fileName)
        {
            StorageFolder folder = await getFolder(fileName, ApplicationData.Current.LocalFolder);
            if (folder == null)
                return null;

            try
            {
                return await folder.GetFileAsync(getFileName(fileName));
            }

            catch { return null; }
        }

        private static string getFileName(string fileName)
        {
            int index = fileName.LastIndexOf('/');
            if (index == -1)
                return fileName;
            return fileName.Substring(index + 1);
        }

        private static async Task<Stream> createFileStream(string fileName)
        {
            return await (await (await GetFolder(fileName)).CreateFileAsync(getFileName(fileName), CreationCollisionOption.ReplaceExisting)).OpenStreamForWriteAsync();
        }


        #endregion


        # region Save

        /// <summary>
        /// Does NOT automatically close the data stream
        /// </summary>
        /// <param name="fileName">Shouldn't have any file paths</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task Save(StorageFolder folder, string fileName, Stream data)
        {
            using (Stream destination = await folder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
            {
                data.CopyTo(destination);
            }
        }

        /// <summary>
        /// Serializes object first before doing the async call, so it should be fine for threading concerns
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task Save(StorageFolder folder, string fileName, object data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new DataContractSerializer(data.GetType()).WriteObject(stream, data);
                stream.Position = 0;

                await Save(folder, fileName, stream);
            }
        }

        # endregion

        # region Load

        public static async Task<T> Load<T>(StorageFolder folder, string fileName, params Type[] knownTypes)
        {
            try
            {
                using (Stream stream = await LoadStream(folder, fileName))
                {
                    if (stream == null)
                        return default(T);

                    return (T)new DataContractSerializer(typeof(T)).ReadObject(stream);
                }
            }

            catch { }

            return default(T);
        }

        public static async Task<T> Load<T>(StorageFile file, params Type[] knownTypes)
        {
            try
            {
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    if (stream == null)
                        return default(T);

                    return (T)new DataContractSerializer(typeof(T)).ReadObject(stream);
                }
            }

            catch { }

            return default(T);
        }

        public static async Task<Stream> LoadStream(StorageFolder folder, string fileName)
        {
            try
            {
                if (folder == null)
                    return null;

                return await folder.OpenStreamForReadAsync(fileName);
            }

            catch { }

            return null;
        }


        # endregion



        # region OpenFolder

        /// <summary>
        /// Should end with a '/'. If not exists, creates the folder. Then returns the folder.
        /// </summary>
        /// <returns>Returns a reference to the storage folder.</returns>
        /// <param name="fileName"></param>
        public static async Task<StorageFolder> OpenFolder(string fileName)
        {
            int index = fileName.IndexOf('/');
            StorageFolder lastFolder = ApplicationData.Current.LocalFolder;

            int start = 0;
            int end = fileName.IndexOf('/');

            while (end != -1)
            {
                string nextFolder = fileName.Substring(start, end - start + 1);
                bool foundFolder = false;

                try
                {
                    //grab the next folder
                    lastFolder = await lastFolder.GetFolderAsync(nextFolder);

                    foundFolder = true;
                }

                catch { }

                //need to create the folder
                if (!foundFolder)
                {
                    lastFolder = await lastFolder.CreateFolderAsync(nextFolder);
                }

                //increment in file path
                start = end + 1;
                end = fileName.IndexOf('/', start);
            }

            return lastFolder;
        }

        # endregion


        public static Stream JsonSerialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            new DataContractJsonSerializer(obj.GetType()).WriteObject(stream, obj);
            stream.Position = 0;

            return stream;
        }

        public static async Task<BitmapImage> GetThumbnailBitmapAsync(StorageFile file, ThumbnailMode mode, uint requestedSize, ThumbnailOptions options)
        {
            var thumbnail = await file.GetThumbnailAsync(mode, requestedSize, options);

            BitmapImage img = new BitmapImage();
            await img.SetSourceAsync(thumbnail);

            return img;
        }

        public static async Task<BitmapImage> GetThumbnailBitmapAsync(StorageFolder folder, ThumbnailMode mode, uint requestedSize, ThumbnailOptions options)
        {
            var thumbnail = await folder.GetThumbnailAsync(mode, requestedSize, options);

            BitmapImage img = new BitmapImage();
            await img.SetSourceAsync(thumbnail);

            return img;
        }
    }
}
