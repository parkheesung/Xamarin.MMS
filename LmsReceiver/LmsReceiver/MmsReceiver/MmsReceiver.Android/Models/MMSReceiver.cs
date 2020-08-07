using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using OctopusV3.Core;
using Xamarin.Forms;

namespace MmsReceiver.Droid
{
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
                        //android.telephony.extra.SUBSCRIPTION_INDEX,header,pduType,data,phone,subscription,transactionId
                        //byte[] buffer = bundle.GetByteArray("data");
                        //string content = (buffer == null) ? "data" : Encoding.Default.GetString(buffer);
                        result = context.ContentResolver.FindLastKey();
                        if (result.Check)
                        {
                            var coupon = context.ContentResolver.GetMMS(result.Code);

                            if (coupon != null)
                            {
                                string content = JsonConvert.SerializeObject(coupon);
                                string key = context.GetMyPhoneNumber();
                                string url = "http:" + $"//giftipangpang.com/Api/External/Log";
                                var paramData = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("key", key),
                                    new KeyValuePair<string, string>("content", content)
                                });

                                string tmp = DependencyService.Get<IApiClient>().PostRequest(url, paramData);
                                if (!string.IsNullOrWhiteSpace(tmp))
                                {
                                    result = JsonConvert.DeserializeObject<ReturnValue>(tmp);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Error(ex);
                    }
                }
                else
                {
                    result.Error("Bundle is null");
                }

                if (!result.Check)
                {
                    Toast.MakeText(context, result.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}