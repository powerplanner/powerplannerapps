using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class BaseMainScreenViewModelDescendant : BaseViewModel
    {
        public BaseMainScreenViewModelDescendant(BaseViewModel parent) : base(parent)
        {
        }

        public virtual MainScreenViewModel MainScreenViewModel
        {
            get
            {
                var ancestor = Parent;
                while (ancestor != null)
                {
                    if (ancestor is MainScreenViewModel answer)
                    {
                        return answer;
                    }

                    ancestor = ancestor.Parent;
                }

                throw new NullReferenceException("Couldn't find MainScreenViewModel");
            }
        }

        private List<MainScreenViewModel.IDataChangeListener> _listeners = new List<MainScreenViewModel.IDataChangeListener>();
        protected MainScreenViewModel.ChangedItemListener ListenToItem(Guid itemIdentifier)
        {
            var listener = MainScreenViewModel.ListenToItem(itemIdentifier);

            // We add to an instance variable list, so that the reference won't get lost until the view model gets destroyed
            _listeners.Add(listener);

            return listener;
        }

        protected MainScreenViewModel.ItemsEditedLocallyListener<T> ListenToLocalEditsFor<T>() where T : BaseDataItem
        {
            var listener = MainScreenViewModel.ListenToLocalEditsFor<T>();

            // We add to an instance variable list, so that the reference won't get lost until the view model gets destroyed
            _listeners.Add(listener);

            return listener;
        }

        public static async void TryStartDataOperationAndThenNavigate(Func<Task> dataOperation, Action navigateOperation, [CallerMemberName]string callerFunctionName = "", [CallerFilePath]string callerFilePath = "")
        {
            try
            {
                var task = dataOperation();

                navigateOperation();

                DateTime start = DateTime.Now;

                await task;

                TimeSpan duration = DateTime.Now - start;
                if (duration.TotalSeconds > 1.0)
                {
                    // If these are taking longer than a second, that gives us an idea that we need to
                    // implement UI to show that the data operation is occurring
                    TelemetryExtension.Current?.TrackEvent("SlowUIDataOperation", new Dictionary<string, string>()
                    {
                        { "CallerFilePath", callerFilePath.Split('\\').LastOrDefault() ?? "" },
                        { "Duration", duration.TotalSeconds.ToString("0.0") },
                        { "CallerFunction", callerFunctionName }
                    });
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                try
                {
                    await new PortableMessageDialog("Failed to save changes. Your error has been reported to the developer.", "Error").ShowAsync();
                }
                catch { }
            }
        }

        public Task<bool> TryHandleUserInteractionAsync(object identifier, Func<System.Threading.CancellationToken, Task> operation, string errorMessage = null, [CallerMemberName]string callerFunctionName = "", [CallerFilePath]string callerFilePath = "")
        {
            return HandleUserInteractionAsync(identifier, async (cancellationToken) =>
            {
                try
                {
                    DateTime start = DateTime.Now;

                    await operation(cancellationToken);

                    TimeSpan duration = DateTime.Now - start;
                    if (duration.TotalSeconds > 1.0)
                    {
                        // If these are taking longer than a second, that gives us an idea that we need to
                        // implement UI to show that the data operation is occurring
                        TelemetryExtension.Current?.TrackEvent("SlowInteractionOperation", new Dictionary<string, string>()
                        {
                            { "CallerFilePath", callerFilePath.Split('\\').LastOrDefault() ?? "" },
                            { "Duration", duration.TotalSeconds.ToString("0.0") },
                            { "CallerFunction", callerFunctionName }
                        });
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);

                    if (errorMessage != null)
                    {
                        var dontWait = new PortableMessageDialog(errorMessage, "Error").ShowAsync();
                    }
                }
            });
        }
    }
}
