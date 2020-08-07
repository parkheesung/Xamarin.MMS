using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(MmsReceiver.Droid.ApiClient))]
namespace MmsReceiver.Droid
{
    public class ApiClient : IApiClient
    {
        private static HttpClient client { get; set; } = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());

        public string GetRequest(string url)
        {
            StringBuilder builder = new StringBuilder(200);

            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                builder.Append(response.Content.ReadAsStringAsync().Result);
            }
            catch (HttpRequestException e)
            {
                builder.Append("{" + $" Check : false, Message : \"{e.Message}\", Value:\"\" " + "}");
            }
            catch (Exception ex)
            {
                builder.Append("{" + $" Check : false, Message : \"{ex.Message}\", Value:\"\" " + "}");
            }

            return builder.ToString();
        }

        public string PostRequest(string url, FormUrlEncodedContent postValue)
        {
            StringBuilder builder = new StringBuilder(200);

            try
            {
                HttpResponseMessage response = client.PostAsync(url, postValue).Result;
                response.EnsureSuccessStatusCode();
                builder.Append(response.Content.ReadAsStringAsync().Result);
            }
            catch (HttpRequestException e)
            {
                builder.Append("{" + $" Check : false, Message : \"{e.Message}\", Value:\"\" " + "}");
            }
            catch (Exception ex)
            {
                builder.Append("{" + $" Check : false, Message : \"{ex.Message}\", Value:\"\" " + "}");
            }

            return builder.ToString();
        }
    }
}