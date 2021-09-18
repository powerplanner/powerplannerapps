using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using InterfacesDroid.Adapters;
using InterfacesDroid.Themes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ToolsPortable;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidComboBox : DroidView<Vx.Views.ComboBox, TextInputLayout>
    {
        private MaterialAutoCompleteTextView _autoCompleteTextView;

        public DroidComboBox() : base(new TextInputLayout(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialOutlinedDropdownStyle))
        {
            _autoCompleteTextView = new MaterialAutoCompleteTextView(View.Context)
            {
                LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.Null
            };

            _autoCompleteTextView.TextChanged += _autoCompleteTextView_TextChanged;

            View.AddView(_autoCompleteTextView);
        }

        private void _autoCompleteTextView_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var textVal = e.Text.ToString();
            var matching = VxView.Items.OfType<object>().FirstOrDefault(i => i.ToString() == textVal);

            if (VxView.SelectedItem != null && matching != VxView.SelectedItem)
            {
                VxView.SelectedItem.ValueChanged?.Invoke(matching);
            }
        }

        private IEnumerable _currentItems;
        protected override void ApplyProperties(ComboBox oldView, ComboBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Hint = newView.Header;

            if (!object.ReferenceEquals(_currentItems, newView.Items) || !object.ReferenceEquals(oldView?.ItemTemplate, newView.ItemTemplate))
            {
                _currentItems = newView.Items;

                if (newView.ItemTemplate != null)
                {
                    var adapter = new VxDroidAdapter(newView.Items)
                    {
                        ItemTemplate = newView.ItemTemplate
                    };
                    _autoCompleteTextView.Adapter = adapter;
                }
                else
                {
                    var adapter = new ArrayAdapter(View.Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, newView.Items.OfType<object>().Select(i => i.ToString()).ToArray());
                    _autoCompleteTextView.Adapter = adapter;
                }
            }

            if (newView.SelectedItem?.Value != null)
            {
                if (_autoCompleteTextView.Text != newView.SelectedItem.Value.ToString())
                {
                    _autoCompleteTextView.SetText(newView.SelectedItem.Value.ToString(), false);
                }
            }
        }

        private class VxDroidAdapter : BaseAdapter<object>, IFilterable
        {
            private IEnumerable _list;
            private ICollection _collection;

            private Func<object, Vx.Views.View> _itemTemplate;
            public Func<object, Vx.Views.View> ItemTemplate
            {
                get => _itemTemplate;
                set
                {
                    if (_itemTemplate == value)
                    {
                        return;
                    }

                    _itemTemplate = value;
                    NotifyDataSetChanged();
                }
            }

            public VxDroidAdapter(IEnumerable list)
            {
                _list = list;
                _collection = list as ICollection;
                if (list is INotifyCollectionChanged collectionChanged)
                {
                    collectionChanged.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(CollectionChanged_CollectionChanged).Handler;
                }
            }

            private void CollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                base.NotifyDataSetChanged();
            }

            public override int Count => _collection?.Count ?? _list.OfType<object>().Count();

            private Filter _filter = new ArrayFilter();
            public Filter Filter => _filter;

            private class ArrayFilter : Filter
            {
                protected override FilterResults PerformFiltering(Java.Lang.ICharSequence constraint)
                {
                    return new FilterResults();
                }

                protected override void PublishResults(Java.Lang.ICharSequence constraint, FilterResults results)
                {

                }
            }

            public override object this[int position]
            {
                get
                {
                    if (_list is IList list)
                    {
                        return list[position];
                    }

                    return _list.OfType<object>().ElementAt(position);
                }
            }

            public override long GetItemId(int position)
            {
                return this[position].GetHashCode();
            }

            public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
            {
                if (DataTemplateHelper.ProcessAndIsNewComponent(this[position], ItemTemplate, convertView as INativeComponent, out VxComponent newComponent))
                {
                    var newView = newComponent.Render();
                    var padding = ThemeHelper.AsPx(parent.Context, 12);
                    newView.SetPadding(padding, padding, padding, padding);
                    return newView;
                }

                // Otherwise recycled and was already updated
                return convertView;
            }
        }
    }
}