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
using CloudCoinCore;
using CloudCoinCoreDirectory;
using CloudCoinClient.CoreClasses;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Microsoft.Win32;

namespace Celebrium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CoreVariables
        CloudCoinCore.RAIDA raidaCore = App.raida;
        CloudCoinClient.CoreClasses.FileSystem FS;
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
            InitializeComponent();

            raidaBar.Value = 90;
            SetupFolders();
            Form1 form1 = new Form1();
            //form1.Show();
            logger = new SimpleLogger(FS.LogsFolder + "logs" + DateTime.Now.ToString("yyyyMMdd").ToLower() + ".log", true);
            raidaCore.LoggerHandler += Raida_LogRecieved;

            fixer = new Frack_Fixer(FS, Config.milliSecondsToTimeOut);

            Echo();
            //showCoins();
            ShowCoins();
            new Thread(delegate () {
                Fix();
            }).Start();

            //resumeImport();

        }

        public async void Echo(bool resumeFix = false)
        {
            //DisableUI();
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
            raidaBar.Value = raidaCore.ReadyCount;
            this.Dispatcher.Invoke(() => {
                //updateLEDs(RAIDA_Status.failsEcho);
            });

            if (resumeFix)
                Task.Run(() => {
                    Fix();
                });
            //txtProgress.AppendText("----------------------------------\n");
        }

        public void Export(string tag, int exp_1, int exp_5, int exp_25, int exp_100, int exp_250)
        {
            //if (rdbJpeg.IsChecked == true)
            //    exportJpegStack = 1;
            //else
            //    exportJpegStack = 2;

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
            System.Threading.SpinWait.SpinUntil(() => !fixer.IsFixing);

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
            //txtTag.Text = "";
            //ShowCoins();

            //NSWorkspace.SharedWorkspace.SelectFile(FS.ExportFolder,
            //                                     FS.ExportFolder);
            Process.Start(FS.ExportFolder);

            //ShowCoins();
            Task.Run(() => {
                Fix();
            });

        }// end export One

        private void printLineDots()
        {
            updateLog("********************************************************************************");
        }

        private void updateLog(string logLine, bool writeUI = true)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                if (writeUI)
                    txtLogs.AppendText(logLine + Environment.NewLine);
                logger.Info(logLine);
            });

        }

        private void Fix()
        {
            fixer.continueExecution = true;
            fixer.IsFixing = true;
            fixer.FixAll();
            fixer.IsFixing = false;
        }

        private void Raida_LogRecieved(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
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

        private void cmdEcho_Click(object sender, RoutedEventArgs e)
        {
            Echo();
        }

        private void txtLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLogs.ScrollToEnd();
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
            //FS.MoveCoins(FileSystem.suspectCoins, FS.SuspectFolder, FS.PreDetectFolder);

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
            int existIndex = 0;
            List<CloudCoin> ccList = new List<CloudCoin>();
            foreach(var jpegCoin in newCoins)
            {
                string jpegExists = await CheckJpeg(jpegCoin);
                if(jpegExists=="yes")
                {
                    updateLog("Jpeg Found" + jpegCoin.sn);
                }
                else
                {
                    updateLog("Jpeg Not Found" + jpegCoin.sn);
                    ccList.Add(jpegCoin);

                }
                existIndex++;
            }

            MessageBox.Show(" Valid collectibles - "+ ccList.Count());
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

        private void ShowCoins()
        {
            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);
            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);
            var counterfeits = FS.LoadFolderCoins(FS.CounterfeitFolder);

            bankCoins.AddRange(frackedCoins);
            bankCoins.AddRange(counterfeits);

            App.Current.Dispatcher.Invoke(delegate
            {
                lvUsers.ItemsSource = bankCoins;
            });

        }

        private void EnableUI()
        {

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


        private void cmdImport_Click(object sender, RoutedEventArgs e)
        {
            raidaCore.ProgressChanged += Raida_ProgressChanged;
            cmdImport.IsEnabled = false;

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

        public async Task<string> CheckJpeg(CloudCoin cc)
        {
            string jpegExists = await Utils.GetHtmlFromURL(string.Format(Config.URL_JPEG_Exists, Config.NetworkNumber, cc.sn));
            //MessageBox.Show(jpegExists);

            if (jpegExists.Equals("true"))
            {
                getticket(cc);

            }

            return jpegExists;

        }

        public async void getticket(CloudCoin cc)
        {
            var ticketTask = raidaCore.nodes[0].GetTicketResponse(Config.NetworkNumber, cc.sn, cc.an[0], cc.denomination);
            string url = string.Format(raidaCore.nodes[3].FullUrl + Config.URL_GET_TICKET, Config.NetworkNumber, cc.sn, cc.an[3], cc.an[3], cc.denomination);
            string resp = await Utils.GetHtmlFromURL(url);
            if (resp.Contains("ticket"))
            {
                String[] KeyPairs = resp.Split(',');
                String message = KeyPairs[3];
                int startTicket = Utils.ordinalIndexOf(message, "\"", 3) + 2;
                int endTicket = Utils.ordinalIndexOf(message, "\"", 4) - startTicket;
                string resp2 = message.Substring(startTicket - 1, endTicket + 1); //This is the ticket or message
                url = string.Format(Config.URL_GET_IMAGE, Config.NetworkNumber, cc.sn, 3, resp2);
                string image = await Utils.GetHtmlFromURL(url);
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
                //imgCoin.Source = ByteImageConverter.ByteToImage(bytes);

                //GetImageResponse imgresp = JsonConvert.DeserializeObject<GetImageResponse>(image);

                //MessageBox.Show(message2);
                // MessageBox.Show(resp);

            }
            Console.WriteLine(ticketTask);
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
