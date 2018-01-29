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
        FileSystem FS = new FileSystem();
        RAIDA raida;
        public MainWindow()
        {
            InitializeComponent();            
            Setup();           
        }

        public void Setup()
        {
            // Create the Folder Structure
            FS.CreateFolderStructure();
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            raida.FS = FS;
            CoinDetected += Raida_CoinDetected;
            //raida.Echo();
            FS.LoadFileSystem();
            FS.LoadFolderCoins(FS.RootPath + FS.CounterfeitFolder);
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
            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            MessageBox.Show("Finished Echo");
            lblReady.Content = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.Ready).Count();
            lblNotReady.Content = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).Count();

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                Debug.WriteLine("Node"+ i +" Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------");
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
            cmdMultiDetect.IsEnabled = false;
            FS.LoadFileSystem();

            FS.DetectPreProcessing();
            
            var predetectCoins = FS.LoadFolderCoins(FS.RootPath + FS.PreDetectFolder);
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if(predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            
            int coinCount = 0;
            for(int i =0;i < LotCount;i++)
            {
                var coins = predetectCoins.Skip(i*CloudCoinCore.Config.MultiDetectLoad).Take(200);
                raida.coins = coins;

                var tasks = raida.GetMultiDetectTasks(coins.ToArray(),CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach(var coin in coins)
                    {
                        for(int k=0;k<CloudCoinCore.Config.NodeCount;k++)
                        {
                            coin.response[k] = raida.nodes[k].multiResponse.responses[j];
                        }
                        j++;
                    }

                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                coinCount++;
            }
            for(int j = 0; j < CloudCoinCore.Config.NodeCount; j++)
            {
                Debug.WriteLine("Multi Detect Response for node " + j + "--" + raida.nodes[j].multiResponse.ToString());
            }
            Debug.WriteLine("Total Coins parsed- " + coinCount);
            cmdMultiDetect.IsEnabled = true;
        }
    }
}
