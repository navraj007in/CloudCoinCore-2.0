using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CloudCoinCore
{
    public enum NodeStatus
    {
        Ready,
        NotReady,
    }

    public class Node
    {
        /*
         * 
         * This Class Contains the properties of a RAIDA node.
         * 
         * */

        public int NodeNumber;
        public String fullUrl;
        public NodeStatus RAIDANodeStatus;
        //Constructor
        public Node(int NodeNumber)
        {
            this.NodeNumber = NodeNumber;
            fullUrl = GetFullURL();
        }

        public String GetFullURL()
        {
            return "https://RAIDA" + NodeNumber + ".cloudcoin.global/service/";
        }

        public async Task<Response> echo()
        {
            Response echoResponse = new Response();
            echoResponse.fullRequest = this.fullUrl + "echo?b=t";
            DateTime before = DateTime.Now;
            //RAIDA_Status.failsEcho[raidaID] = true;
            try
            {
                echoResponse.fullResponse = await Utils.GetHtmlFromURL(echoResponse.fullRequest);
                if (echoResponse.fullResponse.Contains("ready"))
                {
                    echoResponse.success = true;
                    echoResponse.outcome = "ready";
                    this.RAIDANodeStatus = NodeStatus.Ready;
                    //RAIDA_Status.failsEcho[raidaID] = false;
                }
                else
                {
                    this.RAIDANodeStatus = NodeStatus.NotReady;
                    echoResponse.success = false;
                    echoResponse.outcome = "error";
                    //RAIDA_Status.failsEcho[raidaID] = true;
                }
            }
            catch (Exception ex)
            {
                echoResponse.outcome = "error";
                echoResponse.success = false;
                this.RAIDANodeStatus = NodeStatus.NotReady;
                //RAIDA_Status.failsEcho[raidaID] = true;
                if (ex.InnerException != null)
                    echoResponse.fullResponse = ex.InnerException.Message;
                Debug.WriteLine("Error---"+ ex.Message);
            }
            DateTime after = DateTime.Now; TimeSpan ts = after.Subtract(before);
            echoResponse.milliseconds = Convert.ToInt32(ts.Milliseconds);
            Debug.WriteLine("Echo Complete-Node No.-" + NodeNumber + ".Status-" + RAIDANodeStatus);
            return echoResponse;
        }//end detect

    }
}
