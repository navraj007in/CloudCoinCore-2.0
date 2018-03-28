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
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using CloudCoinIE;
using Microsoft.Win32;
using CloudCoinCore;
using CloudCoinCE.CoreClasses;

namespace CloudCoinCE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public EventHandler RefreshCoins;

        #region CoreVariables
        CloudCoinCore.RAIDA raidaCore = App.raida;
        CloudCoinCE.CoreClasses.FileSystem FS ;
        //string RootPath;
        //FileSystem FS;
        //RAIDA raida;
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
        SimpleLogger logger = new SimpleLogger();
        Frack_Fixer fixer;
        
        #endregion
        public static String RootFolder = AppDomain.CurrentDomain.BaseDirectory;
        
              
        public MainWindow()
        {
            ShowDisclaimer();
            SetupFolders();

            InitializeComponent();
            //fileUtils.CreateDirectoryStructure();
            //loadJson();
            printWelcome();
            noteOne.NoteCount = "0";
            noteFive.NoteCount = "0";
            noteQtr.NoteCount = "0";
            noteHundred.NoteCount = "0";
            noteTwoFifty.NoteCount = "0";
            logger = new SimpleLogger(FS.LogsFolder + "logs" + DateTime.Now.ToString("yyyyMMdd").ToLower() + ".log", true);
            raidaCore.LoggerHandler += Raida_LogRecieved;

            fixer = new Frack_Fixer(FS, Config.milliSecondsToTimeOut);
            setLEDStatus(true);
            SetLEDFlashing(true);
            Echo();
            showCoins();
            ShowCoins();
            new Thread(delegate () {
                Fix();
            }).Start();

            resumeImport();
        }

        private void setLEDStatus(bool IsActive)
        {
            raida1.IsActive = IsActive;
            raida2.IsActive = IsActive;
            raida3.IsActive = IsActive;
            raida4.IsActive = IsActive;
            raida5.IsActive = IsActive;
            raida6.IsActive = IsActive;
            raida7.IsActive = IsActive;
            raida8.IsActive = IsActive;
            raida9.IsActive = IsActive;
            raida10.IsActive = IsActive;
            raida11.IsActive = IsActive;
            raida12.IsActive = IsActive;
            raida13.IsActive = IsActive;
            raida14.IsActive = IsActive;
            raida15.IsActive = IsActive;
            raida16.IsActive = IsActive;
            raida17.IsActive = IsActive;
            raida18.IsActive = IsActive;
            raida19.IsActive = IsActive;
            raida20.IsActive = IsActive;
            raida21.IsActive = IsActive;
            raida22.IsActive = IsActive;
            raida23.IsActive = IsActive;
            raida24.IsActive = IsActive;
            raida25.IsActive = IsActive;

        }
        public async void Echo(bool resumeFix = false)
        {
            DisableUI();
            var echos = raidaCore.GetEchoTasks();
            printLineDots();
            updateLog("\tEcho RAIDA");
            //updateLog("----------------------------------");
            printLineDots();
            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            //MessageBox.Show("Finished Echo");
            // lblReady.Content = raida.ReadyCount;
            // lblNotReady.Content = raida.NotReadyCount;

            for (int i = 0; i < raidaCore.nodes.Count(); i++)
            {

                //txtProgress.StringValue += "Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n";

                //txtProgress.AppendText("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n");
                Debug.WriteLine("Node" + i + " Status --" + raidaCore.nodes[i].RAIDANodeStatus);
            }

            //raidaLevel.IntValue = raidaCore.ReadyCount;

            updateLog("\tReady Nodes : " + Convert.ToString(raidaCore.ReadyCount) + "");
            updateLog("\tNot Ready Nodes : " + Convert.ToString(raidaCore.NotReadyCount) + "");
            printLineDots();

            this.Dispatcher.Invoke(() => {
                //updateLEDs(RAIDA_Status.failsEcho);
            });
            SetLEDFlashing(false);
            updateLEDs(null);
            EnableUI();
            if (resumeFix)
                Task.Run(() => {
                    Fix();
                });
            //txtProgress.AppendText("----------------------------------\n");
        }

        private void Fix()
        {
            fixer.continueExecution = true;
            fixer.IsFixing = true;
            fixer.FixAll();
            fixer.IsFixing = false;
        }

        public void DisableUI()
        {

        }

        public void EnableUI()
        {

        }

        private void Refresh(object sender, EventArgs e)
        {
            showCoins();
        }

        void Raida_LogRecieved(object sender, EventArgs e)
        {
            CloudCoinCore.ProgressChangedEventArgs pge = (CloudCoinCore.ProgressChangedEventArgs)e;
            //DetectEventArgs eargs = e;
            //updateLog(pge.MajorProgressMessage);
            logger.Info(pge.MajorProgressMessage);
            //Debug.WriteLine("Coin Detection Event Recieved - " + eargs.DetectedCoin.sn);
        }

        private void resumeImport()
        {

            int count = Directory.GetFiles(FS.SuspectFolder).Length;
            if (count > 0)
            {
                new Thread(() =>
                {

                    Thread.CurrentThread.IsBackground = true;

                   // echoRaida();

                    int totalRAIDABad = raidaCore.NotReadyCount;
                   
                    if (totalRAIDABad > 8)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLine("You do not have enough RAIDA to perform an import operation.");
                        Console.Out.WriteLine("Check to make sure your internet is working.");
                        Console.Out.WriteLine("Make sure no routers at your work are blocking access to the RAIDA.");
                        Console.Out.WriteLine("Try to Echo RAIDA and see if the status has changed.");
                        Console.ForegroundColor = ConsoleColor.White;

                        insufficientRAIDA();
                        return;
                    }
                    else
                    {

                    }
                        //import();

                    /* run your code here */
                }).Start();

            }
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

            raidaCore.ProgressChanged += Raida_ProgressChanged;
            cmdPown.IsEnabled = false;

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
                    EnableUI();
                    return;
                }
            }

            // Load All Coins of Workspace File System
            new Thread(delegate () {
                Detect();
            }).Start();



        }

        private void Raida_ProgressChanged(object sender, EventArgs e)
        {
            CloudCoinCore.ProgressChangedEventArgs pge = (CloudCoinCore.ProgressChangedEventArgs)e;


            App.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    //bar1.Value = pge.MinorProgress;
                    //lblProgress.Content = "Progress Status : " + pge.MinorProgress + " %";
                }
                catch (Exception ee)
                {

                }
            });

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

            IEnumerable<CloudCoin> predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
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
                FS.MoveFile(FS.PreDetectFolder + coin.FileName + ".stack", FS.TrashFolder + coin.FileName + ".stack", IFileSystem.FileMoveOptions.Replace);
            }

            predetectCoins = newCoins;
            //return;




            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if (predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            CloudCoinCore.ProgressChangedEventArgs pge = new CloudCoinCore.ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for (int i = 0; i < LotCount; i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i * CloudCoinCore.Config.MultiDetectLoad).Take(200);
                coins.ToList().ForEach(x => x.pown = "");
                raidaCore.coins = coins;

                var tasks = raidaCore.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
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
                            coin.response[k] = raidaCore.nodes[k].MultiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = coin.pown.Count(x => x == 'p');
                        coin.FailCount = coin.pown.Count(x => x == 'f');
                        CoinCount++;


                        updateLog("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "." + coin.pown);
                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raidaCore.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raidaCore.OnProgressChanged(pge);
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
            raidaCore.OnProgressChanged(pge);
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
                EnableUI();
                //ShowCoins();
            });

        }

        public void Export(string tag, int exp_1, int exp_5, int exp_25, int exp_100, int exp_250)
        {
            if (rdbJpeg.IsChecked == true)
                exportJpegStack = 1;
            else
                exportJpegStack = 2;

            fixer.continueExecution = false;
            if (fixer.IsFixing)
            {
                updateLog("Stopping Fix");
                Debug.WriteLine("Stopping Fix");
            }
            printLineDots();
            updateLog("Starting CloudCoin Export.");
            updateLog("\tPlease do not close the CloudCoin CE program until it is finished. " +
                      "\n\tOtherwise it may result in loss of CloudCoins.");
            printLineDots();
            SpinWait.SpinUntil(() => !fixer.IsFixing);

            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);

            //updateLog("  Your Bank Inventory:");
            int grandTotal = (bankTotals[0] + frackedTotals[0] + partialTotals[0]);
            int exportTotal = exp_1 + (exp_5 * 5) + (exp_25 * 25) + (exp_100 * 100) + (exp_250 * 250);

            if (exp_1 > onesCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 1.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_5 > fivesCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 5.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_25 > qtrCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 25.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_100 > hundredsCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 100.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_250 > twoFiftiesCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 250.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }


            updateLog("Exporting " + exportTotal + " CloudCoins from Bank.");
            printLineDots();

            //Warn if too many coins

            if (exp_1 + exp_5 + exp_25 + exp_100 + exp_250 == 0)
            {
                Console.WriteLine("Can not export 0 coins");
                updateLog("Can not export 0 coins");
                Task.Run(() => {
                    Fix();
                });
                return;
            }

            if (((bankTotals[1] + frackedTotals[1]) + (bankTotals[2] + frackedTotals[2]) + (bankTotals[3] + frackedTotals[3]) + (bankTotals[4] + frackedTotals[4]) + (bankTotals[5] + frackedTotals[5]) + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]) > 1000)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                printLineDots();
                updateLog("Warning!: You have more than 1000 Notes in your bank. \n\tStack files should not have more than 1000 Notes in them.");
                updateLog("\tDo not export stack files with more than 1000 notes. .");
                printLineDots();
                Console.Out.WriteLine("Warning: You have more than 1000 Notes in your bank. Stack files should not have more than 1000 Notes in them.");
                Console.Out.WriteLine("Do not export stack files with more than 1000 notes. .");

                Console.ForegroundColor = ConsoleColor.White;
            }//end if they have more than 1000 coins

            int file_type = 0; //reader.readInt(1, 2);

            Exporter exporter = new Exporter(FS);
            file_type = exportJpegStack;


            if (file_type == 1)
            {
                exporter.writeJPEGFiles(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            }
            else
            {
                exporter.writeJSONFile(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            }
            // end if type jpge or stack
            Console.Out.WriteLine("Exporting of the CloudCoins Completed.");
            updateLog("Export of the CloudCoins Completed.");
            updateLog("\tExported " + String.Format("{0:n0}", (exp_1 + exp_5 + exp_25 + exp_100 + exp_250))
                      + " coins in Total of " + String.Format("{0:n}", exportTotal) + " CC into " +
                      " " + FS.ExportFolder + " .");
            printLineDots();
            txtTag.Text = "";
            ShowCoins();

            //NSWorkspace.SharedWorkspace.SelectFile(FS.ExportFolder,
            //                                     FS.ExportFolder);
            Process.Start(FS.ExportFolder);

            ShowCoins();
            Task.Run(() => {
                Fix();
            });

        }// end export One



        private void insufficientRAIDA()
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                txtLogs.AppendText("You do not have enough RAIDA to perform an import operation.");
                txtLogs.AppendText("Check to make sure your internet is working.");
                txtLogs.AppendText("Make sure no routers at your work are blocking access to the RAIDA.");
                txtLogs.AppendText("Try to Echo RAIDA and see if the status has changed.");


                //cmdImport.IsEnabled = true;
                //cmdRestore.IsEnabled = true;
            });

        }
        private void updateLog(string logLine, bool writeUI = true)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                if(writeUI)
                    txtLogs.AppendText(logLine + Environment.NewLine);
                logger.Info(logLine);
            });

        }


        int[] bankTotals;
        int[] frackedTotals;
        int[] partialTotals;
        public int timeout = 10000;

        public void showCoins()
        {

        }// end show

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

            App.Current.Dispatcher.Invoke(delegate
            {
                noteOne.lblNoteCount.Content = Convert.ToString(onesCount);
                noteFive.lblNoteCount.Content = Convert.ToString(fivesCount);
                noteQtr.lblNoteCount.Content = Convert.ToString(qtrCount);
                noteHundred.lblNoteCount.Content = Convert.ToString(hundredsCount);
                noteTwoFifty.lblNoteCount.Content = Convert.ToString(twoFiftiesCount);

				updOne.Max = onesCount;
				updFive.Max = fivesCount;
                updQtr.Max = qtrCount;
                updHundred.Max = hundredsCount;
                updTwoFifty.Max = twoFiftiesCount;

                int totalAmount = onesCount + (fivesCount * 5) + (qtrCount * 25) + (hundredsCount * 100) + (twoFiftiesCount * 250);

                lblNotesTotal.Text = "₡" + totalAmount;
            });

        }
        private void setLabelText(Label lbl, string text)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                if (lbl != null)
                    lbl.Content = text;
            });

        }

        private void setLabelText(TextBlock lbl, string text)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                if (lbl != null)
                    lbl.Text = "₡ " + text;
            });

        }

        private void SetLEDFlashing(bool flashing)
        {
            raida1.Flashing = flashing;
            raida2.Flashing = flashing;
            raida3.Flashing = flashing;
            raida4.Flashing = flashing;
            raida5.Flashing = flashing;
            raida6.Flashing = flashing;
            raida7.Flashing = flashing;
            raida8.Flashing = flashing;
            raida9.Flashing = flashing;
            raida10.Flashing = flashing;
            raida11.Flashing = flashing;
            raida12.Flashing = flashing;
            raida13.Flashing = flashing;
            raida14.Flashing = flashing;
            raida15.Flashing = flashing;
            raida16.Flashing = flashing;
            raida17.Flashing = flashing;
            raida18.Flashing = flashing;
            raida19.Flashing = flashing;
            raida20.Flashing = flashing;
            raida21.Flashing = flashing;
            raida22.Flashing = flashing;
            raida23.Flashing = flashing;
            raida24.Flashing = flashing;
            raida25.Flashing = flashing;

        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        /*
         *Shows disclaimer for a first time Run 
         */
        private void ShowDisclaimer()
        {
            //Properties.Settings.Default["FirstRun"] = false;

            bool firstRun = (bool)Properties.Settings.Default["FirstRun"];
            if (firstRun == false)
            {
                //First application run
                //Update setting
                Properties.Settings.Default["FirstRun"] = true;
                //Save setting
                Properties.Settings.Default.Save();

                Disclaimer disclaimer = new Disclaimer();
                disclaimer.ShowDialog();

                //Create new instance of Dialog you want to show
                //FirstDialogForm fdf = new FirstDialogForm();
                //Show the dialog
                //fdf.ShowDialog();
            }
            else
            {
                //Not first time of running application.
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
           // echoRaida();
        }

        private void printWelcome()
        {
            updateLog("CloudCoin Consumers Edition");
            updateLog("Version " + DateTime.Now.ToShortDateString());
            updateLog("Used to Authenticate ,Store,Payout CloudCoins");
            updateLog("This Software is provided as is with all faults, defects and errors, and without warranty of any kind.Free from the CloudCoin Consortium.");
        }
        private void loadJson()
        {
            string fileName = @"nodes.json";
            TextRange range;
            FileStream fStream;
            if (File.Exists(fileName))
            {
                range = new TextRange(txtLogs.Document.ContentStart, txtLogs.Document.ContentEnd);
                fStream = new FileStream(fileName, FileMode.OpenOrCreate);
                range.Load(fStream, DataFormats.Text);
                fStream.Close();
            }
        }

        /*
         * Updates LED controls based on the status of failsEcho Array
         * Green for Echo Passed 
         * Red for Echo Failed
         */
        private void updateLEDs(bool[] failsEcho)
        {
            raida1.ColorOn = ! raidaCore.nodes[0].FailsEcho  ? Colors.Green : Colors.Red;
            raida2.ColorOn = !raidaCore.nodes[1].FailsEcho ? Colors.Green : Colors.Red;
            raida3.ColorOn = !raidaCore.nodes[2].FailsEcho ? Colors.Green : Colors.Red;
            raida4.ColorOn = !raidaCore.nodes[3].FailsEcho ? Colors.Green : Colors.Red;
            raida5.ColorOn = !raidaCore.nodes[4].FailsEcho ? Colors.Green : Colors.Red;
            raida6.ColorOn = !raidaCore.nodes[5].FailsEcho ? Colors.Green : Colors.Red;
            raida7.ColorOn = !raidaCore.nodes[6].FailsEcho ? Colors.Green : Colors.Red;
            raida8.ColorOn = !raidaCore.nodes[7].FailsEcho ? Colors.Green : Colors.Red;
            raida9.ColorOn = !raidaCore.nodes[8].FailsEcho ? Colors.Green : Colors.Red;
            raida10.ColorOn = !raidaCore.nodes[9].FailsEcho ? Colors.Green : Colors.Red;
            raida11.ColorOn = !raidaCore.nodes[10].FailsEcho ? Colors.Green : Colors.Red;
            raida12.ColorOn = !raidaCore.nodes[11].FailsEcho ? Colors.Green : Colors.Red;
            raida13.ColorOn = !raidaCore.nodes[12].FailsEcho ? Colors.Green : Colors.Red;
            raida14.ColorOn = !raidaCore.nodes[13].FailsEcho ? Colors.Green : Colors.Red;
            raida15.ColorOn = !raidaCore.nodes[14].FailsEcho ? Colors.Green : Colors.Red;
            raida16.ColorOn = !raidaCore.nodes[15].FailsEcho ? Colors.Green : Colors.Red;
            raida17.ColorOn = !raidaCore.nodes[16].FailsEcho ? Colors.Green : Colors.Red;
            raida18.ColorOn = !raidaCore.nodes[17].FailsEcho ? Colors.Green : Colors.Red;
            raida19.ColorOn = !raidaCore.nodes[18].FailsEcho ? Colors.Green : Colors.Red;
            raida20.ColorOn = !raidaCore.nodes[19].FailsEcho ? Colors.Green : Colors.Red;
            raida21.ColorOn = !raidaCore.nodes[20].FailsEcho ? Colors.Green : Colors.Red;
            raida22.ColorOn = !raidaCore.nodes[21].FailsEcho ? Colors.Green : Colors.Red;
            raida23.ColorOn = !raidaCore.nodes[22].FailsEcho ? Colors.Green : Colors.Red;
            raida24.ColorOn = !raidaCore.nodes[23].FailsEcho ? Colors.Green : Colors.Red;
            raida25.ColorOn = !raidaCore.nodes[24].FailsEcho ? Colors.Green : Colors.Red;


        }


        public void export(string backupDir)
        {






            //MessageBox.Show("Export completed.", "Cloudcoins", MessageBoxButtons.OK);
        }// end export One


        private void cmdBackup_Click(object sender, RoutedEventArgs e)
        {
            backup();
        }
        private void backup()
        {
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
                    string backupFileName = "backup" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower();
                    FS.WriteCoinsToFile(bankCoins, dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar +
                                        backupFileName);
                    printLineDots();
                    updateLog("Backup file " + backupFileName + " saved to " + dialog.SelectedPath + " .");
                    printLineDots();

                    MessageBox.Show("Backup completed successfully.");
                }
            }

        }

        private void cmdShowFolders_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(MainWindow.RootFolder);
        }

        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetLEDFlashing(true);
            Echo();
            showCoins();
        }

        public void SetupFolders()
        {
            RootFolder = getWorkspace();
            FS = new FileSystem(RootFolder);

            // Create the Folder Structure
            FS.CreateFolderStructure();
            FS.CopyTemplates();
            // Populate RAIDA Nodes
            raidaCore = CloudCoinCore.RAIDA.GetInstance();
            raidaCore.FS = FS;
            //CoinDetected += Raida_CoinDetected;
            //raida.Echo();

            FS.LoadFileSystem();

            //fileUtils = FileUtils.GetInstance(MainWindow.rootFolder);


        }
        public string getWorkspace()
        {
            string workspace = "";
            if (Properties.Settings.Default.WorkSpace != null && Properties.Settings.Default.WorkSpace.Length > 0)
                workspace = Properties.Settings.Default.WorkSpace;
            else
                workspace = AppDomain.CurrentDomain.BaseDirectory;
            Properties.Settings.Default.WorkSpace = workspace;
            return workspace;
        }

        private void cmdChangeWorkspace_Click(object sender, RoutedEventArgs e)
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

                            string[] fileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                            foreach (String fileName in fileNames)
                            {
                                if (fileName.Contains("jpeg") || fileName.Contains("jpg"))
                                {
                                    try
                                    {
                                        string outputpath = Properties.Settings.Default.WorkSpace + "Templates" + System.IO.Path.DirectorySeparatorChar + fileName.Substring(22);
                                        updateLog(outputpath);
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

        private void cmdPown_Click(object sender, RoutedEventArgs e)
        {
            int totalRAIDABad = raidaCore.NotReadyCount;

            if (totalRAIDABad > 8)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("You do not have enough RAIDA to perform an import operation.");
                Console.Out.WriteLine("Check to make sure your internet is working.");
                Console.Out.WriteLine("Make sure no routers at your work are blocking access to the RAIDA.");
                Console.Out.WriteLine("Try to Echo RAIDA and see if the status has changed.");
                Console.ForegroundColor = ConsoleColor.White;

                insufficientRAIDA();

                return;
            }

            int count = Directory.GetFiles(FS.ImportFolder).Length;
            if (count == 0)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Cloudcoins (*.stack, *.jpg,*.jpeg)|*.stack;*.jpg;*.jpeg|Stack files (*.stack)|*.stack|Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                //openFileDialog.InitialDirectory = FS.ImportFolder;
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
                    return;
            }

            //cmdImport.IsEnabled = false;
            //cmdRestore.IsEnabled = false;
            //progressBar.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //import();
                Detect();
                /* run your code here */
            }).Start();
        }

        public async void Detect()
        {
            updateLog("Starting Detect..");
            printLineDots();
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();
            FS.MoveCoins(FileSystem.suspectCoins, FS.SuspectFolder, FS.PreDetectFolder);

            IEnumerable<CloudCoin> predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
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
                FS.MoveFile(FS.PreDetectFolder + coin.FileName + ".stack", FS.TrashFolder + coin.FileName + ".stack", IFileSystem.FileMoveOptions.Replace);
            }

            predetectCoins = newCoins;

            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if (predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            CloudCoinCore.ProgressChangedEventArgs pge = new CloudCoinCore.ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for (int i = 0; i < LotCount; i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i * CloudCoinCore.Config.MultiDetectLoad).Take(200);
                coins.ToList().ForEach(x => x.pown = "");
                raidaCore.coins = coins;
                if (i == LotCount - 1)
                {
                    updateLog("\tDetecting Coins " + (i * CloudCoinCore.Config.MultiDetectLoad + 1) +
                              " to " + totalCoinCount);
                }
                else
                    updateLog("\tDetecting Coins " + (i * CloudCoinCore.Config.MultiDetectLoad + 1) +
                          " to " + (i + 1) * CloudCoinCore.Config.MultiDetectLoad);
                var tasks = raidaCore.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    string requestFileName = coins.Count() + ".CloudCoins." + Utils.RandomString(16).ToLower() +
                                                                            DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        coin.pown = "";
                        for (int k = 0; k < CloudCoinCore.Config.NodeCount; k++)
                        {
                            coin.response[k] = raidaCore.nodes[k].MultiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;
                        updateLog("No. " + CoinCount + ". Coin Detected. S. No. : " + coin.sn + ". Pass Count : " +
                                  coin.PassCount + ". Fail Count  : " + coin.FailCount + ". Result : " +
                                  coin.DetectionResult + "." + coin.pown + ".", false);


                        Debug.WriteLine("Coin Detected. S. No. - " + coin.sn + ". Pass Count : " + coin.PassCount + ". Fail Count  : " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        //bar1.Value = pge.MinorProgress;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raidaCore.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raidaCore.OnProgressChanged(pge);
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
            raidaCore.OnProgressChanged(pge);
            updateLog("\tDetection finished.");
            printLineDots();
            updateLog("Starting Grading Coins..");
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);
            //detectedCoins.ForEach(x => x.pown = "pppppppdppppppppppppppppp");
            detectedCoins.ForEach(x => x.SetAnsToPansIfPassed());
            detectedCoins.ForEach(x => x.CalculateHP());
            detectedCoins.ForEach(x => x.CalcExpirationDate());

            detectedCoins.ForEach(x => x.SortToFolder());
            updateLog("Grading Coins Completed.");
            //foreach (var coin in detectedCoins)
            //{
            //    //updateLog()
            //    Debug.WriteLine(coin.sn + "-" + coin.pown + "-" + coin.folder);
            //}
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
            updateLog("Detection and Import of the CloudCoins completed.");
            updateLog("\tTotal Passed Coins : " + (passedCoins.Count() + frackedCoins.Count()) + "");
            updateLog("\tTotal Counterfeit Coins : " + failedCoins.Count() + "");
            updateLog("\tTotal Lost Coins : " + lostCoins.Count() + "");
            updateLog("\tTotal Suspect Coins : " + suspectCoins.Count() + "");
            updateLog("\tTotal Skipped Coins : " + existingCoins.Count() + "");
            updateLog("\tTotal Dangerous Coins : " + dangerousCoins.Count() + "");
            printLineDots();

            // Move Coins to their respective folders after sort
            FS.TransferCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.TransferCoins(frackedCoins, FS.DetectedFolder, FS.FrackedFolder);
            if (failedCoins.Count > 0)
            {
                FS.WriteCoin(failedCoins, FS.CounterfeitFolder, true);
            }
            FS.MoveCoins(lostCoins, FS.DetectedFolder, FS.LostFolder);
            FS.TransferCoins(suspectCoins, FS.DetectedFolder, FS.SuspectFolder);
            FS.MoveCoins(dangerousCoins, FS.DetectedFolder, FS.DangerousFolder);

            // Clean up Detected Folder
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);
            FS.RemoveCoins(lostCoins, FS.DetectedFolder);
            FS.RemoveCoins(suspectCoins, FS.DetectedFolder);

            FS.MoveImportedFiles();

            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

            Debug.WriteLine("Detection Completed in : " + ts.TotalMilliseconds / 1000);
            updateLog("Detection Completed in : " + ts.TotalMilliseconds / 1000);
            printLineDots();
            //printLineDots();
            EnableUI();
            //ShowCoins();
            Task.Run(() => {
                Fix();
            });
            FS.LoadFileSystem();
            ShowCoins();
        }

        private void printLineDots()
        {
            updateLog("********************************************************************************");
        }

        public void export()
        {
            if (rdbJpeg.IsChecked == true)
                exportJpegStack = 1;
            else
                exportJpegStack = 2;

            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);

            //updateLog("  Your Bank Inventory:");
            int grandTotal = (bankTotals[0] + frackedTotals[0] + partialTotals[0]);
            // state how many 1, 5, 25, 100 and 250
            int exp_1 = Convert.ToInt16(updOne.Value);
            int exp_5 = Convert.ToInt16(updFive.Value);
            int exp_25 = Convert.ToInt16(updQtr.Value);
            int exp_100 = Convert.ToInt16(updHundred.Value);
            int exp_250 = Convert.ToInt16(updTwoFifty.Value);
            //Warn if too many coins

            if (exp_1 + exp_5 + exp_25 + exp_100 + exp_250 == 0)
            {
                MessageBox.Show("Can not export 0 Coins.","Export CloudCoins");
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
            int file_type = 0; //reader.readInt(1, 2);





            //Exporter exporter = new Exporter(fileUtils);
            //exporter.OnUpdateStatus += Exporter_OnUpdateStatus; ;
            //file_type = exportJpegStack;

            //String tag = txtTag.Text;// reader.readString();
            ////Console.Out.WriteLine(("Exporting to:" + exportFolder));

            //if (file_type == 1)
            //{
            //    exporter.writeJPEGFiles(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            //    // stringToFile( json, "test.txt");
            //}
            //else
            //{
            //    exporter.writeJSONFile(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            //}


            // end if type jpge or stack

            RefreshCoins?.Invoke(this, new EventArgs());
            //updateLog("Exporting CloudCoins Completed.");
            showCoins();
            Process.Start(FS.ExportFolder);
            cmdExport.Content = "₡0";
            //MessageBox.Show("Export completed.", "Cloudcoins", MessageBoxButtons.OK);
        }// end export One

        
        private void txtLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLogs.ScrollToEnd();
        }

        private void updOne_OnExportChanged(object sender, EventArgs e)
        {
            updateExportTotal();
        }
        int exportTotal = 0;
        public void updateExportTotal()
        {
            exportTotal = updOne.val + (updFive.val *5) + (updQtr.val*25) + (updHundred.val*100) 
                + (updTwoFifty.val*250);
			cmdExport.Content = exportTotal.ToString();

			//lblExportTotal.Text = exportTotal.ToString();
            cmdExport.Content = "₡ " + exportTotal.ToString();
            
        }

        private void cmdExport_Click(object sender, RoutedEventArgs e)
        {
            string sMessageBoxText = "Are you sure you want to export CloudCoins?";
            string sCaption = "Export CloudCoins";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    int exp_1 = Convert.ToInt16(updOne.Value);
                    int exp_5 = Convert.ToInt16(updFive.Value);
                    int exp_25 = Convert.ToInt16(updQtr.Value);
                    int exp_100 = Convert.ToInt16(updHundred.Value);
                    int exp_250 = Convert.ToInt16(updTwoFifty.Value);

                    //Task.Run(() => {
                        Export(txtTag.Text, exp_1,
                               exp_5,
                               exp_25,
                               exp_100,
                               exp_250);
                   // });

                    break;
            }
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
                            FileSystem fileUtils = new FileSystem(Properties.Settings.Default.WorkSpace);
                            fileUtils.CreateDirectories();
                            //FS.CreateDirectories();
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
    }
    public static class MyExtensions
    {
        public static string ToCustomString(this TimeSpan span)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
        }
    }

}
