using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Vx.Views
{
    public abstract class VxComponentForms : ContentView
    {
        public VxComponentForms()
        {
            SubscribeToStates();
        }

        protected override void OnParentSet()
        {
            lock (this)
            {
                if (!_dirty)
                {
                    return;
                }
            }

            RenderActual();
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

        private static void ReconcileViewOfSameType(View oldView, View newView)
        {
            // Transfer over the properties (we only look at properties at VisualElement (contains backgroundcolor, etc) and above, since if we transfer properties from lower like Element, things go bad)
            foreach (var prop in newView.GetType().GetProperties().Where(i => i.CanRead && _visualElementType.IsAssignableFrom(i.DeclaringType)))
            {
#if DEBUG
                try
                {
#endif
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

                        var newVal = prop.GetValue(newView);

                        if (_viewType.IsAssignableFrom(propType))
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
