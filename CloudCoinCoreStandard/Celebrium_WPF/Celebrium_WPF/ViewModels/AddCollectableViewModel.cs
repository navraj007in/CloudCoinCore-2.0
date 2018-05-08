using Celebrium_WPF.Other;
using CloudCoinClient.CoreClasses;
using CloudCoinCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Media;

using System.Windows.Controls;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Net;

namespace Celebrium_WPF.ViewModels
{
    class AddCollectableViewModel : BaseNavigationViewModel
    {
        List<CloudCoin> memoCoins = new List<CloudCoin>();
        public AddCollectableViewModel()
        {
            Title1 = "ADD";
            Title2 = "MEMO";
            ShowFirst = System.Windows.Visibility.Visible;
            ShowLast = System.Windows.Visibility.Collapsed;
            ProgressValue = 0;
            ProgressStatus = "Not started";
        }

        public void FetchImages(List<CloudCoin> cloudCoins)
        {
            int count = 1;
            foreach (var jpegCoin in cloudCoins)
            {

                FetchImage(jpegCoin);
                ProgressValue = (int)(count * 100 / cloudCoins.Count);
                ProgressStatus = "Fetching Images. " + ProgressValue + " % completed.";
                count++;
            }
            ProgressValue = 100;
            ProgressStatus = "Fetching Images Completed.";
        }
        private void mChooseFiles(object obj)
        {
            //TODO write code here
            //System.Windows.Forms.MessageBox.Show("Write the choose files logic here and update the ProgressValue + ProgressStatus properties");
            var files = Directory
               .GetFiles(App.templateFolder)
               .Where(file => CloudCoinCore.Config.allowedExtensions.Any(file.ToLower().EndsWith))
               .ToList();

            int filesCount = Directory.GetFiles(MainWindow.FS.ImportFolder).Length;
            if (files.Count() == 0)
            {
                bool pickResult = PickFiles();
                if (!pickResult)
                {
                    //enableUI();
                    return;
                }
            }
            //this below is just an example please adjust to your needs
            //update the values of these properties and the UI will update itself.
            //ProgressValue += 20;
            //ProgressStatus = ProgressValue.ToString() + "%";
            ProgressValue = 0;
            ProgressStatus = "Processing Files";

            new Thread(delegate () {
                memoCoins = ProcessBank().Result;
                MainWindow.logger.Info("Celebriums detected " + memoCoins.Count);
                MessageBox.Show("Celebriums detected " + memoCoins.Count);
                Detect();
            }).Start();

            //memoCoins = ProcessBank().Result;

            //Detect();

        }

        private async Task<List<CloudCoin>> ProcessBank()
        {
            var importCoins = MainWindow.FS.LoadFolderCoins(MainWindow.FS.ImportFolder);
            List<CloudCoin> memoCoins = new List<CloudCoin>();
            int count = 1;
            foreach(var coin in importCoins)
            {
                //string jpegExists = await CheckJpeg(coin);
                string jpegExists = await GetHtmlFromURL(string.Format(Config.URL_JPEG_Exists, Config.NetworkNumber, coin.sn));

                if (jpegExists == "true")
                    memoCoins.Add(coin);
                Console.WriteLine("S. No. -"+ coin.sn +" Coins Exists - "+ jpegExists);
                ProgressValue = (int)(count * 100 / importCoins.Count);
                ProgressStatus = "Detecting Memos. " + ProgressValue + " % completed.";
                count++;
            }
            ProgressValue = 100;
            ProgressStatus = "Memo Detection completed.";
            return memoCoins;
        }
        #region Pickup files
        private bool PickFiles()
        {

            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Cloudcoins (*.celebrium, *.jpg,*.jpeg)|*.celebrium;*.jpg;*.jpeg|Stack files (*.celebrium)|*.celebrium|Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = MainWindow.FS.RootPath;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        try
                        {
                            if (!File.Exists(MainWindow.FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename)))
                                File.Move(filename, MainWindow.FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename));
                            else
                            {
                                string msg = "File " + filename + " already exists. Do you want to overwrite it?";
                                MessageBoxResult result =
                                  MessageBox.Show(
                                    msg,
                                    "CloudCoins",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning);
                                if (result == MessageBoxResult.Yes)
                                {
                                    try
                                    {
                                        File.Delete(MainWindow.FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename));
                                        File.Move(filename, MainWindow.FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename));
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                           // updateLog(ex.Message);
                        }
                    }
                }
                else
                    return false;
            }
            return true;
        }
        #endregion

        #region Celebrium Detection
        private async void Detect()
        {
            MainWindow.updateLog("Starting Multi Detect..");
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            MainWindow.FS.LoadFileSystem();
            ProgressValue = 0;
            ProgressStatus =  "Starting detection";
            // Prepare Coins for Import
            MainWindow.FS.DetectPreProcessing();

            IEnumerable<CloudCoin> predetectCoins = MainWindow.FS.LoadFolderCoins(MainWindow.FS.PreDetectFolder);
            FileSystem.predetectCoins = predetectCoins;

            IEnumerable<CloudCoin> bankCoins = FileSystem.bankCoins;
            IEnumerable<CloudCoin> frackedCoins1 = FileSystem.frackedCoins;

            var bCoins = bankCoins.ToList();
            bCoins.AddRange(frackedCoins1);
            //bankCoins.ToList().AddRange(frackedCoins1);

            var totalBankCoins = bCoins;

            var snList = (from x in totalBankCoins
                          select x.sn).ToList();

            var newCoins = from x in predetectCoins where !snList.Contains(x.sn) select x;
            var existingCoins = from x in predetectCoins where snList.Contains(x.sn) select x;

            foreach (var coin in existingCoins)
            {
                MainWindow.updateLog("Found existing coin :" + coin.sn + ". Skipping.");
                MainWindow.FS.MoveFile(MainWindow.FS.PreDetectFolder + coin.FileName + ".stack", MainWindow.FS.TrashFolder + coin.FileName + ".stack", IFileSystem.FileMoveOptions.Replace);
            }

            predetectCoins = newCoins;
            //return;

            int existIndex = 0;
            List<CloudCoin> ccList = new List<CloudCoin>();
            foreach (var jpegCoin in newCoins)
            {
                string jpegExists = await CheckJpeg(jpegCoin);
                if (jpegExists == "yes")
                {
                    MainWindow.updateLog("Jpeg Found" + jpegCoin.sn);
                  //  ccList.Add(jpegCoin);
                }
                else
                {
                    MainWindow.updateLog("Jpeg Not Found" + jpegCoin.sn);
                    

                }
                ccList.Add(jpegCoin);
                existIndex++;
            }

            predetectCoins = memoCoins;
            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if (predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            ProgressChangedEventArgs pge = new ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for (int i = 0; i < LotCount; i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i * CloudCoinCore.Config.MultiDetectLoad).Take(200);
                coins.ToList().ForEach(x => x.pown = "");
                App.raida.coins = coins;

                var tasks = App.raida.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut,false);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    MainWindow.FS.WriteCoinsToFile(coins, MainWindow.FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        //coin.pown = "";
                        for (int k = 0; k < CloudCoinCore.Config.NodeCount; k++)
                        {
                            coin.response[k] = App.raida.nodes[k].MultiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = coin.pown.Count(x => x == 'p');
                        coin.FailCount = coin.pown.Count(x => x == 'f');
                        CoinCount++;


                        MainWindow.updateLog("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "." + coin.pown);
                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        App.raida.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    ProgressValue = (int)pge.MinorProgress;
                    ProgressStatus = ProgressValue.ToString() + "% Detected";
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    App.raida.OnProgressChanged(pge);
                    MainWindow.FS.WriteCoin(coins, MainWindow.FS.DetectedFolder);
                    MainWindow.FS.RemoveCoins(coins, MainWindow.FS.PreDetectFolder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }


            }
            pge.MinorProgress = 100;
            Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
            ProgressValue = 100;
            ProgressStatus = ProgressValue.ToString() + "% Detected";

            App.raida.OnProgressChanged(pge);
            var detectedCoins = MainWindow.FS.LoadFolderCoins(MainWindow.FS.DetectedFolder);
            //detectedCoins.ForEach(x => x.pown = "pppppppdppppppppppppppppp");
            detectedCoins.ForEach(x => x.SetAnsToPansIfPassed());
            detectedCoins.ForEach(x => x.CalculateHP());
            detectedCoins.ForEach(x => x.CalcExpirationDate());

            // Apply Sort to Folder to all detected coins at once.
            MainWindow.updateLog("Starting Sort.....");
            detectedCoins.ForEach(x => x.SortToFolder());
            MainWindow.updateLog("Ended Sort........");

            var passedCoins = (from x in detectedCoins
                               where x.folder == MainWindow.FS.BankFolder
                               select x).ToList();

            var frackedCoins = (from x in detectedCoins
                                where x.folder == MainWindow.FS.FrackedFolder
                                select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder == MainWindow.FS.CounterfeitFolder
                               select x).ToList();
            var lostCoins = (from x in detectedCoins
                             where x.folder == MainWindow.FS.LostFolder
                             select x).ToList();
            var suspectCoins = (from x in detectedCoins
                                where x.folder == MainWindow.FS.SuspectFolder
                                select x).ToList();
            var dangerousCoins = (from x in detectedCoins
                                  where x.folder == MainWindow.FS.DangerousFolder
                                  select x).ToList();
            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            MainWindow.updateLog("Coin Detection finished.");
            MainWindow.updateLog("Total Passed Coins : " + (passedCoins.Count() + frackedCoins.Count()) + "");
            MainWindow.updateLog("Total Failed Coins : " + failedCoins.Count() + "");
            MainWindow.updateLog("Total Lost Coins : " + lostCoins.Count() + "");
            MainWindow.updateLog("Total Suspect Coins : " + suspectCoins.Count() + "");
            MainWindow.updateLog("Total Skipped Coins : " + existingCoins.Count() + "");
            MainWindow.updateLog("Total Dangerous Coins : " + dangerousCoins.Count() + "");

            // Move Coins to their respective folders after sort
            MainWindow.FS.MoveCoins(passedCoins, MainWindow.FS.DetectedFolder, MainWindow.FS.BankFolder);
            MainWindow.FS.MoveCoins(frackedCoins, MainWindow.FS.DetectedFolder, MainWindow.FS.FrackedFolder);
            MainWindow.FS.WriteCoin(failedCoins, MainWindow.FS.CounterfeitFolder, true);
            MainWindow.FS.MoveCoins(lostCoins, MainWindow.FS.DetectedFolder, MainWindow.FS.LostFolder);
            MainWindow.FS.MoveCoins(suspectCoins, MainWindow.FS.DetectedFolder, MainWindow.FS.SuspectFolder);
            MainWindow.FS.MoveCoins(dangerousCoins, MainWindow.FS.DetectedFolder, MainWindow.FS.DangerousFolder);

            // Clean up Detected Folder
            MainWindow.FS.RemoveCoins(failedCoins, MainWindow.FS.DetectedFolder);
            MainWindow.FS.RemoveCoins(lostCoins, MainWindow.FS.DetectedFolder);
            MainWindow.FS.RemoveCoins(suspectCoins, MainWindow.FS.DetectedFolder);

            MainWindow.FS.MoveImportedFiles();
            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

            Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            MainWindow.updateLog("Detection Completed in - " + ts.TotalMilliseconds / 1000);

            FetchImages(passedCoins);


            App.Current.Dispatcher.Invoke(delegate
            {
                MainWindow.FS.LoadFileSystem();
                //enableUI();
                //ShowCoins();
            });

        }
        #endregion
        public ICommand ChooseFiles
        {
            get { return new ActionCommand(mChooseFiles); }
        }



        private int _progressvalue;

        public int ProgressValue
        {
            get { return _progressvalue; }
            set
            {
                _progressvalue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }
        public static async Task<String> GetHtmlFromURL(String urlAddress)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string data = "";
            try
            {
                using (var cli = new HttpClient())
                {
                    HttpResponseMessage response = await cli.GetAsync(urlAddress);
                    if (response.IsSuccessStatusCode)
                        data = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(data);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return data;
        }//end get HTML
        private string _progressStatus;

        public string ProgressStatus
        {
            get { return _progressStatus.ToUpper(); }
            set
            {
                _progressStatus = value;
                OnPropertyChanged(nameof(ProgressStatus));
            }
        }

        public async Task<string> CheckJpeg(CloudCoin cc)
        {
            string jpegExists = await GetHtmlFromURL(string.Format(Config.URL_JPEG_Exists, Config.NetworkNumber, cc.sn));
            //MessageBox.Show(jpegExists);

            if (jpegExists.Equals("true"))
            {
                //await FetchImage(cc);

            }

            return jpegExists;

        }

        public async Task<string> FetchImage(CloudCoin cc)
        {
            var ticketTask = App.raida.nodes[0].GetTicketResponse(Config.NetworkNumber, cc.sn, cc.an[0], cc.denomination);
            string url = string.Format(App.raida.nodes[3].FullUrl + Config.URL_GET_TICKET, Config.NetworkNumber, cc.sn, cc.an[3], cc.an[3], cc.denomination);
            string resp = await GetHtmlFromURL(url);
            if (resp.Contains("ticket"))
            {
                try
                {
                    String[] KeyPairs = resp.Split(',');
                    String message = KeyPairs[3];
                    int startTicket = Utils.ordinalIndexOf(message, "\"", 3) + 2;
                    int endTicket = Utils.ordinalIndexOf(message, "\"", 4) - startTicket;
                    string resp2 = message.Substring(startTicket - 1, endTicket + 1); //This is the ticket or message
                    url = string.Format(Config.URL_GET_IMAGE, Config.NetworkNumber, cc.sn, 3, resp2);
                    string image = await GetHtmlFromURL(url);
                    image = image.Substring(0, image.Length - 4);
                    int curlpos = image.IndexOf("{");
                    image = image.Substring(curlpos);
                    image = image.Replace("\r", "").Replace("\n", "").Replace("\t", "");

                    String[] KeyPairs2 = image.Split(',');
                    String message2 = KeyPairs2[3];

                    int startTicket1 = Utils.ordinalIndexOf(message2, "\"", 3) + 2;
                    int endTicket1 = Utils.ordinalIndexOf(message2, "\"", 4) - startTicket1;
                    string resp3 = message2.Substring(startTicket1 - 1, endTicket1 + 1); //This is the ticket or message


                    byte[] bytes = Convert.FromBase64String(resp3);

                    MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length);
                    ms.Position = 0;
                    ms.Write(bytes, 0, bytes.Length);
                    System.Drawing.Image imgimg = System.Drawing.Image.FromStream(ms, true);// this line giving exception parameter not valid

                    imgimg.Save(MainWindow.FS.TemplateFolder + System.IO.Path.DirectorySeparatorChar + cc.FileName + ".jpg");
                }
                catch(Exception e)
                {
                    MainWindow.logger.Error(e.Message);
                    Console.WriteLine(e.Message);

                }
               
                //imgCoin.Source = ByteImageConverter.ByteToImage(bytes);

                //GetImageResponse imgresp = JsonConvert.DeserializeObject<GetImageResponse>(image);

                //MessageBox.Show(message2);
                // MessageBox.Show(resp);

            }
            Console.WriteLine(ticketTask);
            return "";
        }

        public Image LoadImage(string img)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(img);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(bytes);
            bi.EndInit();

            Image img1 = new Image();
            img1.Source = bi;

            return img1;
        }

        public Image LoadImageBytes(string img)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(img);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(bytes);
            bi.EndInit();

            Image img1 = new Image();
            img1.Source = bi;

            return img1;
        }
        private void Raida_ProgressChanged(object sender, EventArgs e)
        {
            //  throw new NotImplementedException();
        }
    }
    public class ByteImageConverter
    {
        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }
    }


}

