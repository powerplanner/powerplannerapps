using BareMvvm.Core.ViewModels;
using Newtonsoft.Json.Linq;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class ImageUploadOptionsViewModel : PopupComponentViewModel
    {
        private AccountDataItem _account;

        public ImageUploadOptionsViewModel(BaseViewModel parent) : base(parent)
        {
            Title = R.S("Settings_ImageUploadOptionsPage_Header.Text");

            _account = MainScreenViewModel.CurrentAccount;
            if (_account != null)
            {
                _selectedUploadOption = _account.ImageUploadOption;
                IsEnabled = true;
            }

            UploadOptionStrings = UploadOptions.Select(i => OptionToString(i)).ToArray();
        }

        public ImageUploadOptions[] UploadOptions { get; private set; } = new ImageUploadOptions[]
        {
            ImageUploadOptions.Always,
            ImageUploadOptions.WifiOnly,
            ImageUploadOptions.Never
        };

        public string[] UploadOptionStrings { get; private set; }

        public static string OptionToString(ImageUploadOptions value)
        {
            return ImageUploadOptionToStringConverter.Convert(value);
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value, nameof(IsEnabled)); }
        }

        private ImageUploadOptions _selectedUploadOption;
        public ImageUploadOptions SelectedUploadOption
        {
            get { return _selectedUploadOption; }
            set
            {
                if (_selectedUploadOption != value)
                {
                    _selectedUploadOption = value;
                    OnPropertyChanged(nameof(SelectedUploadOption));
                }

                if (_account.ImageUploadOption != value)
                {
                    _account.ImageUploadOption = value;
                    SaveAndUpdate();
                }
            }
        }

        private async void SaveAndUpdate()
        {
            try
            {
                await AccountsManager.Save(_account);

                // Sync so that images will be uploaded
                if (_account.ImageUploadOption != ImageUploadOptions.Never)
                {
                    try
                    {
                        var dontWait = Sync.SyncAccountAsync(_account);
                    }
                    catch { }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(
                new TextBlock
                {
                    Text = R.S("Settings_ImageUploadOptionsPage_Description.Text")
                },

                new ComboBox
                {
                    Header = R.S("Settings_ImageUploadOptionsPage_ComboBoxUploadOptions.Header"),
                    Items = UploadOptionStrings,
                    SelectedItem = VxValue.Create<object>(OptionToString(SelectedUploadOption), i => SelectedUploadOption = UploadOptions[Array.IndexOf(UploadOptionStrings, i)]),
                    IsEnabled = IsEnabled,
                    Margin = new Thickness(0, 12, 0, 0)
                }
            );
        }
    }
}
