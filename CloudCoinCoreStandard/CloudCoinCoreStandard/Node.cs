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
        public int readTimeout;
        public NodeStatus RAIDANodeStatus = NodeStatus.NotReady;
        //Constructor
        public Node(int NodeNumber)
        {
            this.NodeNumber = NodeNumber;
            fullUrl = GetFullURL();
            Debug.WriteLine(fullUrl);
        }

        public String GetFullURL()
        {
            return "https://RAIDA" + (NodeNumber-1) + ".cloudcoin.global/service/";
        }

        public async Task<Response> Echo()
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

        /**
         * Method DETECT
         * Sends a Detection request to a RAIDA server
         * @param nn  int that is the coin's Network Number 
         * @param sn  int that is the coin's Serial Number
         * @param an String that is the coin's Authenticity Number (GUID)
         * @param pan String that is the Proposed Authenticity Number to replace the AN.
         * @param d int that is the Denomination of the Coin
         * @return Response object. 
         */
        public async Task<Response> Detect(CloudCoin coin)
        {
            Response detectResponse = new Response();
            detectResponse.fullRequest = this.fullUrl + "detect?nn=" + coin.nn + "&sn=" + coin.sn + "&an=" + coin.an[NodeNumber] + "&pan=" + coin.pan[NodeNumber] + "&denomination=" + coin.denomination + "&b=t";
            DateTime before = DateTime.Now;
            coin.setAnsToPans();
            try
            {
                detectResponse.fullResponse = await Utils.GetHtmlFromURL(detectResponse.fullRequest);
                
                DateTime after = DateTime.Now; TimeSpan ts = after.Subtract(before);
                detectResponse.milliseconds = Convert.ToInt32(ts.Milliseconds);
                coin.response = detectResponse;

                if (detectResponse.fullResponse.Contains("pass"))
                {
                    detectResponse.outcome = "pass";
                    detectResponse.success = true;
                }
                else if (detectResponse.fullResponse.Contains("fail") && detectResponse.fullResponse.Length < 200)//less than 200 incase their is a fail message inside errored page
                {
                    detectResponse.outcome = "fail";
                    detectResponse.success = false;
                    RAIDANodeStatus = NodeStatus.Ready;
                    //RAIDA_Status.failsDetect[RAIDANumber] = true;
                }
                else
                {
                    detectResponse.outcome = "error";
                    detectResponse.success = false;
                    RAIDANodeStatus = NodeStatus.NotReady;
                    //RAIDA_Status.failsDetect[RAIDANumber] = true;
                }

            }
            catch (Exception ex)
            {
                detectResponse.outcome = "error";
                detectResponse.fullResponse = ex.InnerException.Message;
                detectResponse.success = false;
            }
            return detectResponse;
        }//end detect

    }
}
