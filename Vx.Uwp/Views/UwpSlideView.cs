using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Vx.Uwp.Views
{
    public class UwpSlideView : UwpView<Vx.Views.SlideView, UwpSlideView.MySlideView>
    {
        private static long _slideViewNum;
        public UwpSlideView()
        {
            View.Name = "SlideView" + _slideViewNum;
            _slideViewNum++;
            View.DeferUpdates = true;
            View.SelectionChanged += View_SelectionChanged;
            View.VerticalContentAlignment = VerticalAlignment.Stretch;
            View.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        private bool _ignoreSelectionChanged;
        private void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (View.SelectedItem == null || _ignoreSelectionChanged)
            {
                return;
            }

            int newPosition = (int)View.SelectedItem;
            if (VxView.Position != null && VxView.Position.Value != newPosition)
            {
                VxView.Position.ValueChanged?.Invoke(newPosition);
            }
        }

        public class MySlideView : FlipView
        {
            private int? _minPosition;
            public int? MinPosition
            {
                get => _minPosition;
                set
                {
                    if (value != _minPosition)
                    {
                        _minPosition = value;
                        UpdateItemsSource();
                    }
                }
            }

            private int? _maxPosition;
            public int? MaxPosition
            {
                get => _maxPosition;
                set
                {
                    if (value != _maxPosition)
                    {
                        _maxPosition = value;
                        UpdateItemsSource();
                    }
                }
            }

            private int _position;
            public int Position
            {
                get => _position;
                set
                {
                    if (value != _position)
                    {
                        _position = value;
                        UpdateSelectedIndex();
                    }
                }
            }

            private bool _hasDeferredUpdateSelectedIndex;
            private void UpdateSelectedIndex()
            {
                if (DeferUpdates)
                {
                    _hasDeferredUpdateSelectedIndex = true;
                    return;
                }

                if (ItemsSource == null)
                {
                    UpdateItemsSource();
                }
                else
                {
                    SelectedIndex = _position + MyCollectionView.MIDDLE_POSITION;
                }
            }

            public bool DeferUpdates { get; set; }

            public void ApplyDeferredUpdates()
            {
                var orig = DeferUpdates;
                DeferUpdates = false;

                if (_hasDeferredUpdateItemsSource)
                {
                    UpdateItemsSource();
                    _hasDeferredUpdateItemsSource = false;
                }

                if (_hasDeferredUpdateSelectedIndex)
                {
                    UpdateSelectedIndex();
                    _hasDeferredUpdateSelectedIndex = false;
                }

                DeferUpdates = orig;
            }

            private class MyCollectionView : ICollectionView
            {
                private int _startingItem;
                public MyCollectionView(int startingItem, int? minItem, int? maxItem)
                {
                    // StartingItem is an int where 0 is the middle, and -1 is prev item, 1 is next item, etc
                    _startingItem = startingItem;
                    _currentPosition = 0;
                    CorrectStartingSelectedIndex = startingItem + MIDDLE_POSITION;
                }

                public int CorrectStartingSelectedIndex { get; private set; }

                public bool MoveCurrentTo(object item)
                {
                    CurrentZeroRelativePosition = (int)item;
                    return true;
                }

                public bool MoveCurrentToPosition(int index)
                {
                    CurrentPosition = index;
                    return true;
                }

                public bool MoveCurrentToFirst()
                {
                    CurrentPosition = 0;
                    return true;
                }

                public bool MoveCurrentToLast()
                {
                    CurrentPosition = int.MaxValue;
                    return true;
                }

                public bool MoveCurrentToNext()
                {
                    CurrentPosition++;
                    return true;
                }

                public bool MoveCurrentToPrevious()
                {
                    CurrentPosition--;
                    return true;
                }

                public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
                {
                    throw new NotImplementedException();
                }

                public IObservableVector<object> CollectionGroups => throw new NotImplementedException();

                public object CurrentItem => _hasRequestedUsingCorrectIndex ? CurrentZeroRelativePosition : _startingItem;

                public const int MIDDLE_POSITION = 5000;
                private int _currentPosition;
                public int CurrentPosition
                {
                    get => _currentPosition;
                    private set
                    {
                        if (_currentPosition != value)
                        {
                            var changing = new CurrentChangingEventArgs();
                            CurrentChanging?.Invoke(this, changing);
                            if (changing.Cancel)
                            {
                                return;
                            }
                            _currentPosition = value;
                            CurrentChanged?.Invoke(this, CurrentItem);
                        }
                    }
                }

                private int CurrentZeroRelativePosition
                {
                    get => CurrentPosition - MIDDLE_POSITION;
                    set => CurrentPosition = value + MIDDLE_POSITION;
                }

                public bool HasMoreItems => false;

                public bool IsCurrentAfterLast => false;

                public bool IsCurrentBeforeFirst => false;

                public event EventHandler<object> CurrentChanged;
                public event CurrentChangingEventHandler CurrentChanging;
                public event VectorChangedEventHandler<object> VectorChanged;

                public int IndexOf(object item)
                {
                    return (int)item + MIDDLE_POSITION;
                }

                public void Insert(int index, object item)
                {
                    throw new NotImplementedException();
                }

                public void RemoveAt(int index)
                {
                    throw new NotImplementedException();
                }

                private bool _hasRequestedUsingCorrectIndex;
                public object this[int index]
                {
                    get
                    {
                        if (index <= 2)
                        {
                            return _startingItem + index;
                        }
                        if (!_hasRequestedUsingCorrectIndex)
                        {
                            if (index <= 2)
                            {
                                return _startingItem + index;
                            }

                            _hasRequestedUsingCorrectIndex = true;
                        }

                        return index - MIDDLE_POSITION;
                    }
                    set => throw new NotImplementedException(); }

                public void Add(object item)
                {
                    throw new NotImplementedException();
                }

                public void Clear()
                {
                    throw new NotImplementedException();
                }

                public bool Contains(object item)
                {
                    return true;
                }

                public void CopyTo(object[] array, int arrayIndex)
                {
                    throw new NotImplementedException();
                }

                public bool Remove(object item)
                {
                    throw new NotImplementedException();
                }

                public int Count => MIDDLE_POSITION * 2;

                public bool IsReadOnly => true;

                public IEnumerator<object> GetEnumerator()
                {
                    throw new NotImplementedException();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    throw new NotImplementedException();
                }
            }

            private bool _hasDeferredUpdateItemsSource;
            private void UpdateItemsSource()
            {
                if (DeferUpdates)
                {
                    _hasDeferredUpdateItemsSource = true;
                    return;
                }

                var mcv = new MyCollectionView(Position, MinPosition, MaxPosition);
                ItemsSource = mcv;
                SelectedIndex = mcv.CorrectStartingSelectedIndex;
            }
        }

        private Func<int, Vx.Views.View> _itemTemplate;
        protected override void ApplyProperties(Vx.Views.SlideView oldView, Vx.Views.SlideView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.MinPosition = newView.MinPosition;
            View.MaxPosition = newView.MaxPosition;
            View.Position = newView.Position.Value;

            if (_itemTemplate != newView.ItemTemplate)
            {
                // Our generic data template expects to take in an object, whereas the SlideView takes in an int (position),
                // so we need to adapt the template for that different type
                _itemTemplate = newView.ItemTemplate;
                Func<object, Vx.Views.View> genericItemTemplate = null;
                if (newView.ItemTemplate != null)
                {
                    genericItemTemplate = (obj) => newView.ItemTemplate((int)obj);
                }
                View.DataContext = genericItemTemplate;
            }

            if (newView.ItemTemplate != null)
            {
                if (View.ItemTemplate == null)
                {
                    View.ItemTemplate = UwpDataTemplateView.GetDataTemplateWithVerticalContentStretch(View.Name);
                }
            }
            else
            {
                if (View.ItemTemplate != null)
                {
                    View.ItemTemplate = null;
                }
            }

            _ignoreSelectionChanged = true;
            View.ApplyDeferredUpdates();
            _ignoreSelectionChanged = false;
        }
    }
}
