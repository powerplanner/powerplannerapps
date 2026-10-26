using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Google.Android.Material.TextField;
using InterfacesDroid.Dialogs;
using InterfacesDroid.Themes;
using System;
using ToolsPortable;

namespace Vx.Droid.Views
{
    public class DroidCustomColorPicker : DroidView<Vx.Views.CustomColorPicker, TextInputLayout>
    {
        private TextInputEditText _editText;
        private GradientDrawable _swatch;
        private CustomColorPickerDialog _dialog;

        public DroidCustomColorPicker() : base(new TextInputLayout(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialOutlinedTextBoxStyle))
        {
            _editText = new TextInputEditText(View.Context)
            {
                LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Focusable = false,
                FocusableInTouchMode = false
            };

            _swatch = new GradientDrawable();
            _swatch.SetShape(ShapeType.Rectangle);
            _swatch.SetStroke(ThemeHelper.AsPx(View.Context, 1), Android.Graphics.Color.Black);
            var swatchSize = ThemeHelper.AsPx(View.Context, 20);
            _swatch.SetSize(swatchSize, swatchSize);
            _editText.SetCompoundDrawablesRelativeWithIntrinsicBounds(_swatch, null, null, null);
            _editText.CompoundDrawablePadding = ThemeHelper.AsPx(View.Context, 12);

            View.AddView(_editText);
            View.Click += View_Click;
            _editText.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            ShowPicker();
        }

        private void ShowPicker()
        {
            if (_dialog == null)
            {
                Activity activity = (BareMvvm.Core.App.PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow as InterfacesDroid.Windows.NativeDroidAppWindow)?.Activity;
                activity = activity ?? View.Context as Activity;
                if (activity == null)
                {
                    return;
                }

                _dialog = new CustomColorPickerDialog(activity);
                _dialog.ColorChosen += Dialog_ColorChosen;
            }

            _dialog.Show((VxView.Color?.Value ?? default(System.Drawing.Color)).ToDroid());
        }

        private void Dialog_ColorChosen(object sender, Android.Graphics.Color e)
        {
            var color = System.Drawing.Color.FromArgb(e.A, e.R, e.G, e.B);

            if (VxView.Color != null && VxView.Color.Value != color)
            {
                UpdateColorDisplay(color);
                VxView.Color.ValueChanged?.Invoke(color);
            }
        }

        private void UpdateColorDisplay(System.Drawing.Color color)
        {
            _swatch.SetColor(color.ToDroid());
            _editText.Text = ColorHelper.ToString(new byte[] { color.R, color.G, color.B }).ToUpper();
        }

        protected override void ApplyProperties(Vx.Views.CustomColorPicker oldView, Vx.Views.CustomColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Hint = newView.Header;
            View.Enabled = newView.IsEnabled;

            if (newView.Color != null)
            {
                UpdateColorDisplay(newView.Color.Value);
            }
        }
    }
}
