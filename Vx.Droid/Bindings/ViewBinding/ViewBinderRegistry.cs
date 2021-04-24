#region File and License Information
/*
<File>
	<License Type="BSD">
		Copyright © 2009 - 2016, Outcoder. All rights reserved.
	
		This file is part of Calcium (http://calciumsdk.net).

		Redistribution and use in source and binary forms, with or without
		modification, are permitted provided that the following conditions are met:
			* Redistributions of source code must retain the above copyright
			  notice, this list of conditions and the following disclaimer.
			* Redistributions in binary form must reproduce the above copyright
			  notice, this list of conditions and the following disclaimer in the
			  documentation and/or other materials provided with the distribution.
			* Neither the name of the <organization> nor the
			  names of its contributors may be used to endorse or promote products
			  derived from this software without specific prior written permission.

		THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
		ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
		DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
		DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
		(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
		LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
		ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
		(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
		SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	</License>
	<Owner Name="Daniel Vaughan" Email="danielvaughan@outcoder.com" />
	<CreationDate>$CreationDate$</CreationDate>
</File>
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InterfacesDroid.Views;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;
using Google.Android.Material.CheckBox;
using Google.Android.Material.RadioButton;
using Android.Views;

#if __ANDROID__ || MONODROID
using Android.Widget;
#endif

namespace BareMvvm.Core.Bindings
{
	public class ViewBinderRegistry
	{
		public bool RemoveViewBinder(Type viewType, string propertyName)
		{
			string key = MakeDictionaryKey(viewType, propertyName);
			return binderDictionary.Remove(key);
		}

		public bool TryGetViewBinder(Type viewType, string propertyName, out IViewBinder viewBinder)
		{
			if (propertyName == "HasFocus")
            {
				viewType = typeof(View);
            }

			string key = MakeDictionaryKey(viewType, propertyName);

			if (binderDictionary.TryGetValue(key, out viewBinder))
			{
				return true;
			}

			return false;
		}

		static string MakeDictionaryKey(Type viewType, string propertyName)
		{
			return viewType.AssemblyQualifiedName + "." + propertyName;
		}

		public void SetViewBinder<TView>(string propertyName, IViewBinder viewBinder)
		{
			string key = typeof(TView).AssemblyQualifiedName + "." + propertyName;
			binderDictionary[key] = viewBinder;
		}

		public void SetViewBinder(Type viewType, string propertyName, IViewBinder viewBinder)
		{
			string key = MakeDictionaryKey(viewType, propertyName);
			binderDictionary[key] = viewBinder;
		}

		readonly Dictionary<string, IViewBinder> binderDictionary
			= new Dictionary<string, IViewBinder>
			{
#if __ANDROID__ || MONODROID
				{MakeDictionaryKey(typeof(CalendarView), nameof(CalendarView.Date)), new ViewEventBinder<CalendarView, CalendarView.DateChangeEventArgs, DateTime>(
					(view, h) => view.DateChange += h, (view, h) => view.DateChange -= h, (view, args) => new DateTime(args.Year, args.Month, args.DayOfMonth))},
				{MakeDictionaryKey(typeof(CheckBox), nameof(CheckBox.Checked)), new ViewEventBinder<CheckBox, CheckBox.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(AppCompatCheckBox), nameof(AppCompatCheckBox.Checked)), new ViewEventBinder<AppCompatCheckBox, AppCompatCheckBox.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(RadioButton), nameof(RadioButton.Checked)), new ViewEventBinder<RadioButton, RadioButton.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(AppCompatRadioButton), nameof(AppCompatRadioButton.Checked)), new ViewEventBinder<AppCompatRadioButton, AppCompatRadioButton.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(MaterialRadioButton), nameof(MaterialRadioButton.Checked)), new ViewEventBinder<MaterialRadioButton, CompoundButton.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(RatingBar), nameof(RatingBar.Rating)), new ViewEventBinder<RatingBar, RatingBar.RatingBarChangeEventArgs, float>(
					(view, h) => view.RatingBarChange += h, (view, h) => view.RatingBarChange -= h, (view, args) => args.Rating)},
				{MakeDictionaryKey(typeof(AppCompatRatingBar), nameof(AppCompatRatingBar.Rating)), new ViewEventBinder<AppCompatRatingBar, AppCompatRatingBar.RatingBarChangeEventArgs, float>(
					(view, h) => view.RatingBarChange += h, (view, h) => view.RatingBarChange -= h, (view, args) => args.Rating)},
				{MakeDictionaryKey(typeof(Android.Widget.SearchView), nameof(Android.Widget.SearchView.Query)), new ViewEventBinder<Android.Widget.SearchView, Android.Widget.SearchView.QueryTextChangeEventArgs, string>(
					(view, h) => view.QueryTextChange += h, (view, h) => view.QueryTextChange -= h, (view, args) => args.NewText)},
				{MakeDictionaryKey(typeof(AndroidX.AppCompat.Widget.SearchView), nameof(AndroidX.AppCompat.Widget.SearchView.Query)), new ViewEventBinder<AndroidX.AppCompat.Widget.SearchView, AndroidX.AppCompat.Widget.SearchView.QueryTextChangeEventArgs, string>(
					(view, h) => view.QueryTextChange += h, (view, h) => view.QueryTextChange -= h, (view, args) => args.NewText)},
				{MakeDictionaryKey(typeof(Switch), nameof(Switch.Checked)), new ViewEventBinder<Switch, Switch.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(SwitchCompat), nameof(SwitchCompat.Checked)), new ViewEventBinder<SwitchCompat, SwitchCompat.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(TimePicker), nameof(TimePicker.Minute)), new ViewEventBinder<TimePicker, TimePicker.TimeChangedEventArgs, int>(
					(view, h) => view.TimeChanged += h, (view, h) => view.TimeChanged -= h, (view, args) => args.Minute)},
				{MakeDictionaryKey(typeof(TimePicker), nameof(TimePicker.Hour)), new ViewEventBinder<TimePicker, TimePicker.TimeChangedEventArgs, int>(
					(view, h) => view.TimeChanged += h, (view, h) => view.TimeChanged -= h, (view, args) => args.HourOfDay)},
				{MakeDictionaryKey(typeof(EditText), nameof(EditText.Text)), new ViewEventBinder<EditText, Android.Text.TextChangedEventArgs, string>(
					(view, h) => view.TextChanged += h, (view, h) => view.TextChanged -= h, (view, args) => args.Text.ToString())},
				{MakeDictionaryKey(typeof(AppCompatEditText), nameof(AppCompatEditText.Text)), new ViewEventBinder<AppCompatEditText, Android.Text.TextChangedEventArgs, string>(
					(view, h) => view.TextChanged += h, (view, h) => view.TextChanged -= h, (view, args) => args.Text.ToString())},
				{MakeDictionaryKey(typeof(ToggleButton), nameof(ToggleButton.Text)), new ViewEventBinder<ToggleButton, CompoundButton.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked)},
				{MakeDictionaryKey(typeof(SeekBar), nameof(SeekBar.Progress)), new ViewEventBinder<SeekBar, SeekBar.ProgressChangedEventArgs, int>(
					(view, h) => view.ProgressChanged += h, (view, h) => view.ProgressChanged -= h, (view, args) => args.Progress)},
				{MakeDictionaryKey(typeof(TextInputEditText), nameof(TextInputEditText.Text)), new ViewEventBinder<TextInputEditText, Android.Text.TextChangedEventArgs, string>(
					(view, h) => view.TextChanged += h, (view, h) => view.TextChanged -= h, (view, args) => args.Text.ToString())},
				{MakeDictionaryKey(typeof(InlineDatePicker), nameof(InlineDatePicker.Date)), new ViewEventBinder<InlineDatePicker, DateTime, DateTime>(
					(view, h) => view.DateChanged += h, (view, h) => view.DateChanged -= h, (view, args) => args) },
				{MakeDictionaryKey(typeof(MaterialCheckBox), nameof(MaterialCheckBox.Checked)), new ViewEventBinder<MaterialCheckBox, CompoundButton.CheckedChangeEventArgs, bool>(
					(view, h) => view.CheckedChange += h, (view, h) => view.CheckedChange -= h, (view, args) => args.IsChecked) },
				{MakeDictionaryKey(typeof(View), nameof(View.HasFocus)), new ViewEventBinder<View, Android.Views.View.FocusChangeEventArgs, bool>(
					(view, h) => view.FocusChange += h, (view, h) => view.FocusChange -= h, (view, args) => args.HasFocus) },
				{MakeDictionaryKey(typeof(BareEditDecimalNumber), nameof(BareEditDecimalNumber.Value)), new ViewEventBinder<BareEditDecimalNumber, double?, double?>(
					(view, h) => view.ValueChanged += h, (view, h) => view.ValueChanged -= h, (view, args) => args) },
				{MakeDictionaryKey(typeof(BareMaterialEditDecimalNumber), nameof(BareMaterialEditDecimalNumber.Value)), new ViewEventBinder<BareMaterialEditDecimalNumber, double?, double?>(
					(view, h) => view.ValueChanged += h, (view, h) => view.ValueChanged -= h, (view, args) => args) }
#endif
			};
	}
}