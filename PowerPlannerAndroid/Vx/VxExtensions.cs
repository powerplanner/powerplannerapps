using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Content;
using BareMvvm.Core.Binding;
using InterfacesDroid.Helpers;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Vx
{
    public class VxBinding
    {
        public BindingHost BindingHost { get; private set; }
        public string SourcePropertyPath { get; private set; }
        public Func<object, object> Converter { get; private set; }

        public VxBinding(BindingHost bindingHost, string sourcePropertyPath, Func<object, object> converter = null)
        {
            BindingHost = bindingHost;
            SourcePropertyPath = sourcePropertyPath;
            Converter = converter;
        }

        public BindingRegistration SetBinding<T>(Action<T> onValue)
        {
            return BindingHost.SetBinding(SourcePropertyPath, v =>
            {
                if (Converter != null)
                {
                    onValue((T)Converter(v));
                }
                else
                {
                    onValue((T)v);
                }
            });
        }
    }

    public static class VxViewExtensions
    {
        public static T VxVisibility<T>(this T view, VxBinding binding) where T : View
        {
            binding.SetBinding<bool>(b => view.Visibility = b ? ViewStates.Visible : ViewStates.Gone);
            return view;
        }

        public static T VxVisibility<T>(this T view, ViewStates viewState) where T : View
        {
            view.Visibility = viewState;
            return view;
        }

        public static T VxBackgroundTintList<T>(this T view, VxBinding binding) where T : View
        {
            binding.SetBinding<ColorStateList>(csl => view.BackgroundTintList = csl);
            return view;
        }

        public static T VxEnabled<T>(this T view, VxBinding binding) where T : View
        {
            binding.SetBinding<bool>(b => view.Enabled = b);
            return view;
        }

        public static T VxClick<T>(this T view, EventHandler action) where T : View
        {
            view.Click += action;
            return view;
        }

        public static T VxReference<T>(this T view, ref T reference) where T : View
        {
            reference = view;
            return view;
        }

        /// <summary>
        /// Sets padding as DP (no need to convert)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="view"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static T VxPadding<T>(this T view, int padding) where T : View
        {
            var px = ThemeHelper.AsPx(view.Context, padding);
            view.SetPadding(px, px, px, px);
            return view;
        }

        /// <summary>
        /// Sets padding as DP (no need to convert)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="view"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public static T VxPadding<T>(this T view, int left, int top, int right, int bottom) where T : View
        {
            view.SetPadding(
                ThemeHelper.AsPx(view.Context, left),
                ThemeHelper.AsPx(view.Context, top),
                ThemeHelper.AsPx(view.Context, right),
                ThemeHelper.AsPx(view.Context, bottom));
            return view;
        }

        public static T VxLayoutParams<T>(this T view, ViewGroup.LayoutParams layoutParams) where T : View
        {
            view.LayoutParameters = layoutParams;
            return view;
        }

        public class LayoutParamsBuilder<T> where T : View
        {
            private T _view;
            private int _width = ViewGroup.LayoutParams.MatchParent;
            private int _height = ViewGroup.LayoutParams.WrapContent;
            private float _weight;
            private int _marginLeft, _marginTop, _marginRight, _marginBottom;
            private GravityFlags? _gravity;

            public LayoutParamsBuilder(T view)
            {
                _view = view;
            }

            public LayoutParamsBuilder<T> StretchHeight()
            {
                _height = ViewGroup.LayoutParams.MatchParent;
                return this;
            }

            /// <summary>
            /// This is the default
            /// </summary>
            /// <returns></returns>
            public LayoutParamsBuilder<T> AutoHeight()
            {
                _height = ViewGroup.LayoutParams.WrapContent;
                return this;
            }

            public LayoutParamsBuilder<T> Height(float dp)
            {
                _height = ThemeHelper.AsPx(_view.Context, dp);
                return this;
            }

            public LayoutParamsBuilder<T> HeightAsPx(int px)
            {
                _height = px;
                return this;
            }

            public LayoutParamsBuilder<T> HeightWeight(float weight)
            {
                _height = 0;
                _weight = weight;
                return this;
            }

            /// <summary>
            /// This is the default
            /// </summary>
            /// <returns></returns>
            public LayoutParamsBuilder<T> StretchWidth()
            {
                _width = ViewGroup.LayoutParams.MatchParent;
                return this;
            }

            /// <summary>
            /// This is the default
            /// </summary>
            /// <returns></returns>
            public LayoutParamsBuilder<T> AutoWidth()
            {
                _width = ViewGroup.LayoutParams.WrapContent;
                return this;
            }

            public LayoutParamsBuilder<T> Width(int dp)
            {
                _width = ThemeHelper.AsPx(_view.Context, dp);
                return this;
            }

            public LayoutParamsBuilder<T> WidthWeight(float weight)
            {
                _width = 0;
                _weight = weight;
                return this;
            }

            public LayoutParamsBuilder<T> Gravity(GravityFlags gravity)
            {
                _gravity = gravity;
                return this;
            }

            public LayoutParamsBuilder<T> Margins(int dp)
            {
                return Margins(dp, dp, dp, dp);
            }

            public LayoutParamsBuilder<T> Margins(int left, int top, int right, int bottom)
            {
                _marginLeft = left;
                _marginTop = top;
                _marginRight = right;
                _marginBottom = bottom;
                return this;
            }

            public T Apply()
            {
                var lp = new LinearLayout.LayoutParams(_width, _height);

                Apply(lp);

                if (_gravity != null)
                {
                    lp.Gravity = _gravity.Value;
                }

                if (_weight != 0)
                {
                    lp.Weight = _weight;
                }

                _view.LayoutParameters = lp;

                return _view;
            }

            public T ApplyForFrameLayout()
            {
                var lp = new FrameLayout.LayoutParams(_width, _height);

                Apply(lp);

                if (_gravity != null)
                {
                    lp.Gravity = _gravity.Value;
                }

                _view.LayoutParameters = lp;

                return _view;
            }

            public T ApplyForCoordinatorLayout()
            {
                var lp = new CoordinatorLayout.LayoutParams(_width, _height);

                Apply(lp);

                if (_gravity != null)
                {
                    lp.Gravity = (int)_gravity.Value;
                }

                _view.LayoutParameters = lp;

                return _view;
            }

            private void Apply(ViewGroup.MarginLayoutParams lp)
            {
                if (_marginLeft != 0)
                {
                    lp.LeftMargin = ThemeHelper.AsPx(_view.Context, _marginLeft);
                }

                if (_marginTop != 0)
                {
                    lp.TopMargin = ThemeHelper.AsPx(_view.Context, _marginTop);
                }

                if (_marginRight != 0)
                {
                    lp.RightMargin = ThemeHelper.AsPx(_view.Context, _marginRight);
                }

                if (_marginBottom != 0)
                {
                    lp.BottomMargin = ThemeHelper.AsPx(_view.Context, _marginBottom);
                }
            }
        }

        /// <summary>
        /// By default, width will stretch width and height will wrap content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="view"></param>
        /// <returns></returns>
        public static LayoutParamsBuilder<T> VxLayoutParams<T>(this T view) where T : View
        {
            return new LayoutParamsBuilder<T>(view);
        }

        public static T VxBackgroundColor<T>(this T view, Color color) where T : View
        {
            view.SetBackgroundColor(color);
            return view;
        }

        public static T VxBackgroundResource<T>(this T view, int resourceId) where T : View
        {
            view.SetBackgroundResource(resourceId);
            return view;
        }

        public static T VxElevation<T>(this T view, int elevation) where T : View
        {
            view.Elevation = ThemeHelper.AsPx(view.Context, elevation);
            return view;
        }
    }

    public static class VxImageViewExtensions
    {
        public static T VxImageResource<T>(this T image, int resId) where T : ImageView
        {
            image.SetImageResource(resId);
            return image;
        }

        public static T VxScaleType<T>(this T image, ImageView.ScaleType scaleType) where T : ImageView
        {
            image.SetScaleType(scaleType);
            return image;
        }
    }

    public static class VxViewGroupExtensions
    {
        public static T VxChildren<T>(this T viewGroup, params View[] views) where T : ViewGroup
        {
            foreach (var view in views)
            {
                viewGroup.AddView(view);
            }
            return viewGroup;
        }

        public static T VxClipToPadding<T>(this T view, bool clip) where T : ViewGroup
        {
            view.SetClipToPadding(clip);
            return view;
        }
    }

    public static class VxLinearLayoutExtensions
    {
        public static T VxOrientation<T>(this T linearLayout, Android.Widget.Orientation orientation) where T : LinearLayout
        {
            linearLayout.Orientation = orientation;
            return linearLayout;
        }
    }

    public static class VxSpinnerExtensions
    {
        /// <summary>
        /// Sets the currently selected item
        /// </summary>
        /// <param name="spinner"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T VxSelection<T>(this T spinner, int index) where T : Spinner
        {
            spinner.SetSelection(index);
            return spinner;
        }

        public static T VxItemSelected<T>(this T spinner, EventHandler<AdapterView.ItemSelectedEventArgs> handler) where T : Spinner
        {
            spinner.ItemSelected += handler;
            return spinner;
        }

        public static T VxAdapter<T>(this T spinner, ISpinnerAdapter adapter) where T : Spinner
        {
            spinner.Adapter = adapter;
            return spinner;
        }
    }

    public static class VsTextViewExtensions
    {
        public static T VxText<T>(this T textView, VxBinding binding) where T : TextView
        {
            var reg = binding.SetBinding<string>(s => textView.Text = s);

            if (textView is EditText)
            {
                textView.TextChanged += delegate
                {
                    reg.SetSourceValue(textView.Text);
                };
            }

            return textView;
        }

        public static T VxText<T>(this T textView, string text) where T : TextView
        {
            textView.Text = text;
            return textView;
        }

        public static T VxTextLocalized<T>(this T textView, string textId) where T : TextView
        {
            textView.Text = PowerPlannerResources.GetString(textId);
            return textView;
        }

        public static T VxMaxLines<T>(this T textView, int maxLines) where T : TextView
        {
            textView.SetMaxLines(maxLines);
            return textView;
        }

        public static T VxMinLines<T>(this T textView, int minLines) where T : TextView
        {
            textView.SetMinLines(minLines);
            return textView;
        }

        public static T VxTextStyle<T>(this T textView, Typeface typeface) where T : TextView
        {
            textView.Typeface = typeface;
            return textView;
        }

        /// <summary>
        /// Gravity for the text itself
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textView"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        public static T VxGravity<T>(this T textView, GravityFlags gravity) where T : TextView
        {
            textView.Gravity = gravity;
            return textView;
        }

        public static T VxTextColor<T>(this T textView, Color color) where T : TextView
        {
            textView.SetTextColor(color);
            return textView;
        }

        public static T VxTextColorResource<T>(this T textView, int resId) where T : TextView
        {
            textView.SetTextColor(new Color(ContextCompat.GetColor(textView.Context, resId)));
            return textView;
        }

        public static T VxTextSize<T>(this T textView, float size) where T : TextView
        {
            textView.SetTextSize(Android.Util.ComplexUnitType.Sp, size);
            return textView;
        }

        public static T VxHint<T>(this T textView, string hint) where T : TextView
        {
            textView.Hint = hint;
            return textView;
        }

        public static T VxHintLocalized<T>(this T textView, string textId) where T : TextView
        {
            return textView.VxHint(PowerPlannerResources.GetString(textId));
        }
    }

    public static class VxEditTextExtensions
    {
        public static T VxInputType<T>(this T editText, InputTypes inputTypes) where T : EditText
        {
            editText.InputType = inputTypes;
            return editText;
        }

        public static T VxImeOptions<T>(this T editText, Android.Views.InputMethods.ImeAction imeActions) where T : EditText
        {
            editText.ImeOptions = imeActions;
            return editText;
        }
    }

    public static class VxCompoundButtonExtensions
    {
        /// <summary>
        /// Does two-way binding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="compoundButton"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static T VxChecked<T>(this T compoundButton, VxBinding binding) where T : CompoundButton
        {
            var reg = binding.SetBinding<bool>(value =>
            {
                compoundButton.Checked = value;
            });

            compoundButton.CheckedChange += delegate
            {
                reg.SetSourceValue(compoundButton.Checked);
            };

            return compoundButton;
        }
    }
}