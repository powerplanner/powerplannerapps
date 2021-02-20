using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.Binding;
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

        public VxBinding(BindingHost bindingHost, string sourcePropertyPath)
        {
            BindingHost = bindingHost;
            SourcePropertyPath = sourcePropertyPath;
        }
    }

    public static class VxViewExtensions
    {
        public static T VxVisibility<T>(this T view, VxBinding binding) where T : View
        {
            binding.BindingHost.SetBinding<bool>(binding.SourcePropertyPath, b => view.Visibility = b ? ViewStates.Visible : ViewStates.Gone);
            return view;
        }

        public static T VxVisibility<T>(this T view, ViewStates viewState) where T : View
        {
            view.Visibility = viewState;
            return view;
        }

        public static T VxEnabled<T>(this T view, VxBinding binding) where T : View
        {
            binding.BindingHost.SetBinding<bool>(binding.SourcePropertyPath, b => view.Enabled = b);
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

            public LayoutParamsBuilder<T> Height(int dp)
            {
                _height = ThemeHelper.AsPx(_view.Context, dp);
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

        public static T VxElevation<T>(this T view, int elevation) where T : View
        {
            view.Elevation = ThemeHelper.AsPx(view.Context, elevation);
            return view;
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
    }

    public static class VxLinearLayoutExtensions
    {
        public static T VxOrientation<T>(this T linearLayout, Orientation orientation) where T : LinearLayout
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
    }

    public static class VsTextViewExtensions
    {
        public static T VxText<T>(this T textView, VxBinding binding) where T : TextView
        {
            binding.BindingHost.SetBinding<string>(binding.SourcePropertyPath, s => textView.Text = s);
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
    }
}