using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsPortable;

namespace Vx.Views
{
    public abstract class VxPage : VxComponent
    {
        public event EventHandler NavigatedTo;
        public event EventHandler NavigatedFrom;
        public event EventHandler ViewLostFocus;
        public event EventHandler ViewFocused;
        public event EventHandler<CancelEventArgs> BackRequested;
        public event EventHandler<bool> AllowLightDismissChanged;

        /// <summary>
        /// If false, the UI presenters will prevent any light dismiss actions from dismissing the view model (like tapping the background of the popup screen in Windows or swiping down on the view in iOS). This is true by default. If your view contains info that would be destructive if accidently dismissed (like editing/adding pages), set this to false. Views subscribe to this, so a view could dynamically set this to false after the user edited content.
        /// </summary>
        public virtual bool AllowLightDismiss
        {
            get => GetState<bool>(InitialAllowLightDismissValue);
            set
            {
                if (value != AllowLightDismiss)
                {
                    SetState(value);
                    AllowLightDismissChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Classes can override this rather than setting the property
        /// </summary>
        protected virtual bool InitialAllowLightDismissValue { get => true; }

        public bool IsCurrentNavigatedPage { get; private set; }
        public new bool IsFocused { get; private set; }

        //public VxPage(VxPage parent)
        //{
        //    // Final content starts off as this, itself
        //    Parent = parent;
        //}

        public virtual string GetPageName()
        {
            string pageName = this.GetType().Name;
            if (pageName.EndsWith("ViewModel"))
            {
                // Remove "ViewModel"
                pageName = pageName.Substring(0, pageName.Length - "ViewModel".Length);
            }

            return pageName;
        }

        private RequestedBackButtonVisibility _thisBackButtonVisibility = RequestedBackButtonVisibility.Inherit;
        /// <summary>
        /// Refers specifically to the back button visibility of this view model, NOT its children or parents.
        /// </summary>
        public RequestedBackButtonVisibility ThisBackButtonVisibility
        {
            get => GetState(RequestedBackButtonVisibility.Inherit);

            set
            {
                if (ThisBackButtonVisibility != value)
                {
                    SetState(value);
                    FinalBackButtonVisibility = GetFinalBackButtonVisibility();
                }
            }
        }

        /// <summary>
        /// Reflects desired back button visibility based on the descendants and this item.
        /// </summary>
        public RequestedBackButtonVisibility FinalBackButtonVisibility
        {
            get => GetState(RequestedBackButtonVisibility.Inherit);
            set => SetState(value);
        }

        public bool IsLoaded
        {
            get => GetState(false);
            private set => SetState(value);
        }

        private Task _loadAsyncTask;
        public async Task LoadAsync()
        {
            if (_loadAsyncTask != null)
            {
                await _loadAsyncTask;
                return;
            }

            _loadAsyncTask = LoadAsyncOverride();
            await _loadAsyncTask;

            IsLoaded = true;
        }

        protected virtual Task LoadAsyncOverride()
        {
            return Task.FromResult(true);
        }

        public new VxPage Parent { get; protected set; }

        /// <summary>
        /// Recursively finds the topmost parent.
        /// </summary>
        /// <returns></returns>
        public VxPage GetRootParent()
        {
            if (Parent != null)
                return Parent.GetRootParent();

            return this;
        }

        public T FindAncestor<T>() where T : VxPage
        {
            if (Parent == null)
                return null;

            if (Parent is T)
                return Parent as T;

            return Parent.FindAncestor<T>();
        }

        public T FindAncestorOrSelf<T>() where T : VxPage
        {
            if (this is T)
            {
                return this as T;
            }

            return FindAncestor<T>();
        }

        /// <summary>
        /// Returns true if was able to go back. Cascades upwards towards parents.
        /// </summary>
        /// <returns></returns>
        public virtual bool GoBack()
        {
            if (BackRequested != null)
            {
                var args = new CancelEventArgs();
                BackRequested(this, args);
                if (args.Cancel)
                {
                    return true;
                }
            }

            if (Parent == null)
                return false;

            return Parent.GoBack();
        }

        /// <summary>
        /// Cascades upwards towards parents.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool RemoveViewModel(VxPage model)
        {
            if (Parent == null)
                return false;

            return Parent.RemoveViewModel(model);
        }

        /// <summary>
        /// Removes the current view model
        /// </summary>
        public void RemoveViewModel()
        {
            RemoveViewModel(this);
        }

        /// <summary>
        /// Tries to remove the current view model under the handle user interaction lock. Won't throw an exception.
        /// </summary>
        public async void TryRemoveViewModelViaUserInteraction()
        {
            try
            {
                await HandleUserInteractionAsync("RemoveViewModel", delegate
                {
                    RemoveViewModel();
                });
            }
            catch (Exception ex)
            {
                ExceptionHelper.ReportHandledException(ex);
            }
        }

        protected virtual VxPage GetChildContent() { return null; }

        /// <summary>
        /// Returns the lower-most current content
        /// </summary>
        /// <returns></returns>
        public VxPage GetFinalContent()
        {
            var child = GetChildContent();
            if (child != null)
                return child.GetFinalContent();

            return this;
        }

        /// <summary>
        /// Returns the lower-most current content, whatever the user is looking at right now.
        /// Supports change notifications via PropertyChanged.
        /// If this model is the final content and doesn't have further children, it returns this.
        /// </summary>
        public VxPage FinalContent
        {
            get => GetState<VxPage>();
            set => SetState(value);
        }

        public virtual IEnumerable<VxPage> GetChildren()
        {
            return new VxPage[0];
        }

        /// <summary>
        /// Recursively gets all descendants
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VxPage> GetDescendants()
        {
            foreach (var c in GetChildren())
            {
                yield return c;

                foreach (var innerC in c.GetDescendants())
                {
                    yield return innerC;
                }
            }
        }

        private RequestedBackButtonVisibility GetFinalBackButtonVisibility()
        {
            // Get the child
            var child = GetChildContent();
            if (child != null)
            {
                // Ask it to find out the final visibility
                RequestedBackButtonVisibility childRequestedVisibility = child.GetFinalBackButtonVisibility();

                // If it's something absolute, use that
                if (childRequestedVisibility != RequestedBackButtonVisibility.Inherit)
                    return childRequestedVisibility;
            }

            // Otherwise return this item's visibility
            return ThisBackButtonVisibility;
        }

        private VxPage _currChild;
        private PropertyChangedEventHandler _childPropertyChangedHandler;
        protected void TriggerChildContentChanged(VxPage newContent)
        {
            if (_currChild != null && _childPropertyChangedHandler != null)
            {
                // Stop listening to previous child's event
                _currChild.PropertyChanged -= _childPropertyChangedHandler;
            }

            // Get the new child
            _currChild = newContent;

            if (_currChild != null)
            {
                // Listen to the new child's event
                if (_childPropertyChangedHandler == null)
                {
                    _childPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(_currChild_PropertyChanged).Handler;
                }
                _currChild.PropertyChanged += _childPropertyChangedHandler;

                // And also update final back button visibility
                UpdateFinalContent();
                UpdateFinalBackButtonVisibility();
            }

            else
            {
                // Otherwise, we don't have any content anymore, meaning that we're the final child.
                // So we still need to update back visibility
                UpdateFinalContent();
                UpdateFinalBackButtonVisibility();
            }
        }

        private VxPage _currVisibleContent;
        private PropertyChangedEventHandler _currVisibleContentPropertyChangedHandler;
        protected void TriggerVisibleContentChanged()
        {
            if (_currVisibleContent != null && _currVisibleContentPropertyChangedHandler != null)
            {
                // Stop listening to previous child's event
                _currVisibleContent.PropertyChanged -= _currVisibleContentPropertyChangedHandler;
            }

            // Get the new child
            _currVisibleContent = GetChildContent();

            if (_currVisibleContent != null)
            {
                // Listen to the new child's event
                if (_currVisibleContentPropertyChangedHandler == null)
                {
                    _currVisibleContentPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(_currVisibleContent_PropertyChanged).Handler;
                }
                _currVisibleContent.PropertyChanged += _currVisibleContentPropertyChangedHandler;

                UpdateFinalContent();
            }

            else
            {
                UpdateFinalContent();
            }
        }

        private void _currChild_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FinalBackButtonVisibility):
                    UpdateFinalBackButtonVisibility();
                    break;
            }
        }

        private void _currVisibleContent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FinalContent):
                    UpdateFinalContent();
                    break;
            }
        }

        private void UpdateFinalContent()
        {
            FinalContent = GetFinalContent();
        }

        private void UpdateFinalBackButtonVisibility()
        {
            FinalBackButtonVisibility = GetFinalBackButtonVisibility();
        }

        //public PagedViewModelWithPopups GetPopupViewModelHost()
        //{
        //    return FindAncestor<PagedViewModelWithPopups>();
        //}

        /// <summary>
        /// Happens as a result of the view becoming the current view
        /// </summary>
        public virtual void OnViewFocused()
        {
            IsFocused = true;
            ViewFocused?.Invoke(this, new EventArgs());

            // Send event down to child too
            GetChildContent()?.OnViewFocused();
        }

        /// <summary>
        /// Happens as a result of the view no longer being the current view (popup appeared, or page navigated)
        /// </summary>
        public virtual void OnViewLostFocus()
        {
            IsFocused = false;
            ViewLostFocus?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Happens as a result of a page navigation (doesn't occur for new popups)
        /// </summary>
        public virtual void OnNavigatedTo()
        {
            IsCurrentNavigatedPage = true;
            NavigatedTo?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Happens as a result of a page navigation (doesn't occur for new popups)
        /// </summary>
        public virtual void OnNavigatedFrom()
        {
            IsCurrentNavigatedPage = false;
            NavigatedFrom?.Invoke(this, new EventArgs());
        }

        private class AsyncUserInteraction
        {
            public VxPage SourceViewModel { get; private set; }
            public object Identifier { get; private set; }
            private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
            public Task Task => _completionSource.Task;
            public bool IsCanceled { get; private set; }
            private TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

            private Func<CancellationToken, Task> _operation;

            public AsyncUserInteraction(VxPage sourceViewModel, object identifier, Func<CancellationToken, Task> operation)
            {
                SourceViewModel = sourceViewModel;
                Identifier = identifier;
                _operation = operation;
            }

            private bool _started = false;
            public async void StartOperation()
            {
                if (IsCanceled)
                {
                    throw new InvalidOperationException("Interaction is already canceled");
                }

                if (_started)
                {
                    throw new InvalidOperationException("Interaction is already started");
                }

                _started = true;

                try
                {
                    await _operation(_cancellationTokenSource.Token);
                    _completionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _completionSource.TrySetException(ex);
                }
            }

            public void Cancel()
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    IsCanceled = true;
                }
            }
        }

        private AsyncUserInteraction _currentInteraction;
        private AsyncUserInteraction _newInteraction;

        protected virtual async Task<bool> HandleUserInteractionAsync(VxPage sourceViewModel, object identifier, Func<CancellationToken, Task> operation)
        {
            // Let parent handle operation
            if (Parent != null)
            {
                return await Parent.HandleUserInteractionAsync(sourceViewModel, identifier, operation);
            }

            // Note: No need to lock anything, since user interactions will always be on UI thread

            var thisInteraction = new AsyncUserInteraction(sourceViewModel, identifier, operation);

            if (_currentInteraction != null)
            {
                // If duplicate of current, merge with that
                if (HandleDuplicateUserInteraction(_currentInteraction, thisInteraction, out Task<bool> answerCurrent))
                {
                    return await answerCurrent;
                }

                // If duplicate of next new, merge with that
                if (_newInteraction != null && HandleDuplicateUserInteraction(_newInteraction, thisInteraction, out Task<bool> answerNew))
                {
                    return await answerNew;
                }

                // Cancel the task
                _currentInteraction.Cancel();

                _newInteraction = thisInteraction;

                try
                {
                    // Wait to see if it cancels, succeeds, or fails
                    await _currentInteraction.Task;

                    // Task has succeeded regardless of cancellation, so don't invoke the new operation
                    return false;
                }

                catch
                {
                    // Task has successfully been canceled, or it failed, either way, continue with new operation
                    _currentInteraction = null;
                }

                // Make sure a subsequent request didn't come in and replace this one
                if (_newInteraction != thisInteraction)
                {
                    return false;
                }

                _newInteraction = null;
            }

            // Start the operation first (it could throw an exception, don't want to assign current interaction unless we actually successfully started)
            thisInteraction.StartOperation();

            // Now store that current interaction
            _currentInteraction = thisInteraction;

            // And then when the operation is done, we clear current
            try
            {
                return await HandleCompletingUserInteractionAsync(_currentInteraction);
            }
            finally
            {
                if (thisInteraction == _currentInteraction)
                {
                    _currentInteraction = null;
                }
            }
        }

        public Task<bool> HandleUserInteractionAsync(object identifier, Action operation)
        {
            return HandleUserInteractionAsync(identifier, (cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                operation();

                return Task.FromResult(true);
            });
        }

        private static bool HandleDuplicateUserInteraction(AsyncUserInteraction existing, AsyncUserInteraction incoming, out Task<bool> existingAnswer)
        {
            if (existing.SourceViewModel == incoming.SourceViewModel && existing.Identifier == incoming.Identifier && !existing.IsCanceled && !incoming.IsCanceled)
            {
                existingAnswer = HandleCompletingUserInteractionAsync(existing);
                return true;
            }

            existingAnswer = null;
            return false;
        }

        private static async Task<bool> HandleCompletingUserInteractionAsync(AsyncUserInteraction interaction)
        {
            try
            {
                await interaction.Task;
                return true;
            }
            catch (OperationCanceledException)
            {
                // Potentially expected if task was canceled
                return false;
            }
        }

        /// <summary>
        /// Must be called from the UI thread.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="operation"></param>
        public virtual Task<bool> HandleUserInteractionAsync(object identifier, Func<CancellationToken, Task> operation)
        {
            return HandleUserInteractionAsync(this, identifier, operation);
        }

        private List<MyBinding> _bindings = new List<MyBinding>();

        protected TFinal GetBindedValue<TFinal>(INotifyPropertyChanged source, string sourcePropertyName, Func<TFinal> convert, [CallerMemberName] string targetPropertyName = null)
        {
            var existing = _bindings.FirstOrDefault(i => i.Source == source && i.SourcePropertyName == sourcePropertyName);
            if (existing == null)
            {
                existing = new MyBinding<TFinal>(source, sourcePropertyName, NotifyPropertyChanged, targetPropertyName, convert);
                _bindings.Add(existing);
            }
            return (TFinal)existing.Value;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        private abstract class MyBinding
        {
            public INotifyPropertyChanged Source { get; protected set; }
            public string SourcePropertyName { get; protected set; }
            public object Value { get; protected set; }
        }

        private class MyBinding<TFinal> : MyBinding
        {
            private Func<TFinal> _convert;
            private BindablePropertyWatcher _propertyWatcher;
            private Action<string> _notifyPropertyChanged;
            private string _targetPropertyName;

            public MyBinding(INotifyPropertyChanged source, string sourcePropertyName, Action<string> notifyPropertyChanged, string targetPropertyName, Func<TFinal> convert)
            {
                Source = source;
                SourcePropertyName = sourcePropertyName;
                _notifyPropertyChanged = notifyPropertyChanged;
                _targetPropertyName = targetPropertyName;
                _convert = convert;
                _propertyWatcher = new BindablePropertyWatcher(source, sourcePropertyName, OnPropertyChanged);

                UpdateValue();
            }

            private void OnPropertyChanged()
            {
                object original = Value;
                UpdateValue();
                object newValue = Value;

                if (!object.Equals(original, newValue))
                {
                    _notifyPropertyChanged(_targetPropertyName);
                }
            }

            private void UpdateValue()
            {
                TFinal finalValue = _convert();
                Value = finalValue;
            }
        }
    }
}
