using Android.App;
using Android.Widget;
using Android.OS;
using System.Diagnostics;
using CloudCoinCore;
using CloudCoinCEMobile.CoreClasses;
using System.Threading;

namespace CloudCoinCEMobile
{
    [Activity(Label = "CloudCoin Consumers Edition", MainLauncher = true, Icon = "@drawable/launcher")]
    public class MainActivity : Activity
    {
        public static FileSystem FS = new FileSystem(CloudCoinApplication.RootPath);
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
             SetContentView (Resource.Layout.Main);
            System.Diagnostics.Debug.WriteLine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
        }

        protected void Init()
        {
            Thread t = new Thread(() =>
            {
                FS.LoadFileSystem();
            });
            t.IsBackground = true;
            t.Start();
        }
    }
}

