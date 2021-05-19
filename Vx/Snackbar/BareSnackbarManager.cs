using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace BareMvvm.Core.Snackbar
{
    public class BareSnackbarManager : BindableBase
    {
        // Android uses the events since Android has a native snackbar API that already behaves how we want it to behave

        /// <summary>
        /// Event called when a snackbar is requested to show
        /// </summary>
        public event EventHandler<BareSnackbar> OnShow;

        /// <summary>
        /// Event called when a shown snackbar is requested to close
        /// </summary>
        public event EventHandler<BareSnackbar> OnClose;

        private Queue<BareSnackbar> _queuedSnackbars = new Queue<BareSnackbar>();

        private BareSnackbar _currentSnackbar;
        public BareSnackbar CurrentSnackbar
        {
            get => _currentSnackbar;
            private set
            {
                SetProperty(ref _currentSnackbar, value, nameof(CurrentSnackbar));

                if (DisplayedSnackbars.Count > 0)
                {
                    DisplayedSnackbars.Clear();
                }

                if (value != null)
                {
                    DisplayedSnackbars.Add(value);
                    HandleDecayingSnackbar(value);
                }
            }
        }

        public ObservableCollection<BareSnackbar> DisplayedSnackbars { get; private set; } = new ObservableCollection<BareSnackbar>();

        public void Show(BareSnackbar snackbar)
        {
            lock (this)
            {
                if (CurrentSnackbar == null)
                {
                    CurrentSnackbar = snackbar;
                }

                else
                {
                    _queuedSnackbars.Enqueue(snackbar);

                    if (CurrentSnackbar.SkipToNextSnackbarImmediately)
                    {
                        Close(CurrentSnackbar); // We call close so on Android it'll close too
                    }
                }
            }

            OnShow?.Invoke(this, snackbar);
        }

        private async void HandleDecayingSnackbar(BareSnackbar newlyShownSnackbar)
        {
            try
            {
                await Task.Delay(newlyShownSnackbar.Duration);

                lock (this)
                {
                    if (CurrentSnackbar != newlyShownSnackbar)
                    {
                        return;
                    }

                    MoveToNext();
                }
            }
            catch { }
        }

        private void MoveToNext()
        {
            if (_queuedSnackbars.Count > 0)
            {
                CurrentSnackbar = _queuedSnackbars.Dequeue();
            }

            else
            {
                CurrentSnackbar = null;
            }
        }

        public void Close(BareSnackbar snackbar)
        {
            lock (this)
            {
                if (CurrentSnackbar == snackbar)
                {
                    MoveToNext();
                }
                else if (_queuedSnackbars.Contains(snackbar))
                {
                    _queuedSnackbars = new Queue<BareSnackbar>(_queuedSnackbars.Except(new BareSnackbar[] { snackbar }));
                }
            }

            OnClose?.Invoke(this, snackbar);
        }
    }
}
