using System;
using System.Collections.Generic;
using System.Text;

namespace CloudCoinCoreDirectory
{
    public class Network
    {
        public int nn { get; set; }
        public RAIDANode[] raida { get; set; }
    }
    public class RAIDANode
    {
        public int raida_index { get; set; }
        public bool failsEcho { get; set; }
        public bool failsDetect { get; set; }
        public bool failsFix { get; set; }
        public bool failsTicket { get; set; }
        public string location { get; set; }
        public NodeURL[] urls { get; set; }
    }
    public class RAIDADirectory
    {
        public Network[] networks { get; set; }
    }
    public class directory
    {
        public Network[] networks { get; set; }
    }
    public class NodeURL
    {
        public string url { get; set; }
        public int? port { get; set; }
        public int? milliseconds { get; set; }

    }
}
