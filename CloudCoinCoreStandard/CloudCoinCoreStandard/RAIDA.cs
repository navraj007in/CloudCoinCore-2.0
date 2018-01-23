using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Diagnostics;

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
        
        public List<Task> GetDetectTasks(CloudCoin cc)
        {
            var detectTasks = new List<Task>
            {

            };

            for (int i = 0; i < nodes.Length; i++)
            {
               detectTasks.Add( Task.Factory.StartNew(() => nodes[i].Detect(cc)));

                //detectTasks.Add(nodes[i].Detect(cc));

            }
            return detectTasks;
        }
        public void Echo()
        {
            for(int i=0;i<Config.NodeCount;i++)
            {
                nodes[i].Echo();
            }
        }

        public void Detect()
        {

        }
        public Response[] responseArray = new Response[25];

        public void DetectCoin(CloudCoin coin, Task<Response>[] taskList, int milliSecondsToTimeOut)
        {

            var results =  Task.WaitAll(taskList, milliSecondsToTimeOut);
            //Get data from the detection agents
            //var result = ((Task<Response>)taskList[0]).Result;
            //Debug.WriteLine(result.fullResponse);
            var response = taskList[0].Result;

            //for (int i = 0; i < Config.NodeCount; i++)
            //{
            //    response = taskList[0].Result;
            //    Debug.WriteLine(response);
            //    try
            //    {
            //        if (responseArray[i] != null)
            //        {
            //            coin.setPastStatus(responseArray[i].outcome, i);
            //        }
            //        else
            //        {
            //            coin.setPastStatus("undetected", i);
            //        };// should be pass, fail, error or undetected. 
            //    }
            //    catch(Exception e)
            //    {

            //    }
            //}//end for each detection agent

            coin.setAnsToPansIfPassed();
            coin.calculateHP();
            // cu.gradeCoin(); // sets the grade and figures out what the file extension should be (bank, fracked, counterfeit, lost
            coin.calcExpirationDate();
            coin.grade();

            //return cu;
        }//end detect coin

        public async void DetectCoin(CloudCoin coin, int milliSecondsToTimeOut)
        {
            //try
            {
                Task.WaitAll(coin.detectTaskList.ToArray(),Config.milliSecondsToTimeOut);
            }
            //catch(Exception e)
            {

            }
            //Get data from the detection agents
            //await Task.WhenAll(coin.DetectTasks.AsParallel().Select(async task => await task()));
            //var result = ((Task<Response>)taskList[0]).Result;
            //Debug.WriteLine(result.fullResponse);
            for (int i = 0; i < Config.NodeCount; i++)
            {
                ///var result = ((Task<Response>)coin.DetectTasks[i]).Result;
                var resp = coin.response;
                //Debug.WriteLine(result);
                Debug.WriteLine(coin.response.outcome);

                //if (responseArray[i] != null)
                //{
                //    coin.setPastStatus(responseArray[i].outcome, i);
                //}
                //else
                //{
                //    coin.setPastStatus("undetected", i);
                //};// should be pass, fail, error or undetected. 
            }//end for each detection agent

            coin.setAnsToPansIfPassed();
            coin.calculateHP();
            // cu.gradeCoin(); // sets the grade and figures out what the file extension should be (bank, fracked, counterfeit, lost
            coin.calcExpirationDate();
            coin.grade();
            //return true;
            //return cu;
        }//end detect coin


    }
}
