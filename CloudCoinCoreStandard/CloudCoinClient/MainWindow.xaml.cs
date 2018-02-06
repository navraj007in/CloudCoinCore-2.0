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
        public MainWindow()
        {
            InitializeComponent();            
            Setup();           
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

        private void detect(CloudCoin coin)
        {
            
        }

        private async void cmdMultiDetect_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            raida.ProgressChanged += Raida_ProgressChanged;
            cmdMultiDetect.IsEnabled = false;
            // Load All Coins of Workspace File System
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();
            
            var predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
            FileSystem.predetectCoins = predetectCoins;

            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if(predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            ProgressChangedEventArgs pge = new ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for(int i =0;i < LotCount;i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i*CloudCoinCore.Config.MultiDetectLoad).Take(200);
                raida.coins = coins;

                var tasks = raida.GetMultiDetectTasks(coins.ToArray(),CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach(var coin in coins)
                    {
                        //coin.pown = "";
                        for(int k=0;k<CloudCoinCore.Config.NodeCount;k++)
                        {
                            coin.response[k] = raida.nodes[k].multiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;

                        txtProgress.AppendText("No. " + CoinCount +". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult+".\n");
                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount ) * 100 / totalCoinCount;
                        bar1.Value = pge.MinorProgress;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raida.OnProgressChanged(pge);
                        j++;
                    }
                    //CoinCount+=j;
                    pge.MinorProgress = (CoinCount-1) * 100 / totalCoinCount;
                    //bar1.Value = pge.MinorProgress;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raida.OnProgressChanged(pge);
                    FS.writeCoin(coins, FS.DetectedFolder);
                    //FS.ProcessCoins(coins);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                
            }

            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);
            //foreach (var coin in detectedCoins)
            //{
            //    coin.sortToFolder();
            //}
            detectedCoins.ForEach(x => x.sortToFolder());
            foreach(var coin in detectedCoins)
            {
                Debug.WriteLine(coin.sn + "-" + coin.pown + "-" + coin.folder);
            }
            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder== FS.CounterfeitFolder
                               select x).ToList();

            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            txtProgress.AppendText("Coin Detection finished.\n");
            txtProgress.AppendText("Total Passed Coins - " + passedCoins.Count() +"\n");
            txtProgress.AppendText("Total Failed Coins - " + failedCoins.Count() + "\n") ;


            FS.moveCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.writeCoin(failedCoins, FS.CounterfeitFolder,true);
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);

            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

        cmdMultiDetect.IsEnabled = true;
            Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds/1000);
            txtProgress.AppendText("Detection Completed in - " + ts.TotalMilliseconds / 1000);

        }

        private void Raida_ProgressChanged(object sender, EventArgs e)
        {
            ProgressChangedEventArgs pge = (ProgressChangedEventArgs)e;

            //txtProgress.Text += "Progress Detected - " + pge.MinorProgress + "\n";
            lblProgress.Content = String.Format("Progress Status : %d %", pge.MinorProgress);
        }

        private void txtProgress_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtProgress.ScrollToEnd();
        }
    }
}
