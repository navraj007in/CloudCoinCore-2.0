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
using System.IO;

namespace CloudCoinAndroid.CoreClasses
{
    public class FileSystem : IFileSystem
    {
        public FileSystem()
        {
            ImportFolder = Config.TAG_IMPORT;
            ExportFolder = Config.TAG_EXPORT;
            ImportedFolder = Config.TAG_IMPORTED;
            TemplateFolder = Config.TAG_TEMPLATES;
            LanguageFolder = Config.TAG_LANGUAGE;
            CounterfeitFolder = Config.TAG_COUNTERFEIT;
            PartialFolder = Config.TAG_PARTIAL;
            FrackedFolder = Config.TAG_FRACKED;
            DetectedFolder = Config.TAG_DETECTED;
            SuspectFolder = Config.TAG_SUSPECT;
            TrashFolder = Config.TAG_TRASH;
            LanguageFolder = Config.TAG_LANGUAGE;
            BankFolder = Config.TAG_BANK;
            PreDetectFolder = Config.TAG_PARTIAL;

        }
        public override bool CreateFolderStructure()
        {


            // Create the Actual Folder Structure
            return CreateDirectories();
            //return true;
        }


        public bool CreateDirectories()
        {
            // Create Subdirectories as per the RootFolder Location
            // Failure will return false

            try
            {
                Directory.CreateDirectory(RootPath + ImportFolder);
                Directory.CreateDirectory(RootPath + ExportFolder);
                Directory.CreateDirectory(RootPath + BankFolder);
                Directory.CreateDirectory(RootPath + ImportedFolder);
                Directory.CreateDirectory(RootPath + LostFolder);
                Directory.CreateDirectory(RootPath + TrashFolder);
                Directory.CreateDirectory(RootPath + SuspectFolder);
                Directory.CreateDirectory(RootPath + DetectedFolder);
                Directory.CreateDirectory(RootPath + FrackedFolder);
                Directory.CreateDirectory(RootPath + TemplateFolder);
                Directory.CreateDirectory(RootPath + PartialFolder);
                Directory.CreateDirectory(RootPath + CounterfeitFolder);
                Directory.CreateDirectory(RootPath + LanguageFolder);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public override void LoadFileSystem()
        {

        }
    }
    
}