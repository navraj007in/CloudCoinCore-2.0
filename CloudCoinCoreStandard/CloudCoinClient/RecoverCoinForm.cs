using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloudCoinCore;
using CloudCoinClient.CoreClasses;

namespace CloudCoinClient
{
    public partial class RecoverCoinForm : Form
    {
        int numFolders = 2;
        public static FileSystem FS = new FileSystem(Application.StartupPath);
        public RecoverCoinForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            numFolders = Convert.ToInt32(textBox2.Text);
            string[] folders = new string[numFolders];
            IEnumerable<CloudCoin>[] coins = new IEnumerable<CloudCoin>[numFolders];
            List<CloudCoin> RecoveredCoins = new List<CloudCoin>();
            int HighestCoins = 0;
            int[] sns = new int[numFolders];
            int totalSNCount = 0;
            for (int i=0;i<numFolders;i++)
            {
                //folderBrowserDialog1. = FS.RootPath;
                var result = folderBrowserDialog1.ShowDialog();
                if(result == DialogResult.OK)
                {
                    textBox1.AppendText(folderBrowserDialog1.SelectedPath + " Selected\n");
                    folders[i] = folderBrowserDialog1.SelectedPath;
                    coins[i] = FS.LoadFolderCoins(folders[i]);
                    int[] thissns = (from x in coins[i]
                                   select x.sn).ToArray();
                    totalSNCount += thissns.Count();
                    textBox1.AppendText(coins[i].Count() + " Coins Loaded\n");
                    if (coins[i].Count() > HighestCoins)
                        HighestCoins = coins[i].Count();

                }
            }
            //RecoveredCoins = new CloudCoin;

            sns = new int[totalSNCount];
            int k = 0;
            for(int j=0;j< numFolders;j++)
            {
                foreach(var coin in coins[j])
                {
                    sns[k++] = coin.sn;
                }
            }


            for(int a=0;a<totalSNCount;a++)
            {
                textBox1.AppendText((a + 1) + " th coin Serial No." + sns[a] + "\n");
            }

            var distinctSNs = (from w in sns
                                 select w).Distinct().ToArray();

            textBox1.AppendText( distinctSNs.Count() +  " distinct SNs found.Starting recovery.\n");

            for(int snCount=0;snCount<distinctSNs.Count();snCount++)
            {
                List<CloudCoin> currentSNCoins = new List<CloudCoin>();
                
                for(int ii=0;ii<numFolders;ii++)
                {
                    var isCoin = IfcoinExists(coins[ii].ToArray(), distinctSNs[snCount]);
                    if (isCoin!=null)
                    {
                        currentSNCoins.Add(isCoin);
                    }
                }
                textBox1.AppendText("For Serial " + distinctSNs[snCount] + ", " + currentSNCoins.Count() + " Coins found\n");
                CloudCoin currentCoin = new CloudCoin();
                currentCoin.sn = distinctSNs[snCount];

                for(int z=0;z<CloudCoinCore.Config.NodeCount;z++)
                {
                    currentCoin.an.Add("");
                    
                }

                foreach(var foundCoin in currentSNCoins)
                {
                    var pown = foundCoin.pown.ToCharArray();

                    for(int i=0;i<pown.Length;i++)
                    {
                        if(pown[i] == 'p')
                        {
                            currentCoin.an[i] = foundCoin.an[i];
                            currentCoin.pan[i] = foundCoin.an[i];
                        }
                        else
                        {
                            //currentCoin.an[i] = foundCoin.generatePan();
                            //currentCoin.pan[i] = currentCoin.an[i];
                        }
                    }
                    currentCoin.ed = foundCoin.ed;
                    currentCoin.edHex = foundCoin.edHex;
                    currentCoin.hp = foundCoin.hp;
                   
                }
                for(int r=0;r<CloudCoinCore.Config.NodeCount;r++)
                {
                    if (currentCoin.an[r] == "")
                        currentCoin.pan[r] = currentCoin.an[r] = currentCoin.generatePan();
                }
                RecoveredCoins.Add(currentCoin);
            }

            saveFileDialog1.ShowDialog();
            string filename = saveFileDialog1.FileName;
            //var filteredCoins = RecoveredCoins.Select(x=>x).Distinct(x.sn);

            FS.WriteCoinsToFile(RecoveredCoins, filename);
            textBox1.AppendText("Coins saved to file " + filename + "\n");
            //for (int cCount = 0; cCount < numFolders; cCount++)
            //{
            //    foreach (var coin in coins[cCount])
            //    {
            //        textBox1.AppendText("SN-"+ coin.sn + " POWN- "+coin.pown +" Pass Count "+ coin.pown.ToCharArray().Count(c => c == 'p') + "\n");
            //        if(!IfcoinExists(RecoveredCoins.ToArray(),coin)) {
            //            RecoveredCoins.Add(coin);
            //        }
            //    }
            //}

        }

        public CloudCoin IfcoinExists(CloudCoin[] coins, int sn)
        {
            for (int i = 0; i < coins.Count(); i++)
                if (coins[i].sn == sn)
                    return coins[i];
            return null;
        }

        public bool IfcoinExists(CloudCoin[] coins,CloudCoin coin)
        {
            for (int i = 0; i < coins.Count(); i++)
                if (coins[i].sn == coin.sn)
                    return true;
            return false;
        }
    }
}
