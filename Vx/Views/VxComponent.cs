using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
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

        public VxComponent()
        {
            SubscribeToStates();
        }

        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public T GetProperty<T>(T defaultValue = default(T), [CallerMemberName]string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out object val))
            {
                return (T)val;
            }

            return defaultValue;
        }

        public void SetProperty<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out object existingVal) && object.Equals(existingVal, value))
            {
                return;
            }

            _properties[propertyName] = value;
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
            }

            RenderActual();
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

        private void SubscribeToStates()
        {
            var stateType = typeof(VxState);
            foreach (var prop in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => stateType.IsAssignableFrom(i.PropertyType)))
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
        private static Type _resourceDictionaryType = typeof(ResourceDictionary);

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
            View oldView = Content;

            if (oldView == null || oldView.GetType() != newView.GetType())
            {
                Content = newView;
            }

            else
            {
                // Transfer over the properties
                ReconcileViewOfSameType(oldView, newView);
            }

            LastMillisecondsToRender = (DateTime.Now - now).Milliseconds;
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

                        // Don't assign ListView ItemSelected
                        if (prop.DeclaringType == _listViewType)
                        {
                            switch (prop.Name)
                            {
                                case nameof(ListView.SelectedItem):
                                    continue;
                            }
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

                        var newVal = prop.GetValue(newView);

                        // If a View property
                        if (_viewType.IsAssignableFrom(propType))
                        {
                            // If the view is a VxComponent, we want to apply its properties like margin or background color, but NOT reconcile its content... since that'll be updated by the VxComponent itself
                            if (!(newView is VxComponent))
                            {
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
                        }
                        else
                        {
                            prop.SetValue(oldView, newVal);
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

            if (oldView is Entry oldEntry && newView is Entry newEntry)
            {
                VxInputViewExtensions.SetBindText(oldEntry, VxInputViewExtensions.GetBindText(newEntry));
            }

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
    }
}
