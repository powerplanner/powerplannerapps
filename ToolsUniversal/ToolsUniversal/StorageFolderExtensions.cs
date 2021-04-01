using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ToolsUniversal
{
    public static class StorageFolderExtensions
    {
        public static async Task<StorageFile> CreateFileByPathAsync(this StorageFolder startingFolder, string[] path, string fileName, CreationCollisionOption collisionOption)
        {
            StorageFolder folder = await startingFolder.CreateFolderByPathAsync(path);

            return await folder.CreateFileAsync(fileName, collisionOption);
        }

        public static async Task<StorageFolder> CreateFolderByPathAsync(this StorageFolder startingFolder, string[] path)
        {
            StorageFolder currFolder = startingFolder;

            for (int i = 0; i < path.Length; i++)
            {
                currFolder = await currFolder.CreateFolderAsync(path[i], CreationCollisionOption.OpenIfExists);
            }

            return currFolder;
        }

        public static async Task<StorageFile> GetFileByPathAsync(this StorageFolder startingFolder, string[] path, string fileName)
        {
            StorageFolder folder = await startingFolder.GetFolderByPathAsync(path);

            return await folder.GetFileAsync(fileName);
        }

        /// <summary>
        /// Returns the folder under the specified path if it exists. Otherwise throws FileNotFound exception (same stuff as GetFolderAsync).
        /// </summary>
        /// <param name="startingFolder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<StorageFolder> GetFolderByPathAsync(this StorageFolder startingFolder, string[] path)
        {
            StorageFolder currFolder = startingFolder;

            for (int i = 0; i < path.Length; i++)
            {
                currFolder = await currFolder.GetFolderAsync(path[i]);
            }

            return currFolder;
        }
    }
}
