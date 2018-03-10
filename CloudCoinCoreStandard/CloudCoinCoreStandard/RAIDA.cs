﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace CloudCoinCore
{
    public class RAIDA
    {
        /*
         * 
         * This Class Contains and abstracts the properties of RAIDA network.
         * */
        public static RAIDA MainNetwork;
        public Node[] nodes = new Node[Config.NodeCount];
        public IFileSystem FS;
        public CloudCoin coin;
        public IEnumerable<CloudCoin> coins;
        public MultiDetectRequest multiRequest;

        // Singleton Pattern implemented using private constructor 
        // This allows only one instance of RAIDA per application

        private RAIDA()
        {
            for(int i = 0; i < Config.NodeCount; i++)
            {
                nodes[i] = new Node(i+1);
            }                   
        }
        public static RAIDA GetInstance()
        {
            if (MainNetwork != null)
                return MainNetwork;
            else
            {
                MainNetwork = new RAIDA();
                return MainNetwork;
            }
        }

        public List<Func<Task>> GetEchoTasks()
        {
            var echoTasks = new List<Func<Task>>
            {

            };
            for (int i = 0; i < nodes.Length; i++)
            {
                echoTasks.Add(nodes[i].Echo);
            }
            return echoTasks;
        }

        public List<Func<Task>> GetDetectTasks(CloudCoin coin)
        {
            this.coin = coin;

            var detectTasks = new List<Func<Task>>
            {

            };
            for (int i = 0; i < nodes.Length; i++)
            {
                detectTasks.Add(nodes[i].Detect);
            }
            return detectTasks;
        }



    
        public List<Func<Task>> GetMultiDetectTasks(CloudCoin[] coins, int milliSecondsToTimeOut)
        {
            this.coins = coins;

            responseArrayMulti = new Response[Config.NodeCount, coins.Length];

            int[] nns = new int[coins.Length];
            int[] sns = new int[coins.Length];


            String[][] ans = new String[Config.NodeCount][];
            String[][] pans = new String[Config.NodeCount][];

            int[] dens = new int[coins.Length];//Denominations
                                               //Stripe the coins
            var detectTasks = new List<Func<Task>>
            {

            };
            
            List<Func<Task>> multiTaskList = new List<Func<Task>>();

            //List<Task<Response[]>> multiTaskList = new List<Task<Response[]>>();
            for (int i = 0; i < coins.Length; i++)//For every coin
            {
                //coins[i].GeneratePAN();
                coins[i].SetAnsToPans();
                nns[i] = coins[i].nn;
                sns[i] = coins[i].sn;
                dens[i] = coins[i].denomination;

            }
            multiRequest = new MultiDetectRequest();
            multiRequest.timeout = Config.milliSecondsToTimeOut;
            for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
            {
                
                ans[nodeNumber] = new String[coins.Length];
                pans[nodeNumber] = new String[coins.Length];

                for (int i = 0; i < coins.Length; i++)//For every coin
                {
                    ans[nodeNumber][i] = coins[i].an[nodeNumber];
                    pans[nodeNumber][i] = coins[i].pan[nodeNumber];

                }
                multiRequest.an[nodeNumber] = ans[nodeNumber];
                multiRequest.pan[nodeNumber] = pans[nodeNumber];
                multiRequest.nn = nns;
                multiRequest.sn = sns;
                multiRequest.d = dens;
            }


            for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
            {
                detectTasks.Add(nodes[nodeNumber].MultiDetect);
            }

            return detectTasks;
        }
        public void get_Tickets(int[] triad, String[] ans, int nn, int sn, int denomination, int milliSecondsToTimeOut)
        {
            //Console.WriteLine("Get Tickets called. ");
            var t00 = get_Ticket(0, triad[00], nn, sn, ans[00], denomination);
            var t01 = get_Ticket(1, triad[01], nn, sn, ans[01], denomination);
            var t02 = get_Ticket(2, triad[02], nn, sn, ans[02], denomination);

            var taskList = new List<Task> { t00, t01, t02 };
            Task.WaitAll(taskList.ToArray(), milliSecondsToTimeOut);
            try
            {
                //  CoreLogger.Log(sn + " get ticket:" + triad[00] + " " + responseArray[triad[00]].fullResponse);
                // CoreLogger.Log(sn + " get ticket:" + triad[01] + " " + responseArray[triad[01]].fullResponse);
                //  CoreLogger.Log(sn + " get ticket:" + triad[02] + " " + responseArray[triad[02]].fullResponse);
            }
            catch { }
            //Get data from the detection agents
        }//end detect coin

        public async Task get_Ticket(int i, int raidaID, int nn, int sn, String an, int d)
        {
            //DetectionAgent da = new DetectionAgent(raidaID, 5000);
            responseArray[raidaID] = await nodes[raidaID].GetTicket(nn, sn, an, d);


        }//end get ticket
        public Response[] responseArray = new Response[25];

        public void GetTickets(int[] triad, String[] ans, int nn, int sn, int denomination, int milliSecondsToTimeOut)
        {
            //Console.WriteLine("Get Tickets called. ");
            var t00 = GetTicket(0, triad[00], nn, sn, ans[00], denomination);
            var t01 = GetTicket(1, triad[01], nn, sn, ans[01], denomination);
            var t02 = GetTicket(2, triad[02], nn, sn, ans[02], denomination);

            var taskList = new List<Task> { t00, t01, t02 };
            Task.WaitAll(taskList.ToArray(), milliSecondsToTimeOut);
            try
            {
                //  CoreLogger.Log(sn + " get ticket:" + triad[00] + " " + responseArray[triad[00]].fullResponse);
                // CoreLogger.Log(sn + " get ticket:" + triad[01] + " " + responseArray[triad[01]].fullResponse);
                //  CoreLogger.Log(sn + " get ticket:" + triad[02] + " " + responseArray[triad[02]].fullResponse);
            }
            catch { }
            //Get data from the detection agents
        }//end detect coin

        public async Task GetTicket(int i, int raidaID, int nn, int sn, String an, int d)
        {
            responseArray[raidaID] = await nodes[raidaID].GetTicket(nn, sn, an, d);
        }//end get ticket
        
        public async Task DetectCoin(CloudCoin coin, int milliSecondsToTimeOut)
        {
            //Task.WaitAll(coin.detectTaskList.ToArray(),Config.milliSecondsToTimeOut);
            //Get data from the detection agents
            //Task.WaitAll(coin.detectTaskList.ToArray(), milliSecondsToTimeOut);
            await Task.WhenAll(coin.detectTaskList);
            for (int i = 0; i < Config.NodeCount; i++)
            {
                var resp = coin.response;

            }//end for each detection agent

            var counts = coin.response
                .GroupBy(item => item.outcome== "pass")
                .Select(grp => new { Number = grp.Key, Count = grp.Count() });

            var countsf = coin.response
                    .GroupBy(item => item.outcome == "fail")
                    .Select(grp => new { Number = grp.Key, Count = grp.Count() });

            Debug.WriteLine("Pass Count -" +counts.Count());
            Debug.WriteLine("Fail Count -" + countsf.Count());

            coin.SetAnsToPansIfPassed();
            coin.CalculateHP();

            coin.CalcExpirationDate();
            coin.grade();
            coin.SortToFolder();
            DetectEventArgs de = new DetectEventArgs(coin);
            OnCoinDetected(de);

        }//end detect coin

        public event EventHandler ProgressChanged;
        public event EventHandler LoggerHandler;

        public int ReadyCount { get { return nodes.Where(x => x.RAIDANodeStatus == NodeStatus.Ready).Count(); } }
        public int NotReadyCount { get { return nodes.Where(x => x.RAIDANodeStatus == NodeStatus.NotReady).Count(); } }

        public virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public void OnLogRecieved(ProgressChangedEventArgs e)
        {
            LoggerHandler?.Invoke(this, e);
        }

        public event EventHandler CoinDetected;

        protected virtual void OnCoinDetected(DetectEventArgs e)
        {
            CoinDetected?.Invoke(this, e);
        }
        public Response[,] responseArrayMulti;


    }
}
