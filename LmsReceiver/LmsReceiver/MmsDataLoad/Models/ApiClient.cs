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

namespace MmsDataLoad
{
    public class ApiClient
    {
        private static HttpClient client { get; set; } = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());

        public static string GetRequest(string url)
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

        public static string PostRequest(string url, FormUrlEncodedContent postValue)
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