using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using Android.Graphics;
using Android.Graphics.Drawables;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views.Controls
{
    public class SettingsListItem : LinearLayout
    {
        private const string NAMESPACE = "settingsListItem";

        private ImageView _icon;
        private TextView _textViewTitle;
        private TextView _textViewSubtitle;

        private void Initialize()
        {
            base.SetPaddingRelative(ThemeHelper.AsPx(Context, 16), ThemeHelper.AsPx(Context, 8), ThemeHelper.AsPx(Context, 16), ThemeHelper.AsPx(Context, 8));

            _icon = new ImageView(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(
                    ThemeHelper.AsPx(Context, 48),
                    ThemeHelper.AsPx(Context, 48))
                {
                    Gravity = GravityFlags.CenterVertical
                }
            };

            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                _icon.ImageTintList = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] {
                            ColorTools.IsInNightMode(this.Context) ? new Color(84, 107, 199) : new Color(46, 54, 109) });
            }

            base.AddView(_icon);

            LinearLayout texts = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WrapContent,
                    LinearLayout.LayoutParams.WrapContent)
                {
                    Gravity = GravityFlags.CenterVertical
                }
            };
            texts.SetPaddingRelative(ThemeHelper.AsPx(Context, 8), 0, 0, 0);

            _textViewTitle = new TextView(Context)
            {
                Ellipsize = Android.Text.TextUtils.TruncateAt.End
            };
            _textViewTitle.SetTypeface(_textViewTitle.Typeface, Android.Graphics.TypefaceStyle.Bold);
            _textViewTitle.SetSingleLine(true);
            _textViewTitle.SetTextColor(ColorTools.GetColor(this.Context, Resource.Color.foregroundFull));
            texts.AddView(_textViewTitle);

            _textViewSubtitle = new TextView(Context)
            {
                Ellipsize = Android.Text.TextUtils.TruncateAt.End
            };
            _textViewSubtitle.SetSingleLine(true);
            texts.AddView(_textViewSubtitle);

            base.AddView(texts);
        }

        public SettingsListItem(Context Context) : base(Context)
        {
            Initialize();
        }

        public SettingsListItem(Context Context, IAttributeSet attrs) : base(Context, attrs)
        {
            Initialize();

            Android.Content.Res.TypedArray a = Context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.SettingsListItem, 0, 0);

            try
            {
                base.SetPaddingRelative(ThemeHelper.AsPx(Context, 16), ThemeHelper.AsPx(Context, 8), ThemeHelper.AsPx(Context, 16), ThemeHelper.AsPx(Context, 8));

                Drawable iconDrawable = a.GetDrawable(Resource.Styleable.SettingsListItem_settingIcon);
                if (iconDrawable != null)
                {
                    _icon.SetImageDrawable(iconDrawable);
                }

                string title = a.GetString(Resource.Styleable.SettingsListItem_settingTitle);
                _textViewTitle.Text = title ?? "title";

                string subtitle = a.GetString(Resource.Styleable.SettingsListItem_settingSubtitle);
                _textViewSubtitle.Text = subtitle ?? "";
            }

#if DEBUG
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            finally { a.Recycle(); }
        }

        public string SettingTitle
        {
            get { return _textViewTitle?.Text; }
            set { if (_textViewTitle != null) _textViewTitle.Text = value; }
        }

        public string SettingSubtitle
        {
            get { return _textViewSubtitle?.Text; }
            set { if (_textViewSubtitle != null) _textViewSubtitle.Text = value; }
        }

        public int IconResource
        {
            get => 0;
            set => _icon.SetImageResource(value);
        }
    }
}