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

namespace Celebrium_WPF.ViewModels
{
    class AddCollectableViewModel : BaseNavigationViewModel
    {
        public AddCollectableViewModel()
        {
            Title1 = "ADD";
            Title2 = "MEMO";
            ShowFirst = System.Windows.Visibility.Visible;
            ShowLast = System.Windows.Visibility.Collapsed;
            ProgressValue = 0;
            ProgressStatus = "Not started";
        }

        private void mChooseFiles(object obj)
        {
            //TODO write code here
            System.Windows.Forms.MessageBox.Show("Write the choose files logic here and update the ProgressValue + ProgressStatus properties");
            var files = Directory
               .GetFiles(MainWindow.FS.ImportFolder)
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
            detect();

        }

        private bool PickFiles()
        {

            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Cloudcoins (*.stack, *.jpg,*.jpeg)|*.stack;*.jpg;*.jpeg|Stack files (*.stack)|*.stack|Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
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

        private async void detect()
        {
            MainWindow.updateLog("Starting Multi Detect..");
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            MainWindow.FS.LoadFileSystem();

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

                var tasks = App.raida.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
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


            App.Current.Dispatcher.Invoke(delegate
            {
                MainWindow.FS.LoadFileSystem();
                //enableUI();
                //ShowCoins();
            });

        }
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

    }
}
