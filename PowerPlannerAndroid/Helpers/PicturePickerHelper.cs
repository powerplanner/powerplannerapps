using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PowerPlannerAndroid.Helpers
{
    public static class PicturePickerHelper
    {
        /// <summary>
        /// Opens up a picture picker to let the user select a picture, and returns the stream (or null if failed/canceled).
        /// </summary>
        /// <returns></returns>
        public static Task<MainActivity.PickImageResult> GetImageStreamAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            // Start the picture-picker activity (resumes in MainActivity.cs)
            var mainActivity = MainActivity.GetCurrent();
            if (mainActivity != null)
            {
                // Save the TaskCompletionSource object as a MainActivity property
                var taskCompletionSource = new TaskCompletionSource<MainActivity.PickImageResult>();
                mainActivity.PickImageTaskCompletionSource = taskCompletionSource;

                mainActivity.StartActivityForResult(
                    Intent.CreateChooser(intent, "Select Picture"),
                    MainActivity.PickImageId);

                // Return Task object
                return taskCompletionSource.Task;
            }

            return Task.FromResult<MainActivity.PickImageResult>(null);
        }
    }
}