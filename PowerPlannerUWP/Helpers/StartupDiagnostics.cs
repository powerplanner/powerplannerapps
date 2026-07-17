using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PowerPlannerUWP.Helpers
{
    internal static class StartupDiagnostics
    {
        private const string StageSetting = "StartupDiagnostics.Stage";
        private const string FailureFileName = "startup-failure.txt";

        public static string CurrentStage { get; private set; } = "Application construction";

        public static void SetStage(string stage)
        {
            CurrentStage = stage;

            try
            {
                ApplicationData.Current.LocalSettings.Values[StageSetting] = $"{DateTimeOffset.UtcNow:O} {stage}";
            }
            catch
            {
            }
        }

        public static async Task RecordFailureAsync(Exception exception)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(FailureFileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, $"Stage: {CurrentStage}\r\nTime: {DateTimeOffset.UtcNow:O}\r\n\r\n{exception}");
            }
            catch
            {
            }
        }

        public static async Task ShowFailureAsync(Exception exception)
        {
            await RecordFailureAsync(exception);

            try
            {
                var details = new TextBlock
                {
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    IsTextSelectionEnabled = true,
                    Text = $"Power Planner failed during startup.\n\nStage: {CurrentStage}\n\n{exception}\n\nA copy was written to LocalState\\{FailureFileName}.",
                    TextWrapping = TextWrapping.Wrap
                };

                Window.Current.Content = new Grid
                {
                    Background = new SolidColorBrush(Colors.Black),
                    Padding = new Thickness(24),
                    Children =
                    {
                        new ScrollViewer { Content = details }
                    }
                };
                Window.Current.Activate();
            }
            catch
            {
            }
        }
    }
}