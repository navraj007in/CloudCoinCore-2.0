﻿using System;
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
        public CloudCoin coin;
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
                Debug.WriteLine(coin.response[i].outcome);

            }//end for each detection agent

            coin.setAnsToPansIfPassed();
            coin.calculateHP();

            coin.calcExpirationDate();
            coin.grade();
        }//end detect coin


    }
}
