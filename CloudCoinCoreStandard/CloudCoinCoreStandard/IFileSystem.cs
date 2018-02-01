using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CloudCoinCore
{
    public abstract class IFileSystem
    {
        public string RootPath { get; set; }
        public string ImportFolder { get; set; }
        public string ExportFolder { get; set; }
        public string BankFolder { get; set; }
        public string ImportedFolder { get; set; }
        public string LostFolder { get; set; }
        public string TrashFolder { get; set; }
        public string SuspectFolder { get; set; }
        public string DetectedFolder { get; set; }
        public string FrackedFolder { get; set; }
        public string TemplateFolder { get; set; }
        public string PartialFolder { get; set; }
        public string CounterfeitFolder { get; set; }
        public string LanguageFolder { get; set; }
        public string PreDetectFolder { get; set; }
        public string RequestsFolder { get; set; }

        //public abstract IFileSystem(string path);

        public abstract bool CreateFolderStructure();

        public abstract void LoadFileSystem();

        public abstract void ClearCoins(string FolderName);

        public List<CloudCoin> LoadFolderCoins(string folder)
        {
            List<CloudCoin> folderCoins = new List<CloudCoin>();

           
            // Get All the supported CloudCoin Files from the folder
            var files = Directory
                .GetFiles(folder)
                .Where(file => Config.allowedExtensions.Any(file.ToLower().EndsWith))
                .ToList();

            string[] fnames = new string[files.Count()];
            for (int i = 0; i < files.Count(); i++)
            {
                fnames[i] = Path.GetFileName(files.ElementAt(i));
                var coins = Utils.LoadJson(files[i]);
                if(coins !=null )
                    folderCoins.AddRange(coins);
            };

            return folderCoins;
        }

        public List<FileInfo> LoadFiles(string folder)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            var files = Directory
                .GetFiles(folder)
                .ToList();
            foreach (var item in files)
            {
                fileInfos.Add(new FileInfo(item));
                Debug.WriteLine("Read File-"+item);
            }

            Debug.WriteLine("Total " + files.Count + " items read");

            return fileInfos;
        }

        public List<FileInfo> LoadFiles(string folder,string[] allowedExtensions)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            var files = Directory
                .GetFiles(folder)
                .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                .ToList();
            foreach (var item in files)
            {
                fileInfos.Add(new FileInfo(item));
                //Debug.WriteLine(item);
            }

            //Debug.WriteLine("Total " + files.Count + " items read");

            return fileInfos;
        }

        public abstract void ProcessCoins(IEnumerable<CloudCoin> coins);
        public abstract void DetectPreProcessing();

        
        public CloudCoin loadOneCloudCoinFromJsonFile(String loadFilePath)
        {

            CloudCoin returnCC = new CloudCoin();

            //Load file as JSON
            String incomeJson = this.importJSON(loadFilePath);
            //STRIP UNESSARY test
            int secondCurlyBracket = ordinalIndexOf(incomeJson, "{", 2) - 1;
            int firstCloseCurlyBracket = ordinalIndexOf(incomeJson, "}", 0) - secondCurlyBracket;
            // incomeJson = incomeJson.Substring(secondCurlyBracket, firstCloseCurlyBracket);
            incomeJson = incomeJson.Substring(secondCurlyBracket, firstCloseCurlyBracket + 1);
            // Console.Out.WriteLine(incomeJson);
            //Deserial JSON

            try
            {
                returnCC = JsonConvert.DeserializeObject<CloudCoin>(incomeJson);

            }
            catch (JsonReaderException)
            {
                Console.WriteLine("There was an error reading files in your bank.");
                Console.WriteLine("You may have the aoid memo bug that uses too many double quote marks.");
                Console.WriteLine("Your bank files are stored using and older version that did not use properly formed JSON.");
                Console.WriteLine("Would you like to upgrade these files to the newer standard?");
                Console.WriteLine("Your files will be edited.");
                Console.WriteLine("1 for yes, 2 for no.");


            }

            return returnCC;
        }//end load one CloudCoin from JSON

   
      
        public String importJSON(String jsonfile)
        {
            String jsonData = "";
            String line;

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.

                using (var sr = File.OpenText(jsonfile))
                {
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while (true)
                    {
                        line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }//End if line is null
                        jsonData = (jsonData + line + "\n");
                    }//end while true
                }//end using
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file " + jsonfile + " could not be read:");
                Console.WriteLine(e.Message);
            }
            return jsonData;
        }//end importJSON

        // en d json test
        public String setJSON(CloudCoin cc)
        {
            const string quote = "\"";
            const string tab = "\t";
            String json = (tab + tab + "{ " + Environment.NewLine);// {
            json += tab + tab + quote + "nn" + quote + ":" + quote + cc.nn + quote + ", " + Environment.NewLine;// "nn":"1",
            json += tab + tab + quote + "sn" + quote + ":" + quote + cc.sn + quote + ", " + Environment.NewLine;// "sn":"367544",
            json += tab + tab + quote + "an" + quote + ": [" + quote;// "an": ["
            for (int i = 0; (i < 25); i++)
            {
                json += cc.an[i];// 8551995a45457754aaaa44
                if (i == 4 || i == 9 || i == 14 || i == 19)
                {
                    json += quote + "," + Environment.NewLine + tab + tab + tab + quote; //", 
                }
                else if (i == 24)
                {
                    // json += "\""; last one do nothing
                }
                else
                { // end if is line break
                    json += quote + ", " + quote;
                }

                // end else
            }// end for 25 ans

            json += quote + "]," + Environment.NewLine;//"],
            // End of ans
            CoinUtils cu = new CoinUtils(cc);
            cu.calcExpirationDate();
            json += tab + tab + quote + "ed" + quote + ":" + quote + cu.cc.ed + quote + "," + Environment.NewLine; // "ed":"9-2016",
            if (string.IsNullOrEmpty(cc.pown)) { cc.pown = "uuuuuuuuuuuuuuuuuuuuuuuuu"; }//Set pown to unknow if it is not set. 
            json += tab + tab + quote + "pown" + quote + ":" + quote + cc.pown + quote + "," + Environment.NewLine;// "pown":"uuupppppffpppppfuuf",
            json += tab + tab + quote + "aoid" + quote + ": []" + Environment.NewLine;
            json += tab + tab + "}" + Environment.NewLine;
            // Keep expiration date when saving (not a truley accurate but good enought )
            return json;
        }
        // end get JSON

        public int ordinalIndexOf(String str, String substr, int n)
        {
            int pos = str.IndexOf(substr);
            while (--n > 0 && pos != -1)
                pos = str.IndexOf(substr, pos + 1);
            return pos;
        }

        public string bytesToHexString(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            int length = data.Length;
            char[] hex = new char[length * 2];
            int num1 = 0;
            for (int index = 0; index < length * 2; index += 2)
            {
                byte num2 = data[num1++];
                hex[index] = GetHexValue(num2 / 0x10);
                hex[index + 1] = GetHexValue(num2 % 0x10);
            }
            return new string(hex);
        }//End NewConverted

        private char GetHexValue(int i)
        {
            if (i < 10)
            {
                return (char)(i + 0x30);
            }
            return (char)((i - 10) + 0x41);
        }//end GetHexValue
    }
}
