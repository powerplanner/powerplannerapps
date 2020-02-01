using StorageEverywhere;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public class TempFile : IFile
    {
        private static object _lock = new object();
        private static Task _firstInitialization;
        private const string FolderName = "TempPPAppDataLibraryHelperFolder";

        private IFile _file;

        public string Name => _file.Name;

        public string Path => _file.Path;

        public static async Task<TempFile> CreateAsync(string fileName)
        {
            lock (_lock)
            {
                if (_firstInitialization == null)
                {
                    _firstInitialization = ClearTempFolderAsync();
                }
            }
            await _firstInitialization;

            var tempFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists);
            var tempFile = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            return new TempFile()
            {
                _file = tempFile
            };
        }

        private static async Task ClearTempFolderAsync()
        {
            try
            {
                IFolder folder = await FileSystem.Current.LocalStorage.GetFolderAsync(FolderName);
                await folder.DeleteAsync();
            }
            catch { }
        }

        ~TempFile()
        {
            Dispose();
        }

        private bool _disposed;
        /// <summary>
        /// This will be automatically called when destructor is called, but apps can choose to call it earlier
        /// </summary>
        public async void Dispose()
        {
            try
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                await _file.DeleteAsync();
            }
            catch { }
        }

        public Task<Stream> OpenAsync(StorageEverywhere.FileAccess fileAccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _file.OpenAsync(fileAccess, cancellationToken);
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _file.DeleteAsync(cancellationToken);
        }

        public Task RenameAsync(string newName, NameCollisionOption collisionOption = NameCollisionOption.FailIfExists, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _file.RenameAsync(newName, collisionOption, cancellationToken);
        }

        public Task MoveAsync(string newPath, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _file.MoveAsync(newPath, collisionOption, cancellationToken);
        }

        /// <summary>
        /// Stop tracking as temp file, it's been promoted to real file
        /// </summary>
        public void DetachTempDisposer()
        {
            _disposed = true;
        }
    }
}
