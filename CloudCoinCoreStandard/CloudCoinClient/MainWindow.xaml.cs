using System;
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
using ZXing;
using ZXing.Common;
using SharpPdf417;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;


namespace CloudCoinClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string RootPath;
        FileSystem FS;
        FileSystem fileUtils;
        RAIDA raida;
        int onesCount = 0;
        int fivesCount = 0;
        int qtrCount = 0;
        int hundredsCount = 0;
        int twoFiftiesCount = 0;

        public static int exportOnes = 0;
        public static int exportFives = 0;
        public static int exportTens = 0;
        public static int exportQtrs = 0;
        public static int exportHundreds = 0;
        public static int exportTwoFifties = 0;
        public static int exportJpegStack = 2;
        public static string exportTag = "";

        public MainWindow()
        {
            InitializeComponent();
            //Disable UI controls before  File system is loaded in memory.
            App.Current.Dispatcher.Invoke(delegate
            {
                disableUI();
            });
            // Load the Coins File system in Memory
            new Thread(delegate () {
                updateLog("Loading File system\n");
                Setup();
            }).Start();
        }

        public void RefreshScreen()
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                disableUI();
            });
            // Load the Coins File system in Memory
            new Thread(delegate () {
                updateLog("Loading File system\n");
                FS.LoadFileSystem();
                App.Current.Dispatcher.Invoke(delegate
                {
                    disableUI();
                });
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

        private void fix()
        {
            Frack_Fixer fixer = new Frack_Fixer(fileUtils, CloudCoinCore.Config.milliSecondsToTimeOut);
            fixer.fixAll();
            //stopwatch.Stop();
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
            fileUtils = FS;

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

            var lostresult = MessageBox.Show("We found some lost coins in your file System. Do you want to detect them again?", "Lost Coins found!", MessageBoxButton.YesNo);

            if(lostresult == MessageBoxResult.Yes)
            {
                FS.MoveCoins(FileSystem.lostCoins, FS.LostFolder, FS.PreDetectFolder);
                FileSystem.predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
                FileSystem.lostCoins = FS.LoadFolderCoins(FS.LostFolder);
            }

            if (FileSystem.predetectCoins.Count() + FileSystem.suspectCoins.Count() > 0)
            {
                var result = MessageBox.Show("There are undetected coins in your file System. Do you want to resume?", "Un Imported Coins found!", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    FS.MoveCoins(FileSystem.predetectCoins, FS.SuspectFolder, FS.PreDetectFolder);
                    detect();
                }
                else
                {

                }
            }
            else
            {

            }

            App.Current.Dispatcher.Invoke(delegate
            {
                ShowCoins();
                enableUI();
                updateLog("File system Loaded");
            });


        }

        private void Raida_CoinDetected(object sender, EventArgs e)
        {
            DetectEventArgs eargs = (DetectEventArgs)e;
            Debug.WriteLine("Coin Detection Event Recieved - " +eargs.DetectedCoin.sn);
        }

        private void cmdShow_Click(object sender, RoutedEventArgs e)
        {
            lblReady.Content = raida.ReadyCount;
            lblNotReady.Content = raida.NotReadyCount;
        }

        private async void cmdEcho_Click(object sender, RoutedEventArgs e)
        {
            var echos = raida.GetEchoTasks();
            txtProgress.AppendText("Starting Echo to RAIDA\n");
            txtProgress.AppendText("----------------------------------\n");

            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            //MessageBox.Show("Finished Echo");
            lblReady.Content = raida.ReadyCount;
            lblNotReady.Content = raida.NotReadyCount;

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                txtProgress.AppendText("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus+"\n");
                Debug.WriteLine("Node"+ i +" Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------\n");
            txtProgress.AppendText("----------------------------------\n");
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
            updateLog("Starting Multi Detect..");
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();

            IEnumerable<CloudCoin>  predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
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
                updateLog("Found existing coin :" + coin.sn + ". Skipping.");
                FS.MoveFile(FS.PreDetectFolder + coin.FileName + ".stack", FS.TrashFolder + coin.FileName + ".stack",IFileSystem.FileMoveOptions.Replace);
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
                        coin.PassCount = coin.pown.Count(x => x == 'p');
                        coin.FailCount = coin.pown.Count(x => x == 'f');
                        CoinCount++;


                        updateLog("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "."+coin.pown);
                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raida.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raida.OnProgressChanged(pge);
                    FS.WriteCoin(coins, FS.DetectedFolder);
                    FS.RemoveCoins(coins, FS.PreDetectFolder);
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
            
            detectedCoins.ForEach(x => x.SetAnsToPansIfPassed());
            detectedCoins.ForEach(x => x.CalculateHP());
            detectedCoins.ForEach(x => x.CalcExpirationDate());

            // Apply Sort to Folder to all detected coins at once.
            updateLog("Starting Sort.....");
            detectedCoins.ForEach(x => x.SortToFolder());
            updateLog("Ended Sort........");

            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var frackedCoins = (from x in detectedCoins
                               where x.folder == FS.FrackedFolder
                               select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder == FS.CounterfeitFolder
                               select x).ToList();
            var lostCoins = (from x in detectedCoins
                               where x.folder == FS.LostFolder
                               select x).ToList();
            var suspectCoins = (from x in detectedCoins
                             where x.folder == FS.SuspectFolder
                             select x).ToList();
            var dangerousCoins = (from x in detectedCoins
                                where x.folder == FS.DangerousFolder
                                select x).ToList();
            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            updateLog("Coin Detection finished.");
            updateLog("Total Passed Coins : " + (passedCoins.Count() + frackedCoins.Count()) + "");
            updateLog("Total Failed Coins : " + failedCoins.Count() + "");
            updateLog("Total Lost Coins : " + lostCoins.Count() + "");
            updateLog("Total Suspect Coins : " + suspectCoins.Count() + "");
            updateLog("Total Skipped Coins : " + existingCoins.Count() + "");
            updateLog("Total Dangerous Coins : " + dangerousCoins.Count() + "");

            // Move Coins to their respective folders after sort
            FS.MoveCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.MoveCoins(frackedCoins, FS.DetectedFolder, FS.FrackedFolder);
            FS.WriteCoin(failedCoins, FS.CounterfeitFolder, true);
            FS.MoveCoins(lostCoins, FS.DetectedFolder, FS.LostFolder);
            FS.MoveCoins(suspectCoins, FS.DetectedFolder, FS.SuspectFolder);
            FS.MoveCoins(dangerousCoins, FS.DetectedFolder, FS.DangerousFolder);

            // Clean up Detected Folder
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);
            FS.RemoveCoins(lostCoins, FS.DetectedFolder);
            FS.RemoveCoins(suspectCoins, FS.DetectedFolder);

            FS.MoveImportedFiles();
            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

            Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            updateLog("Detection Completed in - " + ts.TotalMilliseconds / 1000);

            
            App.Current.Dispatcher.Invoke(delegate
            {
                FS.LoadFileSystem();
                enableUI();
                ShowCoins();
            });

        }

        private bool PickFiles()
        {
            
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Cloudcoins (*.stack, *.jpg,*.jpeg)|*.stack;*.jpg;*.jpeg|Stack files (*.stack)|*.stack|Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = FS.RootPath;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        try
                        {
                            if (!File.Exists(FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename)))
                                File.Move(filename, FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename));
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
                                        File.Delete(FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename));
                                        File.Move(filename, FS.ImportFolder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(filename));
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            updateLog(ex.Message);
                        }
                    }
                }
                else
                    return false;
            }
            return true;
        }
        private void cmdMultiDetect_Click(object sender, RoutedEventArgs e)
        {
            
            raida.ProgressChanged += Raida_ProgressChanged;
            cmdMultiDetect.IsEnabled = false;

            // Check if There are already files in import folder
            // if not then show file picker dialog

            var files = Directory
               .GetFiles(FS.ImportFolder)
               .Where(file => CloudCoinCore.Config.allowedExtensions.Any(file.ToLower().EndsWith))
               .ToList();

            int filesCount = Directory.GetFiles(FS.ImportFolder).Length;
            if (files.Count() == 0)
            {
                bool pickResult = PickFiles();
                if (!pickResult)
                {
                    enableUI();
                    return;
                }
            }

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
                try
                {
                    bar1.Value = pge.MinorProgress;
                    lblProgress.Content = "Progress Status : " + pge.MinorProgress + " %";
                }
                catch (Exception ee)
                {

                }
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

            lblNotesTotal.Text = "₡" + Convert.ToString(onesCount + fivesCount + qtrCount + hundredsCount + twoFiftiesCount);
        }

        public void export()
        {
            if (rdbJpeg.IsChecked == true)
                exportJpegStack = 1;
            else
                exportJpegStack = 2;

            //Banker bank = new Banker(fileUtils);
            //int[] bankTotals = bank.countCoins(fileUtils.bankFolder);
            //int[] frackedTotals = bank.countCoins(fileUtils.frackedFolder);
            //int[] partialTotals = bank.countCoins(fileUtils.partialFolder);

            ////updateLog("  Your Bank Inventory:");
            //int grandTotal = (bankTotals[0] + frackedTotals[0] + partialTotals[0]);
            // state how many 1, 5, 25, 100 and 250
            int exp_1 = Convert.ToInt16(txtExportOne.Text);
            int exp_5 = Convert.ToInt16(txtExportFive.Text);
            int exp_25 = Convert.ToInt16(txtExportQtr.Text);
            int exp_100 = Convert.ToInt16(txtExportHundred.Text);
            int exp_250 = Convert.ToInt16(txtExportTwoFifties.Text);

            ////Warn if too many coins

            if (exp_1 + exp_5 + exp_25 + exp_100 + exp_250 == 0)
            {
                MessageBox.Show("Can not export 0 Coins.", "Export CloudCoins");
                updateLog("Can not export 0 coins\n");
                return;
            }

            int file_type = 0; 

            Exporter exporter = new Exporter(FS);
            //exporter.OnUpdateStatus += Exporter_OnUpdateStatus; ;
            file_type = exportJpegStack;

            String tag = txtTag.Text;// reader.readString();
            ////Console.Out.WriteLine(("Exporting to:" + exportFolder));

            if (file_type == 1)
            {
                exporter.writeJPEGFiles(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
                // stringToFile( json, "test.txt");
            }
            else
            {
                exporter.writeJSONFile(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            }

            ShowCoins();
            //// end if type jpge or stack

            //RefreshCoins?.Invoke(this, new EventArgs());
            updateLog("Exporting CloudCoins Completed.");
            //showCoins();
            Process.Start(FS.ExportFolder);
            //cmdExport.Content = "₡0";
            System.Windows.Forms.MessageBox.Show("Export completed.", "Cloudcoins", System.Windows.Forms.MessageBoxButtons.OK);
            FS.LoadFileSystem();
            //RefreshScreen();
        }// end export One

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

        private void cmdExport_Click(object sender, RoutedEventArgs e)
        {
            export();
        }

        private void cmdListSerials_Click(object sender, RoutedEventArgs e)
        {
            // Show Dialog to Select the Folder for which you want to list Serial numbers of CloudCoins
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = FS.RootPath;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Load the Coins in the Selected folders
                    var csv = new StringBuilder();
                    var coins = FS.LoadFolderCoins(dialog.SelectedPath).OrderBy(x=>x.sn);

                    var headerLine = string.Format("sn,denomination,nn,");
                    string headeranstring = "";
                    for (int i = 0; i < CloudCoinCore.Config.NodeCount; i++)
                    {
                        headeranstring += "an" + (i+1) + ",";
                    }

                    // Write the Header Record
                    csv.AppendLine(headerLine + headeranstring);

                    // Write the Coin Serial Numbers
                    foreach (var coin in coins)
                    {
                        string anstring = "";
                        for(int i=0;i<CloudCoinCore.Config.NodeCount;i++)
                        {
                            anstring += coin.an[i] + ",";
                        }
                        var newLine = string.Format("{0},{1},{2},{3}", coin.sn, coin.denomination,coin.nn,anstring);
                        csv.AppendLine(newLine);

                    }
                    File.WriteAllText(dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "coinserails" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower() + ".csv", csv.ToString());
                    Process.Start(dialog.SelectedPath);

                }
            }

        }

        private byte[] GenerateQRCodeZXing(string data)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE
                //Options = new EncodingOptions { Width = 200, Height = 50 } //optional
            };
            var imgBitmap = writer.Write(data);
            using (var stream = new MemoryStream())
            {
                imgBitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private byte[] GenerateBarCodeZXing(string data)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.PDF_417,
                Options = new EncodingOptions { Width = 200, Height = 50 } //optional
            };
            var imgBitmap = writer.Write(data);
            using (var stream = new MemoryStream())
            {
                imgBitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }


        private void cmdExportBarCode_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = FS.BankFolder;
            openFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";
            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "jpeg files (*.jpeg)|*.jpeg|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var bankCoins = FS.LoadCoin(openFileDialog.FileName);
                    var barcode = GenerateBarCodeZXing(bankCoins.GetCSV());
                    System.Drawing.Image x = (Bitmap)((new ImageConverter()).ConvertFrom(barcode));

                    x.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                    Process.Start(saveFileDialog.FileName);

                }

            }

        }

        private void cmdExportQRCode_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = FS.BankFolder;
            openFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";
            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "jpeg files (*.jpeg)|*.jpeg|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var bankCoins = FS.LoadCoin(openFileDialog.FileName);
                    var barcode = GenerateQRCodeZXing(bankCoins.GetCSV());
                    System.Drawing.Image x = (Bitmap)((new ImageConverter()).ConvertFrom(barcode));

                    x.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                    Process.Start(saveFileDialog.FileName);

                }

            }
        }

        private void cmdStackToCSV_Click(object sender, RoutedEventArgs e)
        {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                //openFileDialog.InitialDirectory = FS.BankFolder;
                openFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";
                //openFileDialog.ShowDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == true)
                    {

                    File.WriteAllText(saveFileDialog.FileName, Utils.CoinsToCSV(FS.LoadCoins(openFileDialog.FileName)).ToString());
                    Process.Start(saveFileDialog.FileName);
                    }
                }
            }
        private void cmdStackToQR_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = FS.BankFolder;
            openFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";
           

            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var coins = FS.LoadCoins(openFileDialog.FileName);
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {

                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        //Save the Coins to File system
                        string path = dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar;
                        foreach (CloudCoin coin in coins)
                        {
                            var barcode = GenerateQRCodeZXing(coin.GetCSV());
                            System.Drawing.Image x = (Bitmap)((new ImageConverter()).ConvertFrom(barcode));

                            x.Save(path + coin.FileName + "qrcode.jpg", ImageFormat.Jpeg);
                        }
                        MessageBox.Show("QR Codes Generated successfully.");
                        Process.Start(path);
                    }
                }

            }
        }

        private void cmdStackToBarCode_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = FS.BankFolder;
            openFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";


            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var coins = FS.LoadCoins(openFileDialog.FileName);
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {

                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        //Save the Coins to File system
                        string path = dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar;
                        foreach (CloudCoin coin in coins)
                        {
                            var barcode = GenerateBarCodeZXing(coin.GetCSV());
                            System.Drawing.Image x = (Bitmap)((new ImageConverter()).ConvertFrom(barcode));

                            x.Save(path + coin.FileName + "barcode.jpg", ImageFormat.Jpeg);
                        }
                        MessageBox.Show("Bar Codes Generated successfully.");
                        Process.Start(path);
                    }
                }

            }
        }

        private void cmdCSVToStack_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = FS.BankFolder;
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            var coins = new List<CloudCoin>();

            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string csvFile = openFileDialog.FileName;
                string[] lines = File.ReadAllLines(csvFile);
                int i = 0;
                foreach(string line in lines)
                {
                    if(i>0)
                    {
                        var coin = CloudCoin.FromCSV(line);
                        if (coin != null)
                            coins.Add(coin);
                    }
                    i++;
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    FS.WriteToFile(coins, saveFileDialog.FileName);
                    Process.Start(saveFileDialog.FileName);
                }
                    
            }
        }

        private void RecoverCoins(IEnumerable<CloudCoin> coins, bool UsePANS)
        {
            foreach (var coin in coins)
            {
                for (int i = 0; i < coin.an.Count; i++)
                {
                    Debug.WriteLine("AN" + i + ": " + coin.an[i]);
                    if (coin.pan[i] != "" && coin.pan[i] != null && UsePANS)
                        coin.an[i] = coin.pan[i];
                }
                for (int i = 0; i < coin.an.Count; i++)
                {
                    Debug.WriteLine("PAN" + i + ": " + coin.pan[i]);
                }

            }
            FS.WriteCoinsToFile(coins, FS.ImportFolder + coins.Count() + ".CloudCoins.recovery." + Utils.RandomString(8).ToLower() + "");
        }
        private void cmdRecover_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = @FS.BankFolder;
            openFileDialog.Filter = "stack files (*.stack)|*.stack|All files (*.*)|*.*";
            //var coins = new List<CloudCoin>();

            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var coins = FS.LoadCoins(openFileDialog.FileName);
                if(coins!=null)
                {
                    RecoverCoins(coins, true);
                    detect();
                }
            }
        }
    }
}
