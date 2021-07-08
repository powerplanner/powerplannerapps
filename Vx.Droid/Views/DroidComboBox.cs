using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.Views
{
    public class DroidComboBox : DroidView<Vx.Views.ComboBox, TextInputLayout>
    {
        private AutoCompleteTextView _autoCompleteTextView;

        public DroidComboBox() : base(new TextInputLayout(new ContextThemeWrapper(VxDroidExtensions.ApplicationContext, Resource.Style.Widget_MaterialComponents_TextInputLayout_OutlinedBox_ExposedDropdownMenu)))
        {
            _autoCompleteTextView = new AutoCompleteTextView(VxDroidExtensions.ApplicationContext)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
        }

        protected override void ApplyProperties(ComboBox oldView, ComboBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Hint = newView.Header;
        }
    }
}