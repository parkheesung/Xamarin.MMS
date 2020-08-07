using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Android;
using Android.Support.V4.App;
using OctopusV3.Core;
using Xamarin.Forms;

namespace MmsReceiver.Droid
{
    [Activity(Label = "MmsReceiver", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity MainActivityInstance { get; private set; }
        private int REQUEST_LOCATION { get; set; } = -1;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            MainActivityInstance = this;
            LoadApplication(new App());

            await CheckAndRequestMessagePermission();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            this.REQUEST_LOCATION = requestCode;

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Internet) == false)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.Internet }, REQUEST_LOCATION);
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async Task<PermissionStatus> CheckAndRequestMessagePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
            }

            return status;
        }

    }
}