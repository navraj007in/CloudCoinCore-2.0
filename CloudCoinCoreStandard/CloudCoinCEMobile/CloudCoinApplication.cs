using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CloudCoinCore;
using CloudCoinCEMobile.CoreClasses;
using System.IO;

namespace CloudCoinCEMobile
{
    [Application]
    public partial class CloudCoinApplication : Application
    {
        string RootPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + System.IO.Path.DirectorySeparatorChar + "CloudCoin";
        FileSystem FS ;

        public CloudCoinApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {

        }
        public override void OnCreate()
        {
            // If OnCreate is overridden, the overridden c'tor will also be called.
            base.OnCreate();
            System.Diagnostics.Debug.WriteLine(RootPath);
            FS = new FileSystem(RootPath);
            FS.CreateFolderStructure();
            var dirs = Directory.EnumerateDirectories(FS.RootPath);
            foreach(var dir in dirs)
            {
                System.Diagnostics.Debug.WriteLine("Folder - " + dir);
            }
        }
    }
}