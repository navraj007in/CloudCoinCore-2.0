using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCoinCore;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;


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
        public static IEnumerable<CloudCoin> lostCoins;
        public static IEnumerable<CloudCoin> predetectCoins;


        public FileSystem(string RootPath)
        {
            this.RootPath = RootPath;
            ImportFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_IMPORT + Path.DirectorySeparatorChar;
            ExportFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_EXPORT + Path.DirectorySeparatorChar;
            ImportedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_IMPORTED + Path.DirectorySeparatorChar;
            TemplateFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_TEMPLATES + Path.DirectorySeparatorChar;
            LanguageFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_LANGUAGE + Path.DirectorySeparatorChar;
            CounterfeitFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_COUNTERFEIT + Path.DirectorySeparatorChar;
            PartialFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_PARTIAL + Path.DirectorySeparatorChar;
            FrackedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_FRACKED + Path.DirectorySeparatorChar;
            DetectedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_DETECTED + Path.DirectorySeparatorChar;
            SuspectFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_SUSPECT + Path.DirectorySeparatorChar;
            TrashFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_TRASH + Path.DirectorySeparatorChar;
            BankFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_BANK + Path.DirectorySeparatorChar;
            PreDetectFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_PREDETECT + Path.DirectorySeparatorChar;
            LostFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_LOST + Path.DirectorySeparatorChar;
            RequestsFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_REQUESTS + Path.DirectorySeparatorChar;
            DangerousFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_DANGEROUS + Path.DirectorySeparatorChar;

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
                Directory.CreateDirectory(ImportFolder);
                Directory.CreateDirectory(ExportFolder);
                Directory.CreateDirectory(BankFolder);
                Directory.CreateDirectory(ImportedFolder);
                Directory.CreateDirectory(LostFolder);
                Directory.CreateDirectory(TrashFolder);
                Directory.CreateDirectory(SuspectFolder);
                Directory.CreateDirectory(DetectedFolder);
                Directory.CreateDirectory(FrackedFolder);
                Directory.CreateDirectory(TemplateFolder);
                Directory.CreateDirectory(PartialFolder);
                Directory.CreateDirectory(CounterfeitFolder);
                Directory.CreateDirectory(LanguageFolder);
                Directory.CreateDirectory(PreDetectFolder);
                Directory.CreateDirectory(RequestsFolder);
                Directory.CreateDirectory(DangerousFolder);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }



        public override void LoadFileSystem()
        {
            importCoins = LoadFolderCoins(ImportFolder);
            exportCoins = LoadFolderCoins(ExportFolder);
            bankCoins = LoadFolderCoins(BankFolder);
            lostCoins = LoadFolderCoins(LostFolder);
            importedCoins = LoadFolderCoins(ImportedFolder);
            trashCoins = LoadFolderCoins(TrashFolder);
            suspectCoins = LoadFolderCoins(SuspectFolder);
            detectedCoins = LoadFolderCoins(DetectedFolder);
            frackedCoins = LoadFolderCoins(FrackedFolder);
            LoadFolderCoins(TemplateFolder);
            partialCoins = LoadFolderCoins(PartialFolder);
            //counterfeitCoins = LoadFolderCoins(CounterfeitFolder);
            predetectCoins = LoadFolderCoins(PreDetectFolder);

        }


        public override void DetectPreProcessing()
        {
            //Preprocess Coins before Detection Starts
            //This moves all the coins from Import list into predetect list
            foreach (var coin in importCoins)
            {
                string fileName = coin.FileName;
                int coinExists = (from x in predetectCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if (coinExists > 0)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coin);
                FileStream fileStream = new FileStream(PreDetectFolder + fileName + ".stack", FileMode.Create);

                using (StreamWriter sw = new StreamWriter(fileStream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }
            }
        }

        public override void ProcessCoins(IEnumerable<CloudCoin> coins)
        {

            var detectedCoins = LoadFolderCoins(DetectedFolder);


            foreach (var coin in detectedCoins)
            {
                if (coin.PassCount >= CloudCoinCore.Config.PassCount)
                {
                    WriteCoin(coin, BankFolder);
                }
                else
                {
                    WriteCoin(coin, CounterfeitFolder);
                }
            }
        }

        public void WriteCoin(CloudCoin coin, string folder)
        {
            var folderCoins = LoadFolderCoins(folder);
            string fileName = coin.FileName;
            int coinExists = (from x in folderCoins
                              where x.sn == coin.sn
                              select x).Count();
            if (coinExists > 0)
            {
                string suffix = Utils.RandomString(16);
                fileName += suffix.ToLower();
            }
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coin);
            FileStream fileStream = new FileStream(folder + Path.DirectorySeparatorChar + fileName + ".stack", FileMode.Create);
            using (StreamWriter sw = new StreamWriter(fileStream))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }

        public void MoveCoins(IEnumerable<CloudCoin> coins, string sourceFolder, string targetFolder)
        {
            var folderCoins = LoadFolderCoins(targetFolder);

            foreach (var coin in coins)
            {
                string fileName = coin.FileName;
                int coinExists = (from x in folderCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if (coinExists > 0)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    Stack stack = new Stack(coin);
                    FileStream fileStream = new FileStream(targetFolder + fileName + ".stack", FileMode.Create);
                    using (StreamWriter sw = new StreamWriter(fileStream))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, stack);
                    }
                    File.Delete(sourceFolder + coin.FileName + ".stack");
                }
                catch (Exception e)
                {

                }


            }
        }

        public void RemoveCoins(IEnumerable<CloudCoin> coins, string folder)
        {

            foreach (var coin in coins)
            {
                File.Delete(folder + coin.FileName + ".stack");

            }
        }

        public void WriteCoinsToFile(IEnumerable<CloudCoin> coins, string fileName)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coins.ToArray());
            FileStream fileStream = new FileStream(fileName + ".stack", FileMode.Create);
            using (StreamWriter sw = new StreamWriter(fileStream))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }
        public void WriteCoin(IEnumerable<CloudCoin> coins, string folder, bool writeAll = false)
        {
            if (writeAll)
            {
                string fileName = Utils.RandomString(16) + ".stack";
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coins.ToArray());
                FileStream fileStream = new FileStream(folder + fileName + ".stack", FileMode.Create);
                using (StreamWriter sw = new StreamWriter(fileStream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }
                return;
            }
            var folderCoins = LoadFolderCoins(folder);

            foreach (var coin in coins)
            {
                string fileName = coin.FileName;
                int coinExists = (from x in folderCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if (coinExists > 0)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coin);
                FileStream fileStream = new FileStream(folder + fileName + ".stack", FileMode.Create);
                using (StreamWriter sw = new StreamWriter(fileStream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }

            }
        }

        public override void ClearCoins(string FolderName)
        {

            var fii = GetFiles(FolderName, CloudCoinCore.Config.allowedExtensions);

            DirectoryInfo di = new DirectoryInfo(FolderName);


            foreach (FileInfo file in fii)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }

        }
        public List<FileInfo> GetFiles(string path, params string[] extensions)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string ext in extensions)
                list.AddRange(new DirectoryInfo(path).GetFiles("*" + ext).Where(p =>
                      p.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                      .ToArray());
            return list;
        }


        public override void MoveImportedFiles()
        {
            var files = Directory
              .GetFiles(ImportFolder)
              .Where(file => CloudCoinCore.Config.allowedExtensions.Any(file.ToLower().EndsWith))
              .ToList();

            string[] fnames = new string[files.Count()];
            for (int i = 0; i < files.Count(); i++)
            {
                MoveFile(files[i], ImportedFolder + Path.DirectorySeparatorChar + Path.GetFileName(files[i]), FileMoveOptions.Rename);
            }
        }
    }


}
