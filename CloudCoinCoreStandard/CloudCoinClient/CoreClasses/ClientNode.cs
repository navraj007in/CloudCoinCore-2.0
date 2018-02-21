using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCoinCore;
using RestSharp;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CloudCoinClient.CoreClasses
{
    class ClientNode : Node
    {
        public ClientNode(int NodeNumber) : base(NodeNumber)
        {

        }

        internal GetTicketResponse getTicket(int nn, int sn, string an, int d)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri( GetFullURL());
            var request = new RestRequest("get_ticket");
            request.AddQueryParameter("nn", nn.ToString());
            request.AddQueryParameter("sn", sn.ToString());
            request.AddQueryParameter("an", an);
            request.AddQueryParameter("pan", an);
            request.AddQueryParameter("denomination", (d).ToString());
            request.Timeout = 5000;

            GetTicketResponse getTicketResult = new GetTicketResponse();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var response = client.Execute(request);
                getTicketResult = JsonConvert.DeserializeObject<GetTicketResponse>(response.Content);
            }
            catch (JsonException _)
            {
                getTicketResult = new GetTicketResponse(NodeNumber.ToString(), sn, "error", "The server does not respond or returns invalid data", DateTime.Now.ToString());
            }
            getTicketResult = getTicketResult ?? new GetTicketResponse(NodeNumber.ToString(), sn, "Network problem", "Node not found", DateTime.Now.ToString());
            if (getTicketResult.ErrorException != null)
                getTicketResult = new GetTicketResponse(NodeNumber.ToString(), sn, "Network problem", "Problems with network connection", DateTime.Now.ToString());
            sw.Stop();
            getTicketResult.responseTime = sw.Elapsed;
            //                Logger.Write("GetTicket request for coin: " + sn + " at node " + this.Number + ", timeout " + request.Timeout + " returned '" + 
            //                    getTicketResult.status + "' with message '" + getTicketResult.message + "' in " + sw.ElapsedMilliseconds + "ms.", Logger.Level.Debug);
            return getTicketResult;
        }//end get ticket

        internal FixResponse fix(Node[] triad, string m1, string m2, string m3, string pan, int sn)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(GetFullURL());
            var request = new RestRequest("fix");
            request.AddQueryParameter("fromserver1", triad[0].NodeNumber.ToString());
            request.AddQueryParameter("fromserver2", triad[1].NodeNumber.ToString());
            request.AddQueryParameter("fromserver3", triad[2].NodeNumber.ToString());
            request.AddQueryParameter("message1", m1);
            request.AddQueryParameter("message2", m2);
            request.AddQueryParameter("message3", m3);
            request.AddQueryParameter("pan", pan);
            request.Timeout = 10000;

            FixResponse fixResult = new FixResponse();
           // Logger.Write("Fix request to node " + NodeNumber + ": " + client.BuildUri(request), Logger.Level.Debug);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var response = client.Execute(request).Content;
                //                    Logger.Write("Server RAIDA" + Number + " returned following string on fix request: '" + response + "'", Logger.Level.Debug);
                fixResult = JsonConvert.DeserializeObject<FixResponse>(response);
            }
            catch (JsonException ex)
            {
                sw.Stop();
                fixResult = new FixResponse(NodeNumber.ToString(), sn, "error", "Server doesn't respond or returned invalid data", DateTime.Now.ToString());
                //                    Logger.Write("Fix request for coin: " + sn + " at node " + Number + ", timeout " + request.Timeout + " returned '" +
                //                    fixResult.status + "' with message '" + fixResult.message + "' return coin sn: '" + fixResult.sn + "' in " + sw.ElapsedMilliseconds + "ms.", Logger.Level.Debug);
                return fixResult;
            }
            fixResult = fixResult ?? new FixResponse(NodeNumber.ToString(), sn, "error", "Node not found", DateTime.Now.ToString());
            if (fixResult.ErrorException != null)
                fixResult = new FixResponse(NodeNumber.ToString(), sn, "error", "Problems with network connection", DateTime.Now.ToString());
            sw.Stop();
            fixResult.responseTime = sw.Elapsed;
            //                Logger.Write("Fix request for coin: " + sn + " at node " + Number + ", timeout " + request.Timeout + " returned '" +
            //                    fixResult.status + "' with message '" + fixResult.message + "' return coin sn: '" + fixResult.sn + "' in " + sw.ElapsedMilliseconds + "ms.", Logger.Level.Debug);
            return fixResult;

        }//end fix

    }
}
