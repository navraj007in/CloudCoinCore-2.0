using Android.App;
using Android.Widget;
using Android.OS;
using System.Diagnostics;
namespace CloudCoinCEMobile
{
    [Activity(Label = "CloudCoin Consumers Edition", MainLauncher = true, Icon = "@drawable/launcher")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
             SetContentView (Resource.Layout.Main);
            System.Diagnostics.Debug.WriteLine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
        }
    }
}

