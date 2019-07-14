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
using Firebase.Iid;

namespace PowerPlannerAndroid.Services
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIdService : FirebaseInstanceIdService
    {
        public override void OnTokenRefresh()
        {
            // This code is here to ensure that FCM compiled correctly
            // If the property is missing, clean and re-build
            // https://forums.xamarin.com/discussion/96263/default-firebaseapp-is-not-initialized-in-this-process
            var id = GetString(Resource.String.gcm_defaultSenderId);

            SendRegistrationToServer();
        }

        void SendRegistrationToServer()
        {
            // TODO: In future should have this kick off a job service that syncs all accounts
        }

    }
}