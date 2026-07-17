using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using PowerPlannerAppDataLibrary.Serialization;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class SavedGradeScalesManager
    {
        private IFolder _savedGradeScalesFolder;

        private SavedGradeScalesManager(IFolder savedGradeScalesFolder)
        {
            _savedGradeScalesFolder = savedGradeScalesFolder;
        }
        
        public async Task<List<SavedGradeScale>> GetSavedGradeScalesAsync()
        {
            List<SavedGradeScale> answer = new List<SavedGradeScale>();

            foreach (IFile file in await _savedGradeScalesFolder.GetFilesAsync())
            {
                SavedGradeScale scale = await LoadSavedGradeScale(file);

                if (scale != null)
                    answer.Add(scale);
            }

            return answer;
        }

        private static async Task<SavedGradeScale> LoadSavedGradeScale(IFile file)
        {
            try
            {
                GradeScale[] scales;

                using (Stream s = await file.OpenAsync(StorageEverywhere.FileAccess.Read))
                {
                    using (var reader = new StreamReader(s))
                    {
                        string contents = reader.ReadToEnd();
                        scales = contents.TrimStart().StartsWith("<")
                            ? ReadLegacyGradeScales(contents)
                            : PowerPlannerJson.Deserialize<GradeScale[]>(contents);
                    }
                }

                if (scales == null)
                    return null;

                return new SavedGradeScale()
                {
                    Name = file.Name,
                    GradeScales = scales
                };
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                return null;
            }
        }

        /// <summary>
        /// Throws exception if saving failed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scales"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task SaveGradeScale(string name, GradeScale[] scales)
        {
            IFile file = await _savedGradeScalesFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            using (Stream s = await file.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
            {
                using (var writer = new StreamWriter(s))
                {
                    writer.Write(PowerPlannerJson.Serialize(scales));
                }
            }
        }

        private static GradeScale[] ReadLegacyGradeScales(string contents)
        {
            return XDocument.Parse(contents)
                .Descendants()
                .Where(element => element.Name.LocalName == "GradeScale")
                .Select(element => new GradeScale
                {
                    StartGrade = double.Parse(element.Elements().First(child => child.Name.LocalName == "StartGrade").Value, CultureInfo.InvariantCulture),
                    GPA = double.Parse(element.Elements().First(child => child.Name.LocalName == "GPA").Value, CultureInfo.InvariantCulture)
                })
                .ToArray();
        }

        /// <summary>
        /// Returns null if account doesn't exist.
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        private static async Task<SavedGradeScalesManager> GetForOfflineAccountAsync(Guid localAccountId)
        {
            IFolder savedGradeScalesFolder = await FileHelper.GetSavedGradeScalesFolder(localAccountId);

            if (savedGradeScalesFolder == null)
                return null;

            return new SavedGradeScalesManager(savedGradeScalesFolder);
        }

        /// <summary>
        /// Always returns initialized object (doesn't check if account doesn't exist).
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private static async Task<SavedGradeScalesManager> GetForOnlineAccountAsync(long accountId)
        {
            IFolder savedGradeScalesFolder = await RoamingHelper.GetSavedGradeScalesFolder(accountId);

            return new SavedGradeScalesManager(savedGradeScalesFolder);
        }

        /// <summary>
        /// Automatically handles the difference between offline and online account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static async Task<SavedGradeScalesManager> GetForAccountAsync(AccountDataItem account)
        {
            if (account.IsOnlineAccount && FileSystem.Current.RoamingStorage != null)
                return await GetForOnlineAccountAsync(account.AccountId);

            else
                return await GetForOfflineAccountAsync(account.LocalAccountId);
        }

        public static async System.Threading.Tasks.Task TransferToOnlineAccountAsync(Guid localAccountId, long accountId)
        {
            // If roaming isn't supported, do nothing
            if (FileSystem.Current.RoamingStorage == null)
                return;

            var localManager = await GetForOfflineAccountAsync(localAccountId);

            if (localManager == null)
                return;

            var onlineManager = await GetForOnlineAccountAsync(accountId);

            var savedScales = await localManager.GetSavedGradeScalesAsync();

            foreach (var s in savedScales)
                await onlineManager.SaveGradeScale(s.Name, s.GradeScales);
        }
    }

    public class SavedGradeScale
    {
        public string Name { get; set; }

        public GradeScale[] GradeScales { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
