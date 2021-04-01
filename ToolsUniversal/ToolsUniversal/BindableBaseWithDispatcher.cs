using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ToolsUniversal
{
    [DataContract]
    public class BindableBaseWithDispatcher : INotifyPropertyChanged
    {
        private class EventHandlersForDispatcher : IEnumerable<PropertyChangedEventHandler>
        {
            public CoreDispatcher Dispatcher { get; private set; }

            private List<PropertyChangedEventHandler> _eventHandlers = new List<PropertyChangedEventHandler>();
            
            public EventHandlersForDispatcher(CoreDispatcher dispatcher)
            {
                Dispatcher = dispatcher;
            }

            public void Add(PropertyChangedEventHandler eventHandler)
            {
                if (_eventHandlers.Contains(eventHandler))
                    return;

                _eventHandlers.Add(eventHandler);
            }

            public bool Remove(PropertyChangedEventHandler eventHandler)
            {
                return _eventHandlers.Remove(eventHandler);
            }

            public IEnumerator<PropertyChangedEventHandler> GetEnumerator()
            {
                return _eventHandlers.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class EventHandlersCollection : IEnumerable<EventHandlersForDispatcher>
        {
            private List<EventHandlersForDispatcher> _list = new List<EventHandlersForDispatcher>();

            public void Add(CoreDispatcher dispatcher, PropertyChangedEventHandler eventHandler)
            {
                EventHandlersForDispatcher handlersForDispatcher = _list.FirstOrDefault(i => i.Dispatcher == dispatcher);

                if (handlersForDispatcher == null)
                {
                    handlersForDispatcher = new EventHandlersForDispatcher(dispatcher);
                    _list.Add(handlersForDispatcher);
                }

                // If the event handler isn't in the list yet, add it
                if (!handlersForDispatcher.Contains(eventHandler))
                    handlersForDispatcher.Add(eventHandler);
            }

            public void Remove(PropertyChangedEventHandler eventHandler)
            {
                List<EventHandlersForDispatcher> toRemove = new List<EventHandlersForDispatcher>();

                foreach (var dispatcherHandlers in _list)
                {
                    // If we removed an event handler, and it's the last one in that list, we need to remove that key too
                    if (dispatcherHandlers.Remove(eventHandler) && !dispatcherHandlers.Any())
                        toRemove.Add(dispatcherHandlers);
                }

                // Remove the keys that we flagged
                foreach (var d in toRemove)
                    _list.Remove(d);
            }

            public IEnumerator<EventHandlersForDispatcher> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private EventHandlersCollection _eventHandlersCollection;
        
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    if (_eventHandlersCollection == null)
                        _eventHandlersCollection = new EventHandlersCollection();

                    CoreDispatcher d = Window.Current.Dispatcher;

                    _eventHandlersCollection.Add(d, value);
                }
            }

            remove
            {
                lock (this)
                {
                    if (_eventHandlersCollection == null)
                        return;

                    _eventHandlersCollection.Remove(value);
                }
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            object sender = this;

            try
            {
                lock (this)
                {
                    if (_eventHandlersCollection != null)
                    {
                        foreach (var pair in _eventHandlersCollection)
                        {
                            // Create a new object since the handlers might change by the time dispatched to UI
                            PropertyChangedEventHandler[] eventHandlers = pair.ToArray();

                            // Don't need to wait for these, UI's can update in parallel
                            var task = pair.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
                            {
                                foreach (var eventHandler in eventHandlers)
                                    foreach (string propertyName in propertyNames)
                                        eventHandler(sender, new PropertyChangedEventArgs(propertyName));
                            });
                        }
                    }
                }
            }

            catch { }
        }


        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperties<T>(ref T storage, T value, params string[] propertyNames)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyNames);
            return true;
        }
    }
}
