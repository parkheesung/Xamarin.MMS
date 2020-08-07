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
using Android.Telephony;
using Android.Provider;
using Java.Lang;
using OctopusV3.Core;
using System.Net;
using Newtonsoft.Json;

namespace LmsReceiver.AndroidApp
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
    public class SMSReceiver : BroadcastReceiver
    {
        public void TestMsg(Context context, string msg)
        {
            Toast.MakeText(context, msg, ToastLength.Short).Show();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, $"SMS Received!!", ToastLength.Short).Show();
        }
    }

    [BroadcastReceiver]
    [IntentFilter(new[] { "android.provider.Telephony.WAP_PUSH_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority, DataMimeType = "application/vnd.wap.mms-message")]
    public class MMSReceiver : BroadcastReceiver
    {
        private const string ACTION_MMS_RECEIVED = "android.provider.Telephony.WAP_PUSH_RECEIVED";
        private const string MMS_DATA_TYPE = "application/vnd.wap.mms-message";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(ACTION_MMS_RECEIVED) || intent.Action.Equals(MMS_DATA_TYPE))
            {
                ReturnValue result = new ReturnValue();
                Bundle bundle = intent.Extras;
                if (bundle != null)
                {
                    try
                    {
                        byte[] buffer = bundle.GetByteArray("data");
                        string incomingNumber = Encoding.UTF8.GetString(buffer);
                        string url = $"//giftipangpang.com/Api/External/Log?key=01032550295&content={incomingNumber}";
                        using (var wc = new WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            string tmp = wc.DownloadString("http:" + url);
                            if (!string.IsNullOrWhiteSpace(tmp))
                            {
                                result = JsonConvert.DeserializeObject<ReturnValue>(tmp);
                                if (result.Check)
                                {
                                    result.Message = "ok";
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        result.Error(ex);
                    }
                }
                else
                {
                    result.Error("Bundle is null");
                }

                Toast.MakeText(context, result.Message, ToastLength.Short).Show();
            }
        }
    }
}