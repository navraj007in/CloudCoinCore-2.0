using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

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

        public void Echo()
        {
            for(int i=0;i<Config.NodeCount;i++)
            {
                nodes[i].echo();
            }
        }
    }
}
