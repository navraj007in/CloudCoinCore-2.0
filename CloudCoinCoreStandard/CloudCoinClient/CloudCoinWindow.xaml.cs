using CloudCoinClient.CoreClasses;
using CloudCoinCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace CloudCoinClient
{
    /// <summary>
    /// Interaction logic for CloudCoinWindow.xaml
    /// </summary>
    public partial class CloudCoinWindow : Window
    {
        string RootPath;
        FileSystem FS;
        RAIDA raida;
        public CloudCoinWindow()
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
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            raida.FS = FS;
            //CoinDetected += Raida_CoinDetected;
            //raida.Echo();
            FS.ClearCoins(FS.PreDetectFolder);
            FS.ClearCoins(FS.DetectedFolder);

            FS.LoadFileSystem();
            //FS.LoadFolderCoins(FS.CounterfeitFolder);
            //Load Local Coins

            echo();
        }

        public async void echo()
        {
            var echos = raida.GetEchoTasks();
            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            MessageBox.Show("Finished Echo");
            int readyCount = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.Ready).Count();
            int notreadyCount = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).Count();
            var notReadyNodes = raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).ToList();
            Debug.WriteLine("RAIDA nodes ready-" + raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.Ready).Count());
            Debug.WriteLine("RAIDA nodes not ready-"+ raida.nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).Count());

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                Debug.WriteLine("Node" + i + " Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------");
        }
    }
}
