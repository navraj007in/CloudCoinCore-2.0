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
using System.Diagnostics;

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
            ImportFolder = RootPath  + Config.TAG_IMPORT + Path.DirectorySeparatorChar;
            ExportFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_EXPORT + Path.DirectorySeparatorChar;
            ImportedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_IMPORTED + Path.DirectorySeparatorChar;
            TemplateFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_TEMPLATES + Path.DirectorySeparatorChar;
            LanguageFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_LANGUAGE + Path.DirectorySeparatorChar;
            CounterfeitFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_COUNTERFEIT + Path.DirectorySeparatorChar;
            PartialFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_PARTIAL + Path.DirectorySeparatorChar;
            FrackedFolder = RootPath + Path.DirectorySeparatorChar+  Config.TAG_FRACKED + Path.DirectorySeparatorChar;
            DetectedFolder = RootPath + Path.DirectorySeparatorChar+ Config.TAG_DETECTED+ Path.DirectorySeparatorChar;
            SuspectFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_SUSPECT+ Path.DirectorySeparatorChar;
            TrashFolder = RootPath + Path.DirectorySeparatorChar +  Config.TAG_TRASH+ Path.DirectorySeparatorChar;
            BankFolder = RootPath + Path.DirectorySeparatorChar +  Config.TAG_BANK + Path.DirectorySeparatorChar;
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

        public void CopyTemplates()
        {
            string[] fileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (String fileName in fileNames)
            {
                if (fileName.Contains("jpeg") || fileName.Contains("jpg"))
                {
                    try
                    {
                        string outputpath = Properties.Settings.Default.WorkSpace + "Templates" + System.IO.Path.DirectorySeparatorChar + fileName.Substring(26);
                        //updateLog(outputpath);
                        using (FileStream fileStream = File.Create(outputpath))
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName).CopyTo(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
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
            exportCoins  =LoadFolderCoins(ExportFolder);
            bankCoins  =LoadFolderCoins(BankFolder);
            lostCoins  = LoadFolderCoins(LostFolder);
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

        private bool importJPEG(String fileName)//Move one jpeg to suspect folder. 
        {
            bool isSuccessful = false;
            // Console.Out.WriteLine("Trying to load: " + this.fileUtils.importFolder + fileName );
            Debug.WriteLine("Trying to load: " + ImportFolder + fileName);
            try
            {
                //  Console.Out.WriteLine("Loading coin: " + fileUtils.importFolder + fileName);
                //CloudCoin tempCoin = this.fileUtils.loadOneCloudCoinFromJPEGFile( fileUtils.importFolder + fileName );

                /*Begin import from jpeg*/

                /* GET the first 455 bytes of he jpeg where the coin is located */
                String wholeString = "";
                byte[] jpegHeader = new byte[455];
                // Console.Out.WriteLine("Load file path " + fileUtils.importFolder + fileName);
                FileStream fileStream = new FileStream(ImportFolder + fileName, FileMode.Open, FileAccess.Read);
                try
                {
                    int count;                            // actual number of bytes read
                    int sum = 0;                          // total number of bytes read

                    // read until Read method returns 0 (end of the stream has been reached)
                    while ((count = fileStream.Read(jpegHeader, sum, 455 - sum)) > 0)
                        sum += count;  // sum is a buffer offset for next reading
                }
                finally
                {
                    fileStream.Close();
                }
                wholeString = bytesToHexString(jpegHeader);

                CloudCoin tempCoin = parseJpeg(wholeString);
                // Console.Out.WriteLine("From FileUtils returnCC.fileName " + tempCoin.fileName);

                /*end import from jpeg file */



                //   Console.Out.WriteLine("Loaded coin filename: " + tempCoin.fileName);

                writeTo(SuspectFolder, tempCoin);
                return true;
            }
            catch (FileNotFoundException ex)
            {
                Console.Out.WriteLine("File not found: " + fileName + ex);
                //CoreLogger.Log("File not found: " + fileName + ex);
            }
            catch (IOException ioex)
            {
                Console.Out.WriteLine("IO Exception:" + fileName + ioex);
                //CoreLogger.Log("IO Exception:" + fileName + ioex);
            }// end try catch
            return isSuccessful;
        }
        public override void DetectPreProcessing()
        {
            //Preprocess Coins before Detection Starts
            //This moves all the coins from Import list into predetect list
            foreach(var coin in importCoins)
            {
                string fileName = coin.FileName;
                int coinExists = (from x in predetectCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if(coinExists>0)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coin);
                using(StreamWriter sw = new StreamWriter(PreDetectFolder + fileName + ".stack"))
                using(JsonWriter writer = new JsonTextWriter(sw))
                {
                       serializer.Serialize(writer, stack);
                }
            } 
       }

         public override void ProcessCoins(IEnumerable<CloudCoin> coins)
        {
           
            var detectedCoins = LoadFolderCoins(DetectedFolder);
            

            foreach(var coin in detectedCoins)
            {
                if(coin.PassCount >= CloudCoinCore.Config.PassCount)
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
            using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + fileName + ".stack"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }

        public void MoveCoins(IEnumerable<CloudCoin> coins,string sourceFolder, string targetFolder)
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
                    using (StreamWriter sw = new StreamWriter(targetFolder + fileName + ".stack"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, stack);
                    }
                    File.Delete(sourceFolder+ coin.FileName+".stack");
                }
                catch(Exception e)
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

        public void WriteCoinsToFile(IEnumerable<CloudCoin> coins,string fileName)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coins.ToArray());
            using (StreamWriter sw = new StreamWriter(fileName + ".stack"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }

        public void WriteToFile(IEnumerable<CloudCoin> coins, string fileName)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coins.ToArray());
            using (StreamWriter sw = new StreamWriter(fileName ))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }
        public void WriteCoin(IEnumerable<CloudCoin> coins, string folder,bool writeAll=false)
        {
            if(writeAll)
            {
                string fileName = Utils.RandomString(16) + ".stack";
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coins.ToArray());
                using (StreamWriter sw = new StreamWriter(folder + fileName + ".stack"))
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
                using (StreamWriter sw = new StreamWriter(folder + fileName + ".stack"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }

            }
        }

        public override void ClearCoins(string FolderName)
        {
            
            var fii = GetFiles(FolderName,CloudCoinCore.Config.allowedExtensions);

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
