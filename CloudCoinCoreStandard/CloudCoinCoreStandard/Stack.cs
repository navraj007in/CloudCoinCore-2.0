using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloudCoinCore
{
    public class Stack
    {
        /* Properties */
       [JsonProperty("cloudcoin")]
        public CloudCoin[] cc { get; set; }
        
        /* Constructors */
        public Stack()
        {

        }
        public Stack(CloudCoin coin)
        {
            cc = new CloudCoin[1];
            cc[0] = coin;
        }
        public Stack(List<CloudCoin> coins)
        {
            cc = coins.ToArray();
        }
        public Stack(CloudCoin[] coins)
        {
            cc = coins;
        }

        /* Methods */ 
        
         public MultiDetectRequest getMultiDetectRequest(int timeout) {  //Method for Extracting a MultiDetection Request from stack file
            
            MultiDetectRequest returnRequest = new MultiDetectRequest();
            //Create the arrays for the detection request
            returnRequest.nn = new int[cc.Length];
            returnRequest.sn = new int[cc.Length];
            returnRequest.an = new string[cc.Length][];
            returnRequest.pan = new string[cc.Length][];
            returnRequest.timeout = timeout;
            
            for ( int i=0; i< cc.Length; i++) {
                returnRequest.nn[i] = cc[i].nn;
                returnRequest.sn[i] = cc[i].sn;
                returnRequest.an[i] = cc[i].an;
                returnRequest.pan[i] = cc[i].pan;
            }//end for each CloudCoin in the stack

            return returnRequest;
        }//end get multi detect request
    }
}
