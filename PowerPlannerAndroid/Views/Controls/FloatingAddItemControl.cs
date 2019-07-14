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
using Android.Support.Design.Widget;
using InterfacesDroid.Views;
using Android.Util;
using InterfacesDroid.Themes;
using BareMvvm.Core.App;
using ToolsPortable;
using System.ComponentModel;

namespace PowerPlannerAndroid.Views.Controls
{
    public class FloatingAddItemControl : InflatedView
    {
        public event EventHandler OnRequestAddHomework;
        public event EventHandler OnRequestAddExam;
        public event EventHandler OnRequestAddHoliday;

        private FloatingActionButton _actionButtonAdd;
        private FloatingActionButton _actionButtonAddExam;
        private FloatingActionButton _actionButtonAddHoliday;

        private View _sectionExam;
        private View _sectionHomework;
        private View _sectionHoliday;
        private TextView _textViewAddHomework;
        private TextView _textViewAddExam;
        private TextView _textViewAddHoliday;
        private View _expandedBackground;

        public bool SupportsAddHoliday { get; set; }

        public FloatingAddItemControl(Context context) : base(context, Resource.Layout.FloatingAddItemControl)
        {
            Initialize();
        }

        public FloatingAddItemControl(Context context, IAttributeSet attrs) : base(context, Resource.Layout.FloatingAddItemControl, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _actionButtonAdd = FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAdd);
            _actionButtonAdd.Click += _actionButtonAdd_Click;

            _sectionExam = FindViewById<View>(Resource.Id.FloadingAddItemAddExamSection);
            _sectionExam.Click += _actionButtonAddExam_Click;
            _sectionHomework = FindViewById<View>(Resource.Id.FloadingAddItemAddHomeworkSection);
            _sectionHomework.Click += _sectionHomework_Click;
            _sectionHoliday = FindViewById<View>(Resource.Id.FloadingAddItemAddHolidaySection);
            _sectionHoliday.Click += _actionButtonAddHoliday_Click;
            _textViewAddHomework = FindViewById<TextView>(Resource.Id.FloatingAddItemAddHomeworkText);
            _textViewAddExam = FindViewById<TextView>(Resource.Id.FloatingAddItemAddExamText);
            _textViewAddHoliday = FindViewById<TextView>(Resource.Id.FloatingAddItemAddHolidayText);

            _actionButtonAddExam = FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAddExam);
            _actionButtonAddExam.Click += _actionButtonAddExam_Click;

            _actionButtonAddHoliday = FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAddHoliday);
            _actionButtonAddHoliday.Click += _actionButtonAddHoliday_Click;

            _expandedBackground = FindViewById<View>(Resource.Id.FloatingAddItemExpandedBackground);
            _expandedBackground.Click += _expandedBackground_Click;

            CollapseExpanded();
        }

        private void _sectionHomework_Click(object sender, EventArgs e)
        {
            OnRequestAddHomework?.Invoke(this, new EventArgs());
            CollapseExpanded();
        }

        private void _expandedBackground_Click(object sender, EventArgs e)
        {
            CollapseExpanded();
        }

        protected override void OnAttachedToWindow()
        {
            PortableApp.Current.GetCurrentWindow().BackPressed += new WeakEventHandler<CancelEventArgs>(FloatingAddItemControl_BackPressed).Handler;

            base.OnAttachedToWindow();
        }

        private void FloatingAddItemControl_BackPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isExpanded)
            {
                CollapseExpanded();
                e.Cancel = true;
            }
        }

        private void _actionButtonAddHoliday_Click(object sender, EventArgs e)
        {
            OnRequestAddHoliday?.Invoke(this, new EventArgs());
            CollapseExpanded();
        }

        private void _actionButtonAddExam_Click(object sender, EventArgs e)
        {
            OnRequestAddExam?.Invoke(this, new EventArgs());
            CollapseExpanded();
        }

        private void _actionButtonAdd_Click(object sender, EventArgs e)
        {
            if (_isExpanded)
            {
                OnRequestAddHomework?.Invoke(this, new EventArgs());
                CollapseExpanded();
            }

            else
            {
                ShowExpanded();
            }
        }

        private bool _isExpanded = true;

        private void ToggleExpanded()
        {
            if (_isExpanded)
            {
                CollapseExpanded();
            }

            else
            {
                ShowExpanded();
            }
        }

        private void ShowExpanded()
        {
            if (_isExpanded)
            {
                return;
            }

            _isExpanded = true;
            _sectionExam.Visibility = ViewStates.Visible;
            _sectionExam.Animate().TranslationY(0).Start();
            ShowItem(_actionButtonAddExam);
            ShowItem(_textViewAddExam);

            if (SupportsAddHoliday)
            {
                _sectionHoliday.Visibility = ViewStates.Visible;
                _sectionHoliday.Animate().TranslationY(0).Start();
                ShowItem(_actionButtonAddHoliday);
                ShowItem(_textViewAddHoliday);
            }

            _textViewAddHomework.Visibility = ViewStates.Visible;
            _textViewAddHomework.Animate().Alpha(1).Start();
            _expandedBackground.Visibility = ViewStates.Visible;
            _expandedBackground.Animate().Alpha(1).Start();
        }

        private void CollapseExpanded()
        {
            if (!_isExpanded)
            {
                return;
            }

            _isExpanded = false;
            HideAddExamItems();
            HideAddHolidayItems();
            HideAddHomeworkText();
            _expandedBackground.Alpha = 0;
            _expandedBackground.Visibility = ViewStates.Gone;
        }

        private void ShowItem(View view)
        {
            view.Visibility = ViewStates.Visible;
            view.Animate().Alpha(1).ScaleX(1).ScaleY(1).Start();
        }

        private void HideItem(View view)
        {
            view.Alpha = 0;
            view.ScaleX = 0.5f;
            view.ScaleY = 0.5f;
            view.Visibility = ViewStates.Gone;
        }

        private void HideAddExamItems()
        {
            _sectionExam.Visibility = ViewStates.Gone;
            _sectionExam.TranslationY = ThemeHelper.AsPx(Context, 40);
            HideItem(_textViewAddExam);
            HideItem(_actionButtonAddExam);
        }

        private void HideAddHolidayItems()
        {
            _sectionHoliday.Visibility = ViewStates.Gone;
            _sectionHoliday.TranslationY = ThemeHelper.AsPx(Context, 40);
            HideItem(_textViewAddHoliday);
            HideItem(_actionButtonAddHoliday);
        }

        private void HideAddHomeworkText()
        {
            _textViewAddHomework.Alpha = 0;
            _textViewAddHomework.Visibility = ViewStates.Gone;
        }
    }
}