using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using OctopusV3.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Essentials;

namespace MmsDataLoad
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ICursor cursor;
        private int TryCount = 0;
        public bool IsPermission { get; set; } = false;

        public Timer checkTimer { get; set; } = null;
        public Timer workTimer { get; set; } = null;

        public Timer sendTimer { get; set; } = null;

        protected static TextView msgText = null;
        protected static TextView postionText = null;
        protected static TextView progressText = null;

        //protected List<string> EnabledList { get; set; } = new List<string>();

        const int CheckTime = 1000 * 60 * 2;
        const int SendTime = 1000 * 5;
        const int WorkingTime = 1000 * 60 * 1;
        const string Mode = "mms";


        protected ConcurrentDictionary<string, bool> Keys { get; set; } = new ConcurrentDictionary<string, bool>();
        protected ConcurrentDictionary<string, Coupon> Values { get; set; } = new ConcurrentDictionary<string, Coupon>();
        protected ConcurrentQueue<Coupon> Sender { get; set; } = new ConcurrentQueue<Coupon>();


        public string GetMyPhoneNumber()
        {
            TelephonyManager mgr = Application.Context.GetSystemService(Context.TelephonyService) as TelephonyManager;
            return mgr.Line1Number;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            msgText = (TextView)FindViewById(Resource.Id.testText);
            postionText = (TextView)FindViewById(Resource.Id.testText2);
            progressText = (TextView)FindViewById(Resource.Id.testText3);

            try
            {
                System.String[] reqCols = new System.String[] { "_id" };
                Android.Net.Uri sentURI = Android.Net.Uri.Parse($"content://{Mode}/inbox");
                cursor = ContentResolver.Query(sentURI, reqCols, null, null, "date DESC");

                IsPermission = true;
                if (IsPermission && this.cursor != null)
                {
                    checkTimer = new Timer(Check_Timer_Tick, null, 1000, CheckTime);
                    sendTimer = new Timer(Check_Send_Tick, null, 1000, SendTime);


                    msgText.Text = $"{GetMyPhoneNumber()} : Started!!";
                }
                else
                {
                    msgText.Text = "Stop!!";
                }
            }
            catch (PermissionException pex)
            {
                IsPermission = false;
                msgText.Text = pex.Message;
                Toast.MakeText(this, "권한이 수락되지 않아 앱이 동작하지 않습니다.  다시 시작해 주세요.", ToastLength.Short).Show();
            }
            catch (System.Exception ex)
            {
                IsPermission = false;
                msgText.Text = ex.Message;

                if (ex.Message.IndexOf("Permission", StringComparison.OrdinalIgnoreCase) > -1 && ex.Message.IndexOf("Denial", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    Toast.MakeText(this, "권한이 수락되지 않아 앱이 동작하지 않습니다.  다시 시작해 주세요.", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                }
            }
        }

        private void Check_Send_Tick(object state)
        {
            Coupon coupon = null;
            if (this.Sender.TryDequeue(out coupon))
            {
                ReturnValue result = new ReturnValue();

                try
                {
                    string content = coupon.Body;
                    string key = coupon.ID;
                    string url = "http:" + $"//giftipangpang.com/Api/External/Log";
                    var paramData = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("key", key),
                            new KeyValuePair<string, string>("content", content)
                        });
                    string tmp = ApiClient.PostRequest(url, paramData);
                    if (!string.IsNullOrWhiteSpace(tmp))
                    {
                        result = JsonConvert.DeserializeObject<ReturnValue>(tmp);
                        if (result.Check)
                        {
                            result.Message = "ok";
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    result.Error(ex);
                }

                msgText.Text = result.Message;
            }
        }

        private void Work_Timer_Tick(object state)
        {
            TryCount++;

            if (IsPermission && this.cursor != null)
            {
                workTimer.Dispose();
                workTimer = null;

                string mmsId = string.Empty;

                Coupon coupon = new Coupon();

                try
                {
                    foreach(var key in Keys.Where(x => x.Value == false))
                    {
                        if (Values.TryGetValue(key.Key, out coupon))
                        {
                            coupon.Bind();
                            if (coupon.IsBinding)
                            {
                                Sender.Enqueue(coupon);
                            }
                        }

                        Keys.AddOrUpdate(key.Key, true, (x, y) => true);
                    }

                    progressText.Text = $"{Sender.Count()} / {Values.Count()} / {Keys.Count()}";
                    workTimer = new Timer(Work_Timer_Tick, null, WorkingTime, Timeout.Infinite);
                }
                catch (System.Exception ex)
                {
                    msgText.Text = ex.Message;
                }
                finally
                {
                    postionText.Text = $"Count : {this.TryCount}";
                }
            }
        }

        private string getMmsText(string id)
        {
            Android.Net.Uri partURI = Android.Net.Uri.Parse($"content://{Mode}/part/" + id);
            StringBuilder sb = new StringBuilder(200);
            try
            {
                using (var input = ContentResolver.OpenInputStream(partURI))
                {
                    if (input != null)
                    {
                        InputStreamReader isr = new InputStreamReader(input, "UTF-8");
                        BufferedReader reader = new BufferedReader(isr);
                        string temp = reader.ReadLine();
                        while (temp != null)
                        {
                            sb.Append(temp);
                            temp = reader.ReadLine();
                        }
                    }
                }
            }
            catch
            { 
            }

            return sb.ToString();
        }

        private string getPhone(string id)
        {
            string name = string.Empty;

            try
            {
                string selectionAdd = $"msg_id={id}";
                Android.Net.Uri uriAddress = Android.Net.Uri.Parse($"content://{Mode}/{id}/addr");
                var cAdd = ContentResolver.Query(uriAddress, null, selectionAdd, null, null);

                if (cAdd.MoveToFirst())
                {
                    name = cAdd.GetString(cAdd.GetColumnIndex("address"));
                }
            }
            catch
            {

            }

            return name;
        }

        private void Check_Timer_Tick(object state)
        {
            if (IsPermission && this.cursor != null)
            {
                string mmsId = string.Empty;
                Coupon coupon = null;

                List<int> cursors = new List<int>();
                int point = -1;

                for (int i = 0; i < cursor.Count && cursor != null; i++)
                {
                    if (cursor.MoveToFirst())
                    {
                        cursor.Move(i);
                        string tmp = cursor.GetString(cursor.GetColumnIndex("_id"));
                        if (!string.IsNullOrWhiteSpace(tmp))
                        {
                            if (int.TryParse(tmp, out point))
                            {
                                cursors.Add(point);
                            }
                        }
                    }
                }

                if (cursors != null && cursors.Count > 0)
                {
                    point = cursors.Max();

                    if (point > 0)
                    {
                        mmsId = Convert.ToString(point);
                        Keys.TryAdd(mmsId, false);
                        Android.Net.Uri uri = Android.Net.Uri.Parse($"content://{Mode}/part");
                        string selection = "_id=" + mmsId;
                        var query = ContentResolver.Query(uri, null, selection, null, null);
                        if (query.MoveToFirst())
                        {
                            do
                            {
                                try
                                {
                                    coupon = new Coupon();
                                    coupon.ID = query.GetString(query.GetColumnIndex("_id"));
                                    coupon.ReceiveType = query.GetString(query.GetColumnIndex("ct"));
                                    coupon.Phone = getPhone(coupon.ID).Trim();

                                    string data = query.GetString(query.GetColumnIndex("_data"));
                                    if (data != null)
                                    {
                                        coupon.Body = getMmsText(coupon.ID);
                                    }
                                    else
                                    {
                                        coupon.Body = query.GetString(query.GetColumnIndex("text"));
                                    }

                                    Values.AddOrUpdate(mmsId, coupon, (x, y) => coupon);
                                }
                                catch
                                {
                                }

                            } while (query.MoveToNext());
                        }
                    }
                }

                workTimer = new Timer(Work_Timer_Tick, null, 1000, Timeout.Infinite);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
