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

        private TextView _textViewTitle;
        private TextView _textViewSubtitle;

        public SettingsListItem(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Android.Content.Res.TypedArray a = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.SettingsListItem, 0, 0);

            try
            {
                base.SetPaddingRelative(ThemeHelper.AsPx(context, 16), ThemeHelper.AsPx(context, 8), ThemeHelper.AsPx(context, 16), ThemeHelper.AsPx(context, 8));

                ImageView icon = new ImageView(context)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(
                        ThemeHelper.AsPx(context, 48),
                        ThemeHelper.AsPx(context, 48))
                    {
                        Gravity = GravityFlags.CenterVertical
                    }
                };

                Drawable iconDrawable = a.GetDrawable(Resource.Styleable.SettingsListItem_settingIcon);
                if (iconDrawable != null)
                {
                    icon.SetImageDrawable(iconDrawable);
                    if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        icon.ImageTintList = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] {
                            ColorTools.IsInNightMode(this.Context) ? new Color(84, 107, 199) : new Color(46, 54, 109) });
                    }
                }

                base.AddView(icon);

                LinearLayout texts = new LinearLayout(context)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.WrapContent,
                        LinearLayout.LayoutParams.WrapContent)
                    {
                        Gravity = GravityFlags.CenterVertical
                    }
                };
                texts.SetPaddingRelative(ThemeHelper.AsPx(context, 8), 0, 0, 0);

                string title = a.GetString(Resource.Styleable.SettingsListItem_settingTitle);
                _textViewTitle = new TextView(context)
                {
                    Text = title ?? "title",
                    Ellipsize = Android.Text.TextUtils.TruncateAt.End
                };
                _textViewTitle.SetTypeface(_textViewTitle.Typeface, Android.Graphics.TypefaceStyle.Bold);
                _textViewTitle.SetSingleLine(true);
                _textViewTitle.SetTextColor(ColorTools.GetColor(this.Context, Resource.Color.foregroundFull));
                texts.AddView(_textViewTitle);

                string subtitle = a.GetString(Resource.Styleable.SettingsListItem_settingSubtitle);
                _textViewSubtitle = new TextView(context)
                {
                    Text = subtitle ?? "",
                    Ellipsize = Android.Text.TextUtils.TruncateAt.End
                };
                _textViewSubtitle.SetSingleLine(true);
                texts.AddView(_textViewSubtitle);

                base.AddView(texts);
            }

#if DEBUG
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                System.Diagnostics.Debug.WriteLine(ex);
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
    }
}