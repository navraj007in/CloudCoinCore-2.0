using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCoinCore;
using System.IO;

namespace CloudCoinClient.CoreClasses
{
    public class FileSystem : IFileSystem
    {
        public static IEnumerable<CloudCoin> importCoins;
        public static IEnumerable<CloudCoin> exportCoins;
        public static IEnumerable<CloudCoin> importedCoins;
        public static IEnumerable<FileInfo> templateFiles;
        public static IEnumerable<CloudCoin> languageCoins;
        public static IEnumerable<CloudCoin> counterfeitCoins;
        public static IEnumerable<CloudCoin> partialCoins;
        public static IEnumerable<CloudCoin> frackedCoins;
        public static IEnumerable<CloudCoin> detectedCoins;
        public static IEnumerable<CloudCoin> suspectCoins;
        public static IEnumerable<CloudCoin> trashCoins;
        public static IEnumerable<CloudCoin> bankCoins;


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
            BankFolder = Config.TAG_BANK;

        }
        public override bool CreateFolderStructure()
        {
            RootPath = AppContext.BaseDirectory + System.IO.Path.DirectorySeparatorChar;
            Console.WriteLine(RootPath);
            
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
            LoadFolderCoins(RootPath + ImportFolder);
            LoadFolderCoins(RootPath + ExportFolder);
            LoadFolderCoins(RootPath + BankFolder);
            LoadFolderCoins(RootPath + LostFolder);
            LoadFolderCoins(RootPath + ImportedFolder);
            LoadFolderCoins(RootPath + TrashFolder);
            LoadFolderCoins(RootPath + SuspectFolder);
            LoadFolderCoins(RootPath + DetectedFolder);
            LoadFolderCoins(RootPath + FrackedFolder);
            LoadFolderCoins(RootPath + TemplateFolder);
            LoadFolderCoins(RootPath + PartialFolder);
            LoadFolderCoins(RootPath + CounterfeitFolder);
            LoadFolderCoins(RootPath + LanguageFolder);
        }
    }
}
