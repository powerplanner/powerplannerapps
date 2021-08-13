using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (!object.ReferenceEquals(_currentItems, newView.Items))
            {
                _currentItems = newView.Items;

                var adapter = new ArrayAdapter(View.Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, newView.Items.OfType<object>().Select(i => i.ToString()).ToArray());
                _autoCompleteTextView.Adapter = adapter;
            }

            if (newView.SelectedItem?.Value != null)
            {
                if (_autoCompleteTextView.Text != newView.SelectedItem.Value.ToString())
                {
                    _autoCompleteTextView.SetText(newView.SelectedItem.Value.ToString(), false);
                }
            }
        }
    }
}