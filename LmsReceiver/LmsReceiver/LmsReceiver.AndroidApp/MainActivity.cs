using System;
using System.IO;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace LmsReceiver.AndroidApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        MMSReceiver mms;
        SMSReceiver sms;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            mms = new MMSReceiver();
            sms = new SMSReceiver();

            var filter2 = new IntentFilter("android.provider.Telephony.WAP_PUSH_RECEIVED");
            filter2.AddDataType("application/vnd.wap.mms-message");
            RegisterReceiver(mms, filter2);

            var filter3 = new IntentFilter("android.provider.Telephony.MMS_RECEIVED");
            RegisterReceiver(mms, filter3);

            var filter1 = new IntentFilter("android.provider.Telephony.SMS_RECEIVED");
            RegisterReceiver(sms, filter1);

            Button button = FindViewById<Button>(Resource.Id.button1);
            button.Click += Btn_Click;
        }


        private void Btn_Click(object sender, EventArgs e)
        {
            
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        protected override void OnStop()
        {
            base.OnStop();

            try
            {

                UnregisterReceiver(mms);
                UnregisterReceiver(sms);
            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }
    }
}
