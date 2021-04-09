using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using ToolsPortable;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Vx.Views
{
    public abstract class VxComponent : ContentView
    {
        private bool _isRootComponent;
        /// <summary>
        /// When adding a VxComponent to normal Xamarin Forms view, you must set this so that it displays
        /// </summary>
        public bool IsRootComponent
        {
            get => _isRootComponent;
            set
            {
                if (_isRootComponent && value == false)
                {
                    throw new NotSupportedException("Cannot change from root element to not root");
                }

                _isRootComponent = value;

                if (value)
                {
                    InitializeRootComponent();
                }
            }
        }

        protected virtual bool IsDependentOnBindingContext => false;

        private object _oldBindingContext;
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (_oldBindingContext is INotifyPropertyChanged oldValPropChanged)
            {
                UnsubscribeToPropertyValue(oldValPropChanged);
            }

            _oldBindingContext = BindingContext;

            if (BindingContext is INotifyPropertyChanged newValPropChanged)
            {
                if (_propertyValuePropertyChangedHandler == null)
                {
                    _propertyValuePropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(PropertyValue_PropertyChanged).Handler;
                }

                SubscribeToPropertyValue(newValPropChanged);
            }

            if (IsDependentOnBindingContext && BindingContext != null)
            {
                InitializeForDisplay();
            }
        }

        private void InitializeRootComponent()
        {
            if (_hasInitializedForDisplay)
            {
                throw new NotSupportedException("You must set IsRootComponent before placing the component in your views.");
            }

            base.DescendantAdded += VxComponent_DescendantAdded;
        }

        private List<VxComponent> _additionalComponentsToInitialize;

        private void VxComponent_DescendantAdded(object sender, ElementEventArgs e)
        {
            if (e.Element is VxComponent component)
            {
                if (_hasInitializedForDisplay)
                {
                    component.InitializeForDisplay();
                }
                else
                {
                    if (_additionalComponentsToInitialize == null)
                    {
                        _additionalComponentsToInitialize = new List<VxComponent>();
                    }
                    _additionalComponentsToInitialize.Add(component);
                }
            }
        }

        private Dictionary<string, object> _states = new Dictionary<string, object>();

        [Obsolete("Use VxState instead")]
        public T GetState<T>(T defaultValue = default(T), [CallerMemberName]string propertyName = null)
        {
            if (_states.TryGetValue(propertyName, out object val))
            {
                return (T)val;
            }

            return defaultValue;
        }

        [Obsolete("Use VxState instead")]
        public void SetState<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (_states.TryGetValue(propertyName, out object existingVal) && object.Equals(existingVal, value))
            {
                return;
            }

            _states[propertyName] = value;
            OnPropertyChanged(propertyName);

            MarkDirty();
        }

        protected override void OnParentSet()
        {
            // Only the root component renders at this time
            // When a root component is rendering views that contain another nested component, the final parent will only be the immediate parent, not the root component
            if (!IsRootComponent)
            {
                return;
            }

            InitializeForDisplay();
        }

        private bool _hasInitializedForDisplay;
        private void InitializeForDisplay()
        {
            if (IsDependentOnBindingContext && BindingContext == null)
            {
                return;
            }

            if (_hasInitializedForDisplay)
            {
                return;
            }

            _hasInitializedForDisplay = true;

            Initialize();

            if (IsRootComponent && _additionalComponentsToInitialize != null)
            {
                foreach (var c in _additionalComponentsToInitialize)
                {
                    c.InitializeForDisplay();
                }

                _additionalComponentsToInitialize = null;
            }

            var renderedContentContainer = PrepRenderedContentContainer();
            if (renderedContentContainer != null)
            {
                base.Content = renderedContentContainer;
            }

            SubscribeToStates();
            SubscribeToProperties();

            RenderActual();
        }

        protected virtual View PrepRenderedContentContainer()
        {
            // Nothing in this case since we just set content directly
            return null;
        }

        private Element GetFinalParent()
        {
            Element view = this;
            while (view.Parent != null)
            {
                view = view.Parent;
            }

            return view;
        }

        private class VxCommand : ICommand
        {
            private Action _action;
            public VxCommand(Action action)
            {
                _action = action;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _action();
            }
        }

        protected ICommand CreateCommand(Action action)
        {
            return new VxCommand(action);
        }

        private Dictionary<string, DataTemplate> _dataTemplates;
        protected DataTemplate CreateViewCellItemTemplate<T>(string templateName, Func<T, View> render)
        {
            if (_dataTemplates == null)
            {
                _dataTemplates = new Dictionary<string, DataTemplate>();
            }

            if (_dataTemplates.TryGetValue(templateName, out DataTemplate existing))
            {
                return existing;
            }

            var newTemplate = new VxViewCellItemTemplate<T>(render);
            _dataTemplates[templateName] = newTemplate;
            return newTemplate;
        }

        private void SubscribeToStates()
        {
            var stateType = typeof(VxState);
            foreach (var prop in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => i.CanRead && stateType.IsAssignableFrom(i.PropertyType)))
            {
                var state = prop.GetValue(this) as VxState;
                state.ValueChanged += State_ValueChanged;
            }
            foreach (var prop in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => stateType.IsAssignableFrom(i.FieldType)))
            {
                var state = prop.GetValue(this) as VxState;
                state.ValueChanged += State_ValueChanged;
            }
        }

        private PropertyChangedEventHandler _propertyValuePropertyChangedHandler;
        private static Type _iNotifyPropertyChangedType = typeof(INotifyPropertyChanged);

        private void SubscribeToProperties()
        {
            _propertyValuePropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(PropertyValue_PropertyChanged).Handler;

            foreach (var prop in this.GetType().GetProperties().Where(i => i.CanWrite && i.CanRead && _iNotifyPropertyChangedType.IsAssignableFrom(i.PropertyType) && i.GetCustomAttribute<VxSubscribeAttribute>() != null))
            {
                var propVal = prop.GetValue(this) as INotifyPropertyChanged;
                if (propVal != null)
                {
                    SubscribeToPropertyValue(propVal);
                }
            }
        }

        private void SubscribeToPropertyValue(INotifyPropertyChanged propertyValue)
        {
            propertyValue.PropertyChanged += _propertyValuePropertyChangedHandler;
        }

        private void UnsubscribeToPropertyValue(INotifyPropertyChanged propertyValue)
        {
            propertyValue.PropertyChanged -= _propertyValuePropertyChangedHandler;
        }

        private void PropertyValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MarkDirty();
        }

        protected abstract View Render();

        private void State_ValueChanged(object sender, EventArgs e)
        {
            MarkDirty();
        }

        private bool _dirty = true;

        /// <summary>
        /// Marks this component for re-render on next UI cycle
        /// </summary>
        protected void MarkDirty()
        {
            lock (this)
            {
                if (_dirty)
                {
                    return;
                }

                _dirty = true;
            }

            MainThread.BeginInvokeOnMainThread(RenderActual);
        }

        public static int LastMillisecondsToRender { get; private set; }

        private static Type _listOfViewsType = typeof(IList<View>);
        private static Type _viewType = typeof(View);
        private static Type _visualElementType = typeof(VisualElement);
        private static Type _iVisualType = typeof(IVisual);
        private static Type _entryType = typeof(Entry);
        private static Type _listViewType = typeof(ListView);
        private static Type _pickerType = typeof(Picker);
        private static Type _resourceDictionaryType = typeof(ResourceDictionary);
        private static Type _vxComponentType = typeof(VxComponent);
        private static Type _imageSourceType = typeof(ImageSource);

        /// <summary>
        ///  Properties that shouldn't be set (for internal renderer use only)
        /// </summary>
        //private static PropertyInfo[] _propertiesToIgnore = new PropertyInfo[]
        //{
        //    typeof(VisualElement).GetProperty(nameof(Batched)),
        //    typeof(VisualElement).GetProperty(nameof(IsInNativeLayout)),
        //    typeof(VisualElement).GetProperty(nameof(Visual)),
        //    typeof(VisualElement).GetProperty(nameof(IsNativeStateConsistent)),
        //    typeof(VisualElement).GetProperty(nameof(DisableLayout)),
        //    typeof(VisualElement).GetProperty(nameof(IsPlatformEnabled))
        //};

        private void RenderActual()
        {
            lock (this)
            {
                _dirty = false;
            }

            var now = DateTime.Now;

            View newView = Render();
            View oldView = RenderedContent;

            if (oldView == null || oldView.GetType() != newView.GetType())
            {
                RenderedContent = newView;
            }

            else
            {
                // Transfer over the properties
                ReconcileViewOfSameType(oldView, newView);
            }

            LastMillisecondsToRender = (DateTime.Now - now).Milliseconds;
        }

        /// <summary>
        /// Can override if you want to wrap rendered content in something else
        /// </summary>
        protected virtual View RenderedContent
        {
            get => base.Content;
            set => base.Content = value;
        }

        /// <summary>
        /// Do NOT get or set this
        /// </summary>
        public new View Content
        {
            get => throw new InvalidOperationException("Don't call VxComponent.Content");
        }

        /// <summary>
        /// This is called only once, before Render is called, and only called when the component is actually going to be displayed (not called for the virtual components that will be discarded). No need to call base.Initialize().
        /// </summary>
        protected virtual void Initialize()
        {
            // Nothing
        }

        private static void ReconcileViewOfSameType(View oldView, View newView)
        {
            // When properties are overridden with new keyword, need to skip evaluating those, as reflection returns all and doesn't allow differentiating, so we track which are seen so far and skip dupes (the highest level props are returned frist)
            HashSet<string> seenProps = new HashSet<string>();
            var props = newView.GetType().GetProperties().Where(i => i.CanRead && _visualElementType.IsAssignableFrom(i.DeclaringType)).ToArray();

            // First have to set ItemsSource before setting 
            //var itemsSourceProp = newView.GetType().GetProperty("ItemsSource");
            //if (itemsSourceProp != null)
            //{
            //    seenProps.Add(itemsSourceProp.Name);
            //}

            // Transfer over the properties (we only look at properties at VisualElement (contains backgroundcolor, etc) and above, since if we transfer properties from lower like Element, things go bad)
            foreach (var prop in newView.GetType().GetProperties().Where(i => i.CanRead && _visualElementType.IsAssignableFrom(i.DeclaringType)))
            {
#if DEBUG
                try
                {
#endif
                    // We already handled the property (the upper class declared the property using "new" modifier, don't want to assign lower-level ones)
                    if (!seenProps.Add(prop.Name))
                    {
                        continue;
                    }

                    var propType = prop.PropertyType;

                    if (!prop.CanWrite)
                    {
                        // Children and other similar properties
                        if (_listOfViewsType.IsAssignableFrom(propType))
                        {
                            var newVal = prop.GetValue(newView);

                            ReconcileList(prop.GetValue(oldView) as IList<View>, newVal as IList<View>);
                        }
                    }
                    else
                    {
                        // Skip assigning Resources properties
                        if (_resourceDictionaryType.IsAssignableFrom(prop.PropertyType))
                        {
                            continue;
                        }

                        // Don't assign internal renderer properties
                        if (prop.DeclaringType == _visualElementType)
                        {
                            switch (prop.Name)
                            {
                                case nameof(Batched):
                                case nameof(IsInNativeLayout):
                                case nameof(Visual):
                                case nameof(IsNativeStateConsistent):
                                case nameof(DisableLayout):
                                case nameof(IsPlatformEnabled):
                                    continue;
                            }
                        }

                        // Don't assign text-related fields that are based on current editing values
                        if (prop.DeclaringType == _entryType)
                        {
                            switch (prop.Name)
                            {
                                case nameof(Entry.CursorPosition):
                                case nameof(Entry.SelectionLength):
                                    continue;
                            }
                        }

                        // Don't assign ListView SelectedItem
                        if (prop.DeclaringType == _listViewType)
                        {
                            switch (prop.Name)
                            {
                                case nameof(ListView.SelectedItem):
                                    continue;
                            }
                        }

                        // Don't assign Picker SelectedItem or SelectedIndex
                        if (prop.DeclaringType == _pickerType)
                        {
                            switch (prop.Name)
                            {
                                case nameof(Picker.SelectedItem):
                                case nameof(Picker.SelectedIndex):
                                    continue;

                                case nameof(Picker.ItemDisplayBinding):
                                    if (!object.ReferenceEquals(prop.GetValue(oldView), prop.GetValue(newView)))
                                    {
#if DEBUG
                                        System.Diagnostics.Debugger.Break();
#endif
                                        throw new InvalidOperationException("Changing ItemDisplayBinding isn't supported. You should define this as a field and re-use the same value in each Render.");
                                    }
                                    break;
                            }
                        }

                        // Don't re-assign ImageSources
                        if (prop.PropertyType == _imageSourceType)
                        {
                            continue;
                        }

                        // For ListView, if ItemsSource was set, and is changing to a different list and had a currently selected item
                        if (newView is ListView newListView && oldView is ListView oldListView && oldListView.SelectedItem != null && newListView.ItemsSource != null && oldListView.ItemsSource != null && !object.Equals(oldListView.ItemsSource, newListView.ItemsSource) && VxListViewExtensions.GetBindSelectedItem(oldListView) != null)
                        {
                            // If the new listview specified a selected item
                            var selected = VxListViewExtensions.GetBindSelectedItem(newListView);
                            if (selected != null && selected.Value != null)
                            {
                                // And if the new list contains that...
                                bool contains = false;
                                foreach (var el in newListView.ItemsSource)
                                {
                                    if (object.Equals(el, selected.Value))
                                    {
                                        contains = true;
                                        break;
                                    }
                                }

                                if (contains)
                                {
                                    // Then we have to ignore the next event which will set the selected item to null, since it'll subsequently get set to the new selected value
                                    VxListViewExtensions.ItemSelectedToIgnore.Add(oldListView);
                                }
                            }
                        }

                        // If a View property
                        // If the view is a VxComponent, we want to apply its properties like margin or background color, but NOT reconcile its Content... since that'll be updated by the VxComponent itself... Note that this is already handled by VxComponent declaring a new Content property that's get-only (and so the underneath properties get caught above earlier)
                        // Also note that we DO want to transfer over view properties on VxComponent if they're properties of the component, like a custom component can declare a view as a property that parent components can set... that DOES need to be transferred over (which will later be reconciled in the render)... we'll let the else transfer it over since that also calls mark dirty
                        if (_viewType.IsAssignableFrom(propType) && !(oldView is VxComponent))
                        {
                            var newVal = prop.GetValue(newView);

                            var oldVal = prop.GetValue(oldView);
                            if (oldVal == null || oldVal.GetType() != newVal.GetType())
                            {
                                prop.SetValue(oldView, newVal);
                            }
                            else
                            {
                                ReconcileViewOfSameType(oldVal as View, newVal as View);
                            }
                        }
                        else
                        {
                            var newVal = prop.GetValue(newView);

                            // For updating properties... if this was a component's property...
                            if (oldView is VxComponent existingComponent && _vxComponentType.IsAssignableFrom(prop.DeclaringType))
                            {
                                // First get the existing value
                                var oldVal = prop.GetValue(oldView);

                                // If it's new
                                if (!object.Equals(oldVal, newVal))
                                {
                                    // Transfer the value and mark dirty
                                    prop.SetValue(oldView, newVal);
                                    existingComponent.MarkDirty();

                                    if (prop.GetCustomAttribute<VxSubscribeAttribute>() != null)
                                    {
                                        // Unsubscribe from old property
                                        if (oldVal is INotifyPropertyChanged oldValPropChanged)
                                        {
                                            existingComponent.UnsubscribeToPropertyValue(oldValPropChanged);
                                        }

                                        // And subscribe to that property
                                        if (newVal is INotifyPropertyChanged newValPropChanged)
                                        {
                                            existingComponent.SubscribeToPropertyValue(newValPropChanged);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Just set the value like normal
                                prop.SetValue(oldView, newVal);
                            }

                        }
                    }
#if DEBUG
                }

                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    System.Diagnostics.Debugger.Break();
                }
#endif
            }

            // Transfer bindings
            VxBindings.TransferBindings(newView, oldView);

            // Handle ListView ItemSelected
            //if (newView is ListView newListView)
            //{
            //    if (oldView == null)
            //    {
            //        // Assign SelectedItem
            //        var selectedItemState = VxListViewExtensions.GetBindSelectedItem(newListView);
            //        if (selectedItemState != null)
            //        {
            //            newListView.SelectedItem = selectedItemState.Value;
            //        }

            //        // Subscribe to ItemSelected
            //        newListView.ItemSelected += ListView_ItemSelected;
            //    }

            //    else if (oldView is ListView oldListView)
            //    {
            //        // Transfer the binding
            //        var selectedItemState = VxListViewExtensions.GetBindSelectedItem(newListView);
            //        VxListViewExtensions.SetBindSelectedItem(oldListView, selectedItemState);

            //        // Assign SelectedItem
            //        if (selectedItemState != null)
            //        {
            //            oldListView.SelectedItem = selectedItemState.Value;
            //        }
            //    }
            //}

            // Transfer attached properties
            Grid.SetRow(oldView, Grid.GetRow(newView));
            Grid.SetColumn(oldView, Grid.GetColumn(newView));
            Grid.SetRowSpan(oldView, Grid.GetRowSpan(newView));
            Grid.SetColumnSpan(oldView, Grid.GetColumnSpan(newView));

            if (newView is ListView)
            {
                VxListViewExtensions.SetBindSelectedItem(oldView, VxListViewExtensions.GetBindSelectedItem(newView));
            }
        }

        private static void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = sender as ListView;

            VxState itemSelectedState = VxListViewExtensions.GetBindSelectedItem(listView);

            if (itemSelectedState != null)
            {
                itemSelectedState.Value = e.SelectedItem;
            }
        }

        private static void ReconcileList(IList<View> oldList, IList<View> newList)
        {
            if (oldList.Count == 0)
            {
                foreach (var val in newList)
                {
                    oldList.Add(val);
                }

                return;
            }

            if (newList.Count == 0)
            {
                oldList.Clear();
                return;
            }

            // Need to copy list since if I add any views from this list, it auto-removes the view from the other view
            newList = new List<View>(newList);

            int i = 0;

            for (; i < oldList.Count; i++)
            {
                var oldItem = oldList[i];
                var newItem = newList.ElementAtOrDefault(i);

                if (newItem == null)
                {
                    oldList.RemoveAt(i);
                }
                else if (oldItem.GetType() == newItem.GetType())
                {
                    ReconcileViewOfSameType(oldItem, newItem);
                }
                else if (oldList.Count < newList.Count)
                {
                    oldList.Insert(i, newItem);
                }
                else if (oldList.Count > newList.Count)
                {
                    oldList.RemoveAt(i);
                    i--;
                }
                else
                {
                    oldList[i] = newItem;
                }
            }

            if (oldList.Count < newList.Count)
            {
                for (; i < newList.Count; i++)
                {
                    oldList.Add(newList[i]);
                }
            }
        }

        protected virtual void ShowPopup(VxPage page)
        {
            // VxComponentWithPopups will override this
            GetParentComponent().ShowPopup(page);
        }

        protected void RemoveThisPage()
        {
            var page = FindAncestorOrSelf<VxPage>();
            RemovePage(page);
        }

        protected virtual void RemovePage(VxPage page)
        {
            // VxComponentWithPopups will override this
            GetParentComponent().RemovePage(page);
        }

        protected T FindAncestorOrSelf<T>() where T : VxComponent
        {
            if (this is T expected)
            {
                return expected;
            }

            return FindAncestor<T>();
        }

        protected T FindAncestor<T>() where T : VxComponent
        {
            VxComponent comp = GetParentComponent();
            while (comp != null)
            {
                if (comp is T compT)
                {
                    return compT;
                }

                comp = comp.GetParentComponent();
            }

            throw new InvalidOperationException("Couldn't find a component ancestor of the specified type.");
        }

        protected VxComponent GetParentComponent()
        {
            Element el = Parent;
            while (el != null)
            {
                if (el is VxComponent comp)
                {
                    return comp;
                }

                el = el.Parent;
            }

            throw new InvalidOperationException("Component didn't have a parent component, you might have called a method in an incorrect manner.");
        }

        private VxComponent GetRootComponent()
        {
            if (IsRootComponent)
            {
                return this;
            }

            Element el = Parent;
            while (el != null)
            {
                if (el is VxComponent comp && comp.IsRootComponent)
                {
                    return comp;
                }

                el = el.Parent;
            }

            throw new InvalidOperationException("Component didn't have a parent, you might have called ShowPopup in an incorrect manner.");
        }
    }
}
