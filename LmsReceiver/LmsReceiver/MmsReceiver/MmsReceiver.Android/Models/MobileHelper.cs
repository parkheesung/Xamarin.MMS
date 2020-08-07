using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using OctopusV3.Core;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MmsReceiver.Droid.MobileHelper))]
namespace MmsReceiver.Droid
{
    public class MobileHelper : IHelper
    {


        public MobileHelper()
        {
        }

        public ReturnValue GetSampleMessage()
        {
            var temp = new ReturnValues<List<long>>();
            var result = new ReturnValue();

            if (MainActivity.MainActivityInstance != null)
            {
                try
                {
                    temp = MainActivity.MainActivityInstance.ContentResolver.FindLastKey();
                    if (result.Check)
                    {
                        Coupon coupon = null;
                        foreach (long num in temp.Data)
                        {
                            coupon = MainActivity.MainActivityInstance.ContentResolver.GetMMS(num);
                            if (coupon != null)
                            {
                                string content = JsonConvert.SerializeObject(coupon);
                                string key = Repository.GetMyPhoneNumber(MainActivity.MainActivityInstance);
                                string url = "http:" + $"//giftipangpang.com/Api/External/Log";
                                var paramData = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("key", key),
                                    new KeyValuePair<string, string>("content", content)
                                });

                                ApiClient client = new ApiClient();
                                string tmp = client.PostRequest(url, paramData);
                                if (!string.IsNullOrWhiteSpace(tmp))
                                {
                                    result = JsonConvert.DeserializeObject<ReturnValue>(tmp);
                                    if (result.Check)
                                    {
                                        result.Message = "ok";
                                        break;
                                    }
                                }
                                else
                                {
                                    result.Error("ReturnValue is Empty!!");
                                }
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
                result.Error("MainActivity is null.");
            }

            return result;
        }
    }
}