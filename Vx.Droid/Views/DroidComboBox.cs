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

namespace Vx.Droid.Views
{
    public class DroidComboBox : DroidView<Vx.Views.ComboBox, FrameLayout>
    {
        private TextInputLayout _blankTextInputLayout;
        private Spinner _spinner;

        private TextInputLayout _autoCompleteTextInputLayout;
        private MaterialAutoCompleteTextView _autoCompleteTextView;

        public DroidComboBox() : base(new FrameLayout(VxDroidExtensions.ApplicationContext))
        {
        }

        private void _autoCompleteTextView_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (_spinner != null)
            {
                return;
            }

            var textVal = e.Text.ToString();
            var matching = VxView.Items.OfType<object>().FirstOrDefault(i => i.ToString() == textVal);

            if (VxView.SelectedItem != null && matching != VxView.SelectedItem.Value)
            {
                VxView.SelectedItem.ValueChanged?.Invoke(matching);
            }
        }

        private IEnumerable _currentItems;
        protected override void ApplyProperties(Vx.Views.ComboBox oldView, Vx.Views.ComboBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.ItemTemplate != null)
            {
                if (_blankTextInputLayout == null)
                {
                    if (_autoCompleteTextInputLayout != null)
                    {
                        _autoCompleteTextView.TextChanged -= _autoCompleteTextView_TextChanged;
                        _autoCompleteTextView.Adapter = null;
                        _autoCompleteTextView = null;

                        View.RemoveView(_autoCompleteTextInputLayout);
                        _autoCompleteTextInputLayout = null;
                    }

                    _blankTextInputLayout = new TextInputLayout(View.Context, null, Resource.Attribute.materialOutlinedTextBoxStyle);

                    var blankEditText = new TextInputEditText(_blankTextInputLayout.Context)
                    {
                        LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                        Text = " "
                    };

                    _blankTextInputLayout.AddView(blankEditText);

                    View.AddView(_blankTextInputLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

                    _spinner = new Spinner(View.Context);
                    _spinner.SetPadding(AsPx(2), AsPx(4), 0, 0);
                    _spinner.ItemSelected += _spinner_ItemSelected;

                    View.AddView(_spinner, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                }

                _blankTextInputLayout.Hint = newView.Header;
            }
            else
            {
                if (_autoCompleteTextView == null)
                {
                    if (_blankTextInputLayout != null)
                    {
                        _spinner.ItemSelected -= _spinner_ItemSelected;
                        _spinner.Adapter = null;
                        _spinner = null;

                        View.RemoveView(_blankTextInputLayout);
                        _blankTextInputLayout = null;
                    }

                    _autoCompleteTextInputLayout = new TextInputLayout(View.Context, null, Resource.Attribute.materialOutlinedDropdownStyle);

                    _autoCompleteTextView = new MaterialAutoCompleteTextView(_autoCompleteTextInputLayout.Context)
                    {
                        LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                        InputType = Android.Text.InputTypes.Null
                    };

                    _autoCompleteTextView.TextChanged += _autoCompleteTextView_TextChanged;

                    _autoCompleteTextInputLayout.AddView(_autoCompleteTextView);

                    View.AddView(_autoCompleteTextInputLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
                }

                _autoCompleteTextInputLayout.Hint = newView.Header;
            }

            if (!object.ReferenceEquals(_currentItems, newView.Items) || !object.ReferenceEquals(oldView?.ItemTemplate, newView.ItemTemplate))
            {
                _currentItems = newView.Items;

                BaseAdapter adapter;

                if (newView.ItemTemplate != null)
                {
                    adapter = new VxDroidAdapter(newView.Items)
                    {
                        ItemTemplate = newView.ItemTemplate
                    };
                }
                else
                {
                    adapter = new ArrayAdapter(View.Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, newView.Items.OfType<object>().Select(i => i.ToString()).ToArray());
                }

                if (_spinner != null)
                {
                    _spinner.Adapter = adapter;
                }
                else
                {
                    _autoCompleteTextView.Adapter = adapter;
                }
            }

            if (newView.SelectedItem?.Value != null)
            {
                if (_spinner != null)
                {
                    _spinner.SetSelection(newView.Items.OfType<object>().ToList().IndexOf(newView.SelectedItem.Value));
                }
                else if (_autoCompleteTextView.Text != newView.SelectedItem.Value.ToString())
                {
                    _autoCompleteTextView.SetText(newView.SelectedItem.Value.ToString(), false);
                }
            }
        }

        private void _spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var matching = VxView.Items.OfType<object>().ElementAtOrDefault(e.Position);

            if (VxView.SelectedItem != null && matching != VxView.SelectedItem.Value)
            {
                VxView.SelectedItem.ValueChanged?.Invoke(matching);
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
                if (Vx.Views.DataTemplateHelper.ProcessAndIsNewComponent(this[position], ItemTemplate, convertView as INativeComponent, out Vx.Views.VxComponent newComponent))
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