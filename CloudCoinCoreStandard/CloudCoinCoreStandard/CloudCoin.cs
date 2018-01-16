using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CloudCoinCore
{
    public class CloudCoin
    {
        public string[] pan;

        public List<Task> DetectTasks;

        //Fields
        [JsonProperty("nn")]
        public int nn { get; set; }

        [JsonProperty("sn")]
        public int sn { get; set; }

        [JsonProperty("an")]
        public List<string> an { get; set; }

        [JsonProperty("ed")]
        public string ed { get; set; }

        [JsonProperty("pown")]
        public string pown { get; set; }

        [JsonProperty("aoid")]
        public List<string> aoid { get; set; }


        //Constructors
        public CloudCoin()
        {

        }//end of constructor

        public CloudCoin(int nn, int sn, List<string> an, string ed, string pown, List<string> aoid)
        {
            this.nn = nn;
            this.sn = sn;
            this.an = an;
            this.ed = ed;
            this.pown = pown;
            this.aoid = aoid;
        }//end of constructor

    
    }
}

