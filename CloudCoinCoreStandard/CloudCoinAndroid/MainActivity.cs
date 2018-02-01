using Android.App;
using Android.Widget;
using Android.OS;

using CloudCoinCore;
using CloudCoinClient.CoreClasses;

namespace CloudCoinAndroid
{
    [Activity(Label = "CloudCoinAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        FileSystem FS = new FileSystem("");
        RAIDA raida;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            // SetContentView (Resource.Layout.Main);
            Setup();
        }

        public void Setup()
        {
            // Create the Folder Structure
            FS.CreateFolderStructure();
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            //raida.Echo();
            //Load Local Coins

        }

    }
}

