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
using InterfacesDroid.Views;
using Android.Content.Res;
using InterfacesDroid.Themes;
using AndroidX.Core.View;
using AndroidX.Core.Content;

namespace PowerPlannerAndroid.Views.Controls
{
    public class HomeworkProgressBarControl : InflatedViewWithBinding, SeekBar.IOnSeekBarChangeListener
    {
        /// <summary>
        /// Event for when the progress has been changed and committed by the user (user let go).
        /// </summary>
        public event EventHandler OnProgressChangedByUser;

        private SeekBar _seekBar;
        private ImageButton _imageButtonDone;
        private ImageButton _imageButtonIncomplete;

        public HomeworkProgressBarControl(ViewGroup root) : base(Resource.Layout.HomeworkProgressBarControl, root)
        {
            Initialize();
        }

        public HomeworkProgressBarControl(Context context, IAttributeSet attrs) : base(Resource.Layout.HomeworkProgressBarControl, context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
            _seekBar = FindViewById<SeekBar>(Resource.Id.SeekBarHomeworkProgress);
            _seekBar.SetOnSeekBarChangeListener(this);

            _imageButtonDone = FindViewById<ImageButton>(Resource.Id.ImageButtonHomeworkProgressDone);
            _imageButtonDone.Click += _imageButtonDone_Click;

            _imageButtonIncomplete = FindViewById<ImageButton>(Resource.Id.ImageButtonHomeworkProgressIncomplete);
            _imageButtonIncomplete.Click += _imageButtonIncomplete_Click;

            // Initially call OnProgressChanged so that the background colors get set correctly for non-API-21 devices
            OnProgressChanged(_seekBar, 0, false);
        }

        private void _imageButtonIncomplete_Click(object sender, EventArgs e)
        {
            SetProgress(0);
            OnProgressChangedByUser?.Invoke(this, new EventArgs());
        }

        private void _imageButtonDone_Click(object sender, EventArgs e)
        {
            SetProgress(1);
            OnProgressChangedByUser?.Invoke(this, new EventArgs());
        }

        public double Progress
        {
            get { return _seekBar.Progress / 100.0; }
        }

        public void SetProgress(double percentComplete)
        {
            _seekBar.Progress = (int)(percentComplete * 100);
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            if (progress >= 100)
            {
                ViewCompat.SetBackgroundTintList(_imageButtonDone, ContextCompat.GetColorStateList(Context, Resource.Color.homework_progress_complete_button_background_completed));
                ViewCompat.SetBackgroundTintList(_imageButtonIncomplete, ContextCompat.GetColorStateList(Context, Resource.Color.homework_progress_button_background_unselected));
            }

            else
            {
                ViewCompat.SetBackgroundTintList(_imageButtonDone, ContextCompat.GetColorStateList(Context, Resource.Color.homework_progress_button_background_unselected));
                ViewCompat.SetBackgroundTintList(_imageButtonIncomplete, ContextCompat.GetColorStateList(Context, Resource.Color.homework_progress_incomplete_button_background));
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            // Nothing
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            OnProgressChangedByUser?.Invoke(this, new EventArgs());
        }
    }
}