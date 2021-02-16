using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace PowerPlannerUWP.Extensions
{
    public class UWPSoundsExtension : SoundsExtension
    {
        private MediaElement _mediaElement;

        public override async void TryPlayTaskCompletedSound()
        {
            try
            {
                if (_mediaElement == null)
                {
                    _mediaElement = new MediaElement();
                    _mediaElement.Volume = 0.2;
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Sounds/TaskCompleted.mp3"));
                    _mediaElement.SetSource(await file.OpenReadAsync(), file.ContentType);
                }

                _mediaElement.Position = TimeSpan.Zero;
                _mediaElement.Play();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
