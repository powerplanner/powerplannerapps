﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Helpers
{
    public static class AutofillHelper
    {
        public static void DisableForAll(View view)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                view.ImportantForAutofill = ImportantForAutofill.NoExcludeDescendants;
            }
        }

        public static void EnableForAll(View view)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                view.ImportantForAutofill = ImportantForAutofill.Yes;
            }
        }
    }
}