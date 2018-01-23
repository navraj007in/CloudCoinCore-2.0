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
            //raida.Echo();
            FS.LoadFileSystem();
            FS.LoadFolderCoins(FS.RootPath + FS.CounterfeitFolder);
            //Load Local Coins

        }
        ParallelLoopResult result;

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
            Debug.WriteLine("Finished Echo");
            
            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                Debug.WriteLine("Parallel - Value-" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------");
           // raida.Echo();
            Debug.WriteLine("-----------------------------------");

        }

        private void cmdDetect_Click(object sender, RoutedEventArgs e)
        {
            FS.LoadFileSystem();
            foreach (var coin in FileSystem.importCoins)
            {
                // coin.GeneratePAN();
                coin.setAnsToPans();
                raida.coin = coin;
                var tasks = raida.GetDetectTasks(coin);
                Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                Debug.WriteLine("Coin No. - " + coin.sn + "Scanned");
                //Task.WaitAll(tasks.ToArray());
                //detect(coin);
                // await raida.DetectCoin(coin, CloudCoinCore.Config.milliSecondsToTimeOut);
              
            }
            MessageBox.Show("Finished Detect");
            //    new Thread( async delegate ()
            //{



            //}).Start();

        }

        private void detect(CloudCoin coin)
        {
            
        }
    }
}
