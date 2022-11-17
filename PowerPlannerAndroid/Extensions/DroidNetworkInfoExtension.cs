﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAndroid.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.NetworkInfo;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidNetworkInfoExtension : NetworkInfoExtension
    {
        public override NetworkCostType GetNetworkCostType()
        {
            try
            {
                ConnectivityManager connMgr = (ConnectivityManager)NativeApplication.Current.ApplicationContext.GetSystemService(Context.ConnectivityService);

                if (connMgr != null)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        Network network = connMgr.ActiveNetwork;
                        if (network != null)
                        {
                            NetworkCapabilities capabilities = connMgr.GetNetworkCapabilities(network);

                            if (capabilities.HasTransport(TransportType.Wifi))
                            {
                                return NetworkCostType.Unlimited;
                            }
                            else
                            {
                                return NetworkCostType.Limited;
                            }
                        }
                    }

                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        NetworkInfo activeInfo = connMgr.ActiveNetworkInfo;
                        if (activeInfo != null && activeInfo.IsConnected)
                        {
                            if (activeInfo.Type == ConnectivityType.Wifi)
                            {
                                return NetworkCostType.Unlimited;
                            }
                            else
                            {
                                return NetworkCostType.Limited;
                            }
                        }
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return NetworkCostType.Disconnected;
        }
    }
}