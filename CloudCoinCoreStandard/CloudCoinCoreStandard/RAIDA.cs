using System;
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

        //public Task<Response[]> GetMultiDetecttionTasks(CloudCoin[] coins, int milliSecondsToTimeOut)
        //{
        //    this.coins = coins;

        //    responseArrayMulti = new Response[Config.NodeCount, coins.Length];

        //    int[] nns = new int[coins.Length];
        //    int[] sns = new int[coins.Length];


        //    String[][] ans = new String[Config.NodeCount][];
        //    String[][] pans = new String[Config.NodeCount][];

        //    int[] dens = new int[coins.Length];//Denominations
        //                                       //Stripe the coins

        //    //List<Func<Task>> multiTaskList = new List<Func<Task>>();
        //    Task<Response[]> multiTaskList = new Task<Response[]>() { };
        //    //var taskList;
        //    for (int i = 0; i < coins.Length; i++)//For every coin
        //    {
        //        coins[i].setAnsToPans();
        //        nns[i] = coins[i].nn;
        //        sns[i] = coins[i].sn;
        //        dens[i] = coins[i].denomination;
        //        //ans[i] = new String[coins.Length];
        //        //pans[i] = new String[coins.Length];

        //        //ans[i] = new String[Config.MultiDetectLoad];
        //        //pans[i] = new String[Config.MultiDetectLoad];

        //        for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
        //        {
        //            ans[nodeNumber][i] = coins[i].an[nodeNumber];
        //            pans[nodeNumber][i] = coins[i].pan[nodeNumber];

        //        }


        //    }//end for every coin put in an array
        //    var tasks;
        //    for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
        //    {
        //        var task = detectOneMulti(nodeNumber, nns, sns, ans[nodeNumber], pans[nodeNumber], dens, milliSecondsToTimeOut);
        //        multiTaskList.Add(task);
                
        //    }

        //    return multiTaskList;
        //}


        public List<Task<Response[]>> GetMultiDetectTasksold(CloudCoin[] coins, int milliSecondsToTimeOut)
        {
            this.coins = coins;

            responseArrayMulti = new Response[Config.NodeCount, coins.Length];

            int[] nns = new int[coins.Length];
            int[] sns = new int[coins.Length];


            String[][] ans = new String[Config.NodeCount][];
            String[][] pans = new String[Config.NodeCount][];

            int[] dens = new int[coins.Length];//Denominations
                                               //Stripe the coins

            //List<Func<Task>> multiTaskList = new List<Func<Task>>();
            List<Task<Response[]>> multiTaskList = new List<Task<Response[]>>();

            for (int i = 0; i < coins.Length; i++)//For every coin
            {
                coins[i].setAnsToPans();
                nns[i] = coins[i].nn;
                sns[i] = coins[i].sn;
                dens[i] = coins[i].denomination;
                //ans[i] = new String[coins.Length];
                //pans[i] = new String[coins.Length];

                //ans[i] = new String[Config.MultiDetectLoad];
                //pans[i] = new String[Config.MultiDetectLoad];

                for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
                {
                    ans[nodeNumber][i] = coins[i].an[nodeNumber];
                    pans[nodeNumber][i] = coins[i].pan[nodeNumber];

                }


            }//end for every coin put in an array

            for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
            {
               // var task = detectOneMulti(nodeNumber, nns, sns, ans[nodeNumber], pans[nodeNumber], dens, milliSecondsToTimeOut);
                //multiTaskList.Add(task);
            }

            return multiTaskList;
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
                coins[i].setAnsToPans();
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


        public Response[] responseArray = new Response[25];

        public async Task DetectCoin(CloudCoin coin, int milliSecondsToTimeOut)
        {
            //Task.WaitAll(coin.detectTaskList.ToArray(),Config.milliSecondsToTimeOut);
            //Get data from the detection agents
            //Task.WaitAll(coin.detectTaskList.ToArray(), milliSecondsToTimeOut);
            await Task.WhenAll(coin.detectTaskList);
            for (int i = 0; i < Config.NodeCount; i++)
            {
                var resp = coin.response;
               // Debug.WriteLine(coin.response[i].outcome);

            }//end for each detection agent

            var counts = coin.response
                .GroupBy(item => item.outcome== "pass")
                .Select(grp => new { Number = grp.Key, Count = grp.Count() });

            var countsf = coin.response
                    .GroupBy(item => item.outcome == "fail")
                    .Select(grp => new { Number = grp.Key, Count = grp.Count() });

            Debug.WriteLine("Pass Count -" +counts.Count());
            Debug.WriteLine("Fail Count -" + countsf.Count());

            coin.setAnsToPansIfPassed();
            coin.calculateHP();

            coin.calcExpirationDate();
            coin.grade();
            DetectEventArgs de = new DetectEventArgs(coin);
            OnCoinDetected(de);

        }//end detect coin

        public event EventHandler CoinDetected;

        protected virtual void OnCoinDetected(DetectEventArgs e)
        {
            CoinDetected?.Invoke(this, e);
        }
        public Response[,] responseArrayMulti;

        //public CloudCoin[] detectMultiCoin(CloudCoin[] coins, int milliSecondsToTimeOut)
        //{
        //    //Make arrays to stripe the coins

        //    responseArrayMulti = new Response[Config.NodeCount, coins.Length];

        //    int[] nns = new int[coins.Length];
        //    int[] sns = new int[coins.Length];
            

        //    String[][] ans = new String[Config.NodeCount][];
        //    String[][] pans = new String[Config.NodeCount][];

        //    int[] dens = new int[coins.Length];//Denominations
        //                                       //Stripe the coins

        //    //List<Func<Task>> multiTaskList = new List<Func<Task>>();
        //    List<Task<Response[]>> multiTaskList = new List<Task<Response[]>>();

        //    for(int arrCount =0;arrCount<Config.NodeCount;arrCount++)
        //    {
        //        ans[arrCount] = new String[coins.Length];
        //        pans[arrCount] = new String[coins.Length];

        //    }
        //    for (int i = 0; i < coins.Length; i++)//For every coin
        //    {
        //        coins[i].setAnsToPans();
        //        nns[i] = coins[i].nn;
        //        sns[i] = coins[i].sn;
        //        dens[i] = coins[i].denomination;
        //        //ans[i] = new String[coins.Length];
        //        //pans[i] = new String[coins.Length];

        //        //ans[i] = new String[Config.MultiDetectLoad];
        //        //pans[i] = new String[Config.MultiDetectLoad];

        //        for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
        //        {
        //            ans[nodeNumber][i] = coins[i].an[nodeNumber];
        //            pans[nodeNumber][i] = coins[i].pan[nodeNumber];

        //        }


        //    }//end for every coin put in an array

        //    for (int nodeNumber = 0; nodeNumber < Config.NodeCount; nodeNumber++)
        //    {
        //        var task = detectOneMulti(nodeNumber, nns, sns, ans[nodeNumber], pans[nodeNumber], dens, milliSecondsToTimeOut);
        //        multiTaskList.Add(task);
        //    }
        //    Task.WaitAll(multiTaskList.ToArray(), milliSecondsToTimeOut);

        //    //Get data from the detection agents
        //    for (int i = 0; i < nns.Length; i++)
        //    {
        //        for (int j = 0; j < 25; j++)
        //        {//For each coin
        //            if (responseArrayMulti[j, i] != null)
        //            {
        //                coins[i].setPastStatus(responseArrayMulti[j, i].outcome, j);
                        
        //                //updateLog(cu[i].cc.sn + " detect:" + j + " " + responseArrayMulti[j, i].fullResponse);

        //            }
        //            else
        //            {
        //                coins[i].setPastStatus("undetected", j);

        //            };// should be pass, fail, error or undetected, or No response. 
        //        }//end for each coin checked
                
        //        coins[i].setAnsToPansIfPassed();
        //        coins[i].calculateHP();
        //        coins[i].calcExpirationDate();
        //        coins[i].grade();
        //    }//end for each detection agent

        //    return coins;//Return the array of coins detected
        //}//end detect coin

        /* For Multi-Detect*/

//        public async Response[] DetectOneMulti1(int raida_id, int[] nn, int[] sn, String[] an, String[] pan, int[] d, int milliSecondsToTimeOut)
//        {
//            //DetectionAgent da = new DetectionAgent(raida_id);

//            Response[] tempArray = await nodes[raida_id].multiDetect(nn, sn, an, pan, d, milliSecondsToTimeOut);

//            for (int i = 0; i < tempArray.Length; i++)//Fill up the array
//            {
//                responseArrayMulti[raida_id, i] = tempArray[i];
//            }

//            return nodes[raida_id].multiDetect(nn, sn, an, pan, d, milliSecondsToTimeOut);
////            return tempArray;
//        }//end detectOneMulti
//        public async Task<Response[]> detectOneMulti(int raida_id, int[] nn, int[] sn, String[] an, String[] pan, int[] d, int milliSecondsToTimeOut)
//        {
//            //DetectionAgent da = new DetectionAgent(raida_id);

//            Response[] tempArray = await nodes[raida_id].multiDetect(nn, sn, an, pan, d, milliSecondsToTimeOut);

//            for (int i = 0; i < tempArray.Length; i++)//Fill up the array
//            {
//                responseArrayMulti[raida_id, i] = tempArray[i];
//            }

//            return tempArray;
//        }//end detectOneMulti
    }
}
