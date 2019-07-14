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
using Android.App.Job;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.Services
{
    public abstract class MyBaseJobService : JobService
    {
        public const string PERMISSION = "android.permission.BIND_JOB_SERVICE";

        public override bool OnStartJob(JobParameters @params)
        {
            PerformWork(@params);
            // True indicates that my service needs to process the work
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            // This method is called if the system decides to stop my job before I've called JobFinished()
            // Returning true means that I would like the job rescheduled
            // Ideally I should cancel the existing task but right now I don't support canceling
            return true;
        }

        private async void PerformWork(JobParameters @params)
        {
            // Avoid throwing here, since otherwise we'll get a "Power Planner has stopped responding" dialog when the user isn't even using the app
            try
            {
                await PerformWorkAsync(@params);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            base.JobFinished(@params, false);
        }

        /// <summary>
        /// JobFinished will automatically be called
        /// </summary>
        /// <returns></returns>
        protected abstract Task PerformWorkAsync(JobParameters @params);
    }
}