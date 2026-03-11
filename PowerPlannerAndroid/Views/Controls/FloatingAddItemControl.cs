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
using InterfacesDroid.Views;
using Android.Util;
using InterfacesDroid.Themes;
using BareMvvm.Core.App;
using ToolsPortable;
using System.ComponentModel;
using Google.Android.Material.FloatingActionButton;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views.Controls
{
    public class FloatingAddItemControl : InflatedView
    {
        public event EventHandler OnRequestAddTask;
        public event EventHandler OnRequestAddEvent;
        public event EventHandler OnRequestAddHoliday;

        private FloatingActionButton _actionButtonAdd;
        private FloatingActionButton _actionButtonAddEvent;
        private FloatingActionButton _actionButtonAddHoliday;

        private View _sectionEvent;
        private View _sectionTask;
        private View _sectionHoliday;
        private TextView _textViewAddTask;
        private TextView _textViewAddEvent;
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

            _sectionEvent = FindViewById<View>(Resource.Id.FloadingAddItemAddEventSection);
            _sectionEvent.Click += _actionButtonAddEvent_Click;
            _sectionTask = FindViewById<View>(Resource.Id.FloadingAddItemAddTaskSection);
            _sectionTask.Click += _sectionTask_Click;
            _sectionHoliday = FindViewById<View>(Resource.Id.FloadingAddItemAddHolidaySection);
            _sectionHoliday.Click += _actionButtonAddHoliday_Click;
            _textViewAddTask = FindViewById<TextView>(Resource.Id.FloatingAddItemAddTaskText);
            _textViewAddTask.Text = R.S("String_AddTask");
            _textViewAddEvent = FindViewById<TextView>(Resource.Id.FloatingAddItemAddEventText);
            _textViewAddEvent.Text = R.S("String_AddEvent");
            _textViewAddHoliday = FindViewById<TextView>(Resource.Id.FloatingAddItemAddHolidayText);
            _textViewAddHoliday.Text = R.S("String_AddHoliday");

            _actionButtonAddEvent = FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAddEvent);
            _actionButtonAddEvent.Click += _actionButtonAddEvent_Click;

            _actionButtonAddHoliday = FindViewById<FloatingActionButton>(Resource.Id.FloatingActionButtonAddHoliday);
            _actionButtonAddHoliday.Click += _actionButtonAddHoliday_Click;

            _expandedBackground = FindViewById<View>(Resource.Id.FloatingAddItemExpandedBackground);
            _expandedBackground.Click += _expandedBackground_Click;

            CollapseExpanded();
        }

        private void _sectionTask_Click(object sender, EventArgs e)
        {
            OnRequestAddTask?.Invoke(this, new EventArgs());
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

        private void _actionButtonAddEvent_Click(object sender, EventArgs e)
        {
            OnRequestAddEvent?.Invoke(this, new EventArgs());
            CollapseExpanded();
        }

        private void _actionButtonAdd_Click(object sender, EventArgs e)
        {
            if (_isExpanded)
            {
                OnRequestAddTask?.Invoke(this, new EventArgs());
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
            _sectionEvent.Visibility = ViewStates.Visible;
            _sectionEvent.Animate().TranslationY(0).Start();
            ShowItem(_actionButtonAddEvent);
            ShowItem(_textViewAddEvent);

            if (SupportsAddHoliday)
            {
                _sectionHoliday.Visibility = ViewStates.Visible;
                _sectionHoliday.Animate().TranslationY(0).Start();
                ShowItem(_actionButtonAddHoliday);
                ShowItem(_textViewAddHoliday);
            }

            _textViewAddTask.Visibility = ViewStates.Visible;
            _textViewAddTask.Animate().Alpha(1).Start();
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
            HideAddEventItems();
            HideAddHolidayItems();
            HideAddTaskText();
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

        private void HideAddEventItems()
        {
            _sectionEvent.Visibility = ViewStates.Gone;
            _sectionEvent.TranslationY = ThemeHelper.AsPx(Context, 40);
            HideItem(_textViewAddEvent);
            HideItem(_actionButtonAddEvent);
        }

        private void HideAddHolidayItems()
        {
            _sectionHoliday.Visibility = ViewStates.Gone;
            _sectionHoliday.TranslationY = ThemeHelper.AsPx(Context, 40);
            HideItem(_textViewAddHoliday);
            HideItem(_actionButtonAddHoliday);
        }

        private void HideAddTaskText()
        {
            _textViewAddTask.Alpha = 0;
            _textViewAddTask.Visibility = ViewStates.Gone;
        }
    }
}