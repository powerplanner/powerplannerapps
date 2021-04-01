using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ToolsUniversal
{
    public static class StorageFileExtensions
    {
        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Task<StorageFile> CreateFile(string fileName)
        {
            //ApplicationData.Current.LocalFolder.CreateFileAsync()
            throw new NotImplementedException();
        }
    }
}
