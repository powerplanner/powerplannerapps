using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using Android.Graphics.Drawables;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesDroid.Themes;
using Android.Graphics;
using PowerPlannerAppDataLibrary.App;

namespace PowerPlannerAndroid.Views
{
    public class MainCalendarItemView : LinearLayout
    {
        private View _viewIsComplete;
        private TextView _textViewTitle;

        public Action AfterOpenedHomeworkAction { get; set; }

        public const int TOTAL_HEIGHT_IN_DP = 31; // 29 + 2dp margin at bottom

        public MainCalendarItemView(Context context) : base(context)
        {
            this.Orientation = Orientation.Horizontal;
            this.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, ThemeHelper.AsPx(context, 29))
            {
                BottomMargin = ThemeHelper.AsPx(context, 2)
            };

            _viewIsComplete = new View(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ThemeHelper.AsPx(context, 10), LayoutParams.MatchParent)
            };
            _viewIsComplete.SetBackgroundColor(Color.Black);
            _viewIsComplete.Alpha = 0.2f;
            _viewIsComplete.Visibility = ViewStates.Gone;
            base.AddView(_viewIsComplete);

            _textViewTitle = new TextView(context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(0, LayoutParams.MatchParent)
                {
                    Weight = 1,
                    LeftMargin = ThemeHelper.AsPx(context, 6),
                    BottomMargin = ThemeHelper.AsPx(context, 2)
                },
                Gravity = GravityFlags.CenterVertical
            };
            _textViewTitle.SetTextColor(Color.White);
            _textViewTitle.SetMaxLines(1);
            base.AddView(_textViewTitle);

            base.Click += MainCalendarItemView_Click;
        }

        private void MainCalendarItemView_Click(object sender, EventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item);

            try
            {
                AfterOpenedHomeworkAction?.Invoke();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private BaseViewItemHomeworkExam _item;
        public BaseViewItemHomeworkExam Item
        {
            get { return _item; }
            set
            {
                _item = value;
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (Item == null)
            {
                return;
            }

            try
            {
                base.Background = new ColorDrawable(ColorTools.GetColor(Item.GetClassOrNull().Color));
                _textViewTitle.Text = Item.Name;

                if (Item.IsComplete())
                {
                    _viewIsComplete.Visibility = ViewStates.Visible;
                    _textViewTitle.Alpha = 0.7f;
                }
                else
                {
                    _viewIsComplete.Visibility = ViewStates.Gone;
                    _textViewTitle.Alpha = 1f;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}