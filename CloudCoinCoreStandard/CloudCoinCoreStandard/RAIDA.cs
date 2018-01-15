using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

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
                detectTasks.Add(nodes[i].Detect(Config.NetworkNumber,cc.sn, cc.an[i],cc.pan[i],0));
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
    }
}
