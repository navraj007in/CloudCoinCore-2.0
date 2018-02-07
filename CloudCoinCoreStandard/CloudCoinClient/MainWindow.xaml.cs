﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudCoinClient.CoreClasses;
using CloudCoinCore;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.IO;

namespace CloudCoinClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string RootPath;
        FileSystem FS ;
        RAIDA raida;
        int onesCount = 0;
        int fivesCount = 0;
        int qtrCount = 0;
        int hundredsCount = 0;
        int twoFiftiesCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            App.Current.Dispatcher.Invoke(delegate
            {
                disableUI();
            });
            new Thread(delegate () {
                Setup();
            }).Start();
        }

        public void disableUI()
        {
            cmdMultiDetect.IsEnabled = false;
            cmdEcho.IsEnabled = false;
            cmdDetect.IsEnabled = false;
            cmdShow.IsEnabled = false;
        }

        public void enableUI()
        {
            cmdMultiDetect.IsEnabled = true;
            cmdEcho.IsEnabled = true;
            cmdDetect.IsEnabled = true;
            cmdShow.IsEnabled = true;
        }

        public void Setup()
        {
            if (Properties.Settings.Default.WorkSpace == "")
            {
                RootPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                RootPath = Properties.Settings.Default.WorkSpace;
            }
            FS = new FileSystem(RootPath);

            // Create the Folder Structure
            FS.CreateFolderStructure();
            FS.CopyTemplates();
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            raida.FS = FS;
            CoinDetected += Raida_CoinDetected;
            //raida.Echo();
            FS.ClearCoins(FS.PreDetectFolder);
            FS.ClearCoins(FS.DetectedFolder);

            FS.LoadFileSystem();
            //FS.LoadFolderCoins(FS.CounterfeitFolder);
            //Load Local Coins

            App.Current.Dispatcher.Invoke(delegate
            {
                ShowCoins();
                enableUI();
            });


        }

        private void Raida_CoinDetected(object sender, EventArgs e)
        {
            DetectEventArgs eargs = (DetectEventArgs)e;
            Debug.WriteLine("Coin Detection Event Recieved - " +eargs.DetectedCoin.sn);
        }

        private void cmdShow_Click(object sender, RoutedEventArgs e)
        {
            lblReady.Content = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.Ready).Count();
            lblNotReady.Content = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).Count();
        }

        private async void cmdEcho_Click(object sender, RoutedEventArgs e)
        {
            var echos = raida.GetEchoTasks();
            txtProgress.AppendText("Starting Echo to RAIDA\n");
            txtProgress.AppendText("----------------------------------\n");

            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            MessageBox.Show("Finished Echo");
            lblReady.Content = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.Ready).Count();
            lblNotReady.Content = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).Count();

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                txtProgress.AppendText("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus+"\n");
                Debug.WriteLine("Node"+ i +" Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------\n");
        }

        public event EventHandler CoinDetected;

        protected virtual void OnCoinDetected(DetectEventArgs e)
        {
            CoinDetected?.Invoke(this, e);
        }

        private async void cmdDetect_Click(object sender, RoutedEventArgs e)
        {
            cmdDetect.IsEnabled = false;
            FS.LoadFileSystem();

            FS.DetectPreProcessing();

            foreach (var coin in FileSystem.importCoins)
            {
                // coin.GeneratePAN();
                coin.setAnsToPans();
                raida.coin = coin;
                var tasks = raida.GetDetectTasks(coin);
                await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                Debug.WriteLine("Coin No. - " + coin.sn + "Scanned");
               
                int countp = coin.response.Where(x => x.outcome == "pass").Count();
                int countf = coin.response.Where(x => x.outcome == "fail").Count();

                Debug.WriteLine(coin.sn + " Pass Count -" + countp);
                Debug.WriteLine(coin.sn + " Fail Count -" + countf);
                DetectEventArgs de = new DetectEventArgs(coin);
                OnCoinDetected(de);
                coin.doPostProcessing();


            }
            cmdDetect.IsEnabled = true;
            MessageBox.Show("Finished Detect");
        }

        private async void detect()
        {
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();

            var predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
            FileSystem.predetectCoins = predetectCoins;

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
                raida.coins = coins;

                var tasks = raida.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        //coin.pown = "";
                        for (int k = 0; k < CloudCoinCore.Config.NodeCount; k++)
                        {
                            coin.response[k] = raida.nodes[k].multiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;
                        updateLog("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "."+coin.pown);


                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        //bar1.Value = pge.MinorProgress;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raida.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raida.OnProgressChanged(pge);
                    FS.writeCoin(coins, FS.DetectedFolder);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }


            }
            pge.MinorProgress = 100;
            Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
            raida.OnProgressChanged(pge);
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);
            
            detectedCoins.ForEach(x => x.sortToFolder());
            //foreach (var coin in detectedCoins)
            //{
            //    //updateLog()
            //    Debug.WriteLine(coin.sn + "-" + coin.pown + "-" + coin.folder);
            //}
            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder == FS.CounterfeitFolder
                               select x).ToList();

            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            updateLog("Coin Detection finished.");
            updateLog("Total Passed Coins - " + passedCoins.Count() + "");
            updateLog("Total Failed Coins - " + failedCoins.Count() + "");


            FS.moveCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.writeCoin(failedCoins, FS.CounterfeitFolder, true);
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);

            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

            Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            updateLog("Detection Completed in - " + ts.TotalMilliseconds / 1000);

            App.Current.Dispatcher.Invoke(delegate
            {
                enableUI();
            });

        }

        
        private void cmdMultiDetect_Click(object sender, RoutedEventArgs e)
        {
            
            raida.ProgressChanged += Raida_ProgressChanged;
            cmdMultiDetect.IsEnabled = false;
            // Load All Coins of Workspace File System
            new Thread(delegate () {
                detect();
            }).Start();
            
           

        }

        private void Raida_ProgressChanged(object sender, EventArgs e)
        {
            ProgressChangedEventArgs pge = (ProgressChangedEventArgs)e;


            App.Current.Dispatcher.Invoke(delegate
            {
                bar1.Value = pge.MinorProgress;
                lblProgress.Content = "Progress Status : " + pge.MinorProgress + " %";
            });

        }

        private void txtProgress_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtProgress.ScrollToEnd();
        }

        private void updateLog(string logLine)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                txtProgress.AppendText(logLine + Environment.NewLine);
            });

        }

        private void cmdWorkspace_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string sMessageBoxText = "Do you want to Change CloudCoin Folder?";
                    string sCaption = "Change Directory";

                    MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                    MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                    switch (rsltMessageBox)
                    {
                        case MessageBoxResult.Yes:
                            /* ... */
                            // lblDirectory.Text = dialog.SelectedPath;
                            Properties.Settings.Default.WorkSpace = dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar;
                            Properties.Settings.Default.Save();
                            FS.RootPath = Properties.Settings.Default.WorkSpace;
                            FS.CreateFolderStructure();
                            //FileUtils fileUtils = FileUtils.GetInstance(Properties.Settings.Default.WorkSpace);
                            //fileUtils.CreateDirectoryStructure();
                            string[] fileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                            foreach (String fileName in fileNames)
                            {
                                if (fileName.Contains("jpeg"))
                                {
                                    try
                                    {
                                        string outputpath = Properties.Settings.Default.WorkSpace + "Templates" + System.IO.Path.DirectorySeparatorChar + fileName.Substring(22);
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
                            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                            Application.Current.Shutdown();
                            break;

                        case MessageBoxResult.No:
                            /* ... */
                            break;

                        case MessageBoxResult.Cancel:
                            /* ... */
                            break;
                    }
                }
            }


        }

        private void ShowCoins()
        {
            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);
            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);

            bankCoins.AddRange(frackedCoins);

            onesCount = (from x in bankCoins
                         where x.denomination == 1
                         select x).Count();
            fivesCount = (from x in bankCoins
                         where x.denomination == 5
                         select x).Count();
            qtrCount = (from x in bankCoins
                         where x.denomination == 25
                         select x).Count();
            hundredsCount = (from x in bankCoins
                         where x.denomination == 100
                         select x).Count();
            twoFiftiesCount = (from x in bankCoins
                         where x.denomination == 250
                         select x).Count();


            lblOnesCount.Content = onesCount;
            lblFivesCount.Content = fivesCount;
            lblQtrsCount.Content = qtrCount;
            lblHundredsCount.Content = hundredsCount;
            lblTwoFiftiesCount.Content = twoFiftiesCount;

            lblOnes.Content = onesCount;
            lblFives.Content = fivesCount * 5;
            lblQtrs.Content = qtrCount * 25;
            lblHundreds.Content = hundredsCount * 100;
            lblTwoFifties.Content = twoFiftiesCount * 250;
        }

        public void export(string backupDir)
        {


            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);

            //updateLog("  Your Bank Inventory:");
            int grandTotal = (bankTotals[0] + frackedTotals[0] + partialTotals[0]);
            // state how many 1, 5, 25, 100 and 250
            int exp_1 = bankTotals[1] + frackedTotals[1] + partialTotals[1];
            int exp_5 = bankTotals[2] + frackedTotals[2] + partialTotals[2];
            int exp_25 = bankTotals[3] + frackedTotals[3] + partialTotals[3];
            int exp_100 = bankTotals[4] + frackedTotals[4] + partialTotals[4];
            int exp_250 = bankTotals[5] + frackedTotals[5] + partialTotals[5];
            //Warn if too many coins

            if (exp_1 + exp_5 + exp_25 + exp_100 + exp_250 == 0)
            {
                Console.WriteLine("Can not export 0 coins");
                return;
            }

            //updateLog(Convert.ToString(bankTotals[1] + frackedTotals[1] + bankTotals[2] + frackedTotals[2] + bankTotals[3] + frackedTotals[3] + bankTotals[4] + frackedTotals[4] + bankTotals[5] + frackedTotals[5] + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]));

            if (((bankTotals[1] + frackedTotals[1]) + (bankTotals[2] + frackedTotals[2]) + (bankTotals[3] + frackedTotals[3]) + (bankTotals[4] + frackedTotals[4]) + (bankTotals[5] + frackedTotals[5]) + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]) > 1000)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("Warning: You have more than 1000 Notes in your bank. Stack files should not have more than 1000 Notes in them.");
                Console.Out.WriteLine("Do not export stack files with more than 1000 notes. .");
                //updateLog("Warning: You have more than 1000 Notes in your bank. Stack files should not have more than 1000 Notes in them.");
                //updateLog("Do not export stack files with more than 1000 notes. .");

                Console.ForegroundColor = ConsoleColor.White;
            }//end if they have more than 1000 coins

            Console.Out.WriteLine("  Do you want to export your CloudCoin to (1)jpgs or (2) stack (JSON) file?");
            Exporter exporter = new Exporter(FS);

            String tag = "backup";// reader.readString();
                                  //Console.Out.WriteLine(("Exporting to:" + exportFolder));

            exporter.writeJSONFile(exp_1, exp_5, exp_25, exp_100, exp_250, tag, 1, backupDir);


            //end if type jpge or stack




            //MessageBox.Show("Export completed.", "Cloudcoins", MessageBoxButtons.OK);
        }// end export One


        private void cmdShowFolders_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(FS.RootPath);
        }

        private void backup()
        {
            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);


            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    export(dialog.SelectedPath);
                    //copyFolders(dialog.SelectedPath);
                    MessageBox.Show("Backup completed successfully.");
                }
            }

        }

        private void cmdBackup_Click(object sender, RoutedEventArgs e)
        {
            // Load Bank, Fracked and Partial coins
            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);
            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);
            var partialCoins = FS.LoadFolderCoins(FS.PartialFolder);

            // Add them all up in a single list for backup
            bankCoins.AddRange(frackedCoins);
            bankCoins.AddRange(partialCoins);

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    //Save the Coins to File system
                    FS.WriteCoinsToFile(bankCoins,dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "backup" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower() );
                    MessageBox.Show("Backup completed successfully.");
                }
            }

            //backup();
        }
    }
}
