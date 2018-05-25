using CloudCoinClient.CoreClasses;
using CloudCoinCore;
using CloudCoinCoreDirectory;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace Celebrium_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static String rootFolder = AppDomain.CurrentDomain.BaseDirectory + "Data" + Path.DirectorySeparatorChar; 
        public static String importFolder = rootFolder + "Import" + System.IO.Path.DirectorySeparatorChar;
        public static String importedFolder = rootFolder + "Imported" + System.IO.Path.DirectorySeparatorChar;
        public static String trashFolder = rootFolder + "Trash" + System.IO.Path.DirectorySeparatorChar;
        public static String suspectFolder = rootFolder + "Suspect" + System.IO.Path.DirectorySeparatorChar;
        public static String frackedFolder = rootFolder + "Fracked" + System.IO.Path.DirectorySeparatorChar;
        public static String bankFolder = rootFolder + "Bank" + System.IO.Path.DirectorySeparatorChar;
        public static String templateFolder = rootFolder + "Templates" + System.IO.Path.DirectorySeparatorChar;
        public static String counterfeitFolder = rootFolder + "Counterfeit" + System.IO.Path.DirectorySeparatorChar;
        public static String directoryFolder = rootFolder + "Directory" + System.IO.Path.DirectorySeparatorChar;
        public static String exportFolder = rootFolder + "Export" + System.IO.Path.DirectorySeparatorChar;
        public static String languageFolder = rootFolder + "Language" + System.IO.Path.DirectorySeparatorChar;
        public static String partialFolder = rootFolder + "Partial" + System.IO.Path.DirectorySeparatorChar;
        public static String detectedFolder = rootFolder + "Detected" + System.IO.Path.DirectorySeparatorChar;
        public static String infoFolder = rootFolder + "Info" + System.IO.Path.DirectorySeparatorChar;

        private FileSystem fileUtils;
        public static String logsFolder = rootFolder + "Logs" + System.IO.Path.DirectorySeparatorChar;
        public static RAIDA raida;// = RAIDA.GetInstance();

        protected override void OnStartup(StartupEventArgs e)
        {
           // SetAssociationWithExtension(".abc", "ABC File Editor", Application.ExecutablePath, "Celebrium Memo");
            // Check if this was launched by double-clicking a doc. If so, use that as the
            // startup file name.
            //parseDirectoryJSON();
            string json = loadDirectory();
            if (json == "")
            {
                MessageBox.Show("Directory could not be loaded.Trying to load backup!!");
                try
                {
                    parseDirectoryJSON();
                }
                catch (Exception exe)
                {
                    MessageBox.Show("Directory loading from backup failed.Quitting!!");
                    Environment.Exit(1);

                }
            }
            parseDirectoryJSON(json);
            if (raida.network.raida.Count() == 0)
            {
                MessageBox.Show("No Valid Network found.Quitting!!");
                System.Environment.Exit(1);
            }

            if (AppDomain.CurrentDomain.SetupInformation
                .ActivationArguments != null)
                if (AppDomain.CurrentDomain.SetupInformation
                    .ActivationArguments.ActivationData != null
                && AppDomain.CurrentDomain.SetupInformation
                    .ActivationArguments.ActivationData.Length > 0)
                {
                    string fname = "No filename given";
                    try
                    {
                        fname = AppDomain.CurrentDomain.SetupInformation
                                .ActivationArguments.ActivationData[0];

                        // It comes in as a URI; this helps to convert it to a path.
                        Uri uri = new Uri(fname);
                        fname = uri.LocalPath;

                        this.Properties["ArbitraryArgName"] = fname;
                        // System.IO.File.Copy(fname, "");
                    }
                    catch (Exception ex)
                    {
                        // For some reason, this couldn't be read as a URI.
                        // Do what you must...
                    }
                }

            setupFolders();

            base.OnStartup(e);
        }


        public static void SetAssociationWithExtension(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            try
            {
                RegistryKey BaseKey;
                RegistryKey OpenMethod;
                RegistryKey Shell;
                RegistryKey CurrentUser;

                BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
                BaseKey.SetValue("", KeyName);

                OpenMethod = Registry.ClassesRoot.CreateSubKey(KeyName);
                OpenMethod.SetValue("", FileDescription);
                OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
                Shell = OpenMethod.CreateSubKey("Shell");
                Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
                Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
                BaseKey.Close();
                OpenMethod.Close();
                Shell.Close();

                CurrentUser = Registry.CurrentUser.CreateSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.ucs");
                CurrentUser = CurrentUser.OpenSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
                CurrentUser.SetValue("Progid", KeyName, RegistryValueKind.String);
                CurrentUser.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private int GetNetworkNumber(RAIDADirectory dir)
        {
            if (Celebrium_WPF. Properties.Settings.Default.NetworkNumber == 0)
                return 0;
            if (Celebrium_WPF.Properties.Settings.Default.NetworkNumber > dir.networks.Count())
                return 0;

            return Celebrium_WPF.Properties.Settings.Default.NetworkNumber - 1;

        }
        public void parseDirectoryJSON()
        {
            try
            {
                string json = File.ReadAllText(Environment.CurrentDirectory + @"\directory2.json");

                JavaScriptSerializer ser = new JavaScriptSerializer();
                var dict = ser.Deserialize<Dictionary<string, object>>(json);


                RAIDADirectory dir = ser.Deserialize<RAIDADirectory>(json);

                raida = RAIDA.GetInstance(dir.networks[GetNetworkNumber(dir)]);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void parseDirectoryJSON(string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var dict = ser.Deserialize<Dictionary<string, object>>(json);

            RAIDADirectory dir = ser.Deserialize<RAIDADirectory>(json);

            raida = RAIDA.GetInstance(dir.networks[GetNetworkNumber(dir)]);
        }


        public void setupFolders()
        {
            //rootFolder = getWorkspace();
            Directory.CreateDirectory(rootFolder);
            Directory.CreateDirectory(infoFolder);

            importFolder = rootFolder + "Import" + System.IO.Path.DirectorySeparatorChar;
            importedFolder = rootFolder + "Imported" + System.IO.Path.DirectorySeparatorChar;
            trashFolder = rootFolder + "Trash" + System.IO.Path.DirectorySeparatorChar;
            suspectFolder = rootFolder + "Suspect" + System.IO.Path.DirectorySeparatorChar;
            frackedFolder = rootFolder + "Fracked" + System.IO.Path.DirectorySeparatorChar;
            bankFolder = rootFolder + "Bank" + System.IO.Path.DirectorySeparatorChar;
            templateFolder = rootFolder + "Templates" + System.IO.Path.DirectorySeparatorChar;
            counterfeitFolder = rootFolder + "Counterfeit" + System.IO.Path.DirectorySeparatorChar;
            directoryFolder = rootFolder + "Directory" + System.IO.Path.DirectorySeparatorChar;
            exportFolder = rootFolder + "Export" + System.IO.Path.DirectorySeparatorChar;
            languageFolder = rootFolder + "Language" + System.IO.Path.DirectorySeparatorChar;
            partialFolder = rootFolder + "Partial" + System.IO.Path.DirectorySeparatorChar;
            detectedFolder = rootFolder + "Detected" + System.IO.Path.DirectorySeparatorChar;



            fileUtils = new FileSystem(rootFolder);
            fileUtils.CreateFolderStructure();

        }
        public string getWorkspace()
        {
            string workspace = "";

            return workspace;
        }
        public string loadDirectory()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string s = client.DownloadString(Config.URL_DIRECTORY);
                    return s;
                }
                catch (Exception e)
                {

                }
            }
            return "";
        }
    }
}
