using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
        public MultiDetectResponse multiResponse = new MultiDetectResponse();

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
                Debug.WriteLine("Echo From Node - " + NodeNumber + ". " + echoResponse.fullResponse);
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
            //Debug.WriteLine("Echo Complete-Node No.-" + NodeNumber + ".Status-" + RAIDANodeStatus);
            return echoResponse;
        }//end detect

        public async Task<Response> Detect()
        {
            CloudCoin coin = RAIDA.GetInstance().coin;
            Response detectResponse = new Response();
            detectResponse.fullRequest = this.fullUrl + "detect?nn=" + coin.nn + "&sn=" + coin.sn + "&an=" + coin.an[NodeNumber-1] + "&pan=" + coin.pan[NodeNumber-1] + "&denomination=" + coin.denomination + "&b=t";
            DateTime before = DateTime.Now;
            coin.setAnsToPans();
            try
            {
                detectResponse.fullResponse = await Utils.GetHtmlFromURL(detectResponse.fullRequest);

                DateTime after = DateTime.Now; TimeSpan ts = after.Subtract(before);
                detectResponse.milliseconds = Convert.ToInt32(ts.Milliseconds);
                coin.response[this.NodeNumber-1] = detectResponse;

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
                coin.response[this.NodeNumber] = detectResponse;

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

        public class MultiDetectResponse
        {
            public Response[] responses;
        }
        public async Task<MultiDetectResponse> multiDetect(int[] nn, int[] sn, String[] an, String[] pan, int[] d, int timeout)
        {
            /*PREPARE REQUEST*/
            Response[] response = new Response[nn.Length];
            for (int i = 0; i < nn.Length; i++)
            {
                response[i] = new Response();
            }

            multiResponse.responses = new Response[nn.Length];

            //Create List of KeyValuePairs to use as the POST data
            List<KeyValuePair<string, string>> postVariables = new List<KeyValuePair<string, string>>();

            //Loop over String array and add all instances to our bodyPoperties
            for (int i = 0; i < nn.Length; i++)
            {
                postVariables.Add(new KeyValuePair<string, string>("nns[]", nn[i].ToString()));
                postVariables.Add(new KeyValuePair<string, string>("sns[]", sn[i].ToString()));
                postVariables.Add(new KeyValuePair<string, string>("ans[]", an[i]));
                postVariables.Add(new KeyValuePair<string, string>("pans[]", pan[i]));
                postVariables.Add(new KeyValuePair<string, string>("denomination[]", d[i].ToString()));
                //Debug.WriteLine("url is " + this.fullUrl + "detect?nns[]=" + nn[i] + "&sns[]=" + sn[i] + "&ans[]=" + an[i] + "&pans[]=" + pan[i] + "&denomination[]=" + d[i]);

                response[i].fullRequest = this.fullUrl + "detect?nns[]=" + nn[i] + "&sns[]=" + sn[i] + "&ans[]=" + an[i] + "&pans[]=" + pan[i] + "&denomination[]=" + d[i];//Record what was sent
            }

            //convert postVariables to an object of FormUrlEncodedContent
            var dataContent = new FormUrlEncodedContent(postVariables.ToArray());
            DateTime before = DateTime.Now;
            DateTime after;
            TimeSpan ts = new TimeSpan();


            /*MAKE REQEST*/
            string totalResponse = "";
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(timeout);

            try
            {
                //POST THE REQUEST AND FILL THE ANSER IN totalResponse
                totalResponse = "";
                HttpResponseMessage json;

                using (client)
                {
                    // Console.Write("postHtml await for response: ");
                    json = await client.PostAsync(fullUrl + "multi_detect", dataContent);

                    //Console.Write(".");
                    if (json.IsSuccessStatusCode)//200 status good
                    {
                        totalResponse = await json.Content.ReadAsStringAsync();
                        Debug.WriteLine("RAIDA " + NodeNumber + " returned good: " + json.StatusCode);
                        //  Console.Out.WriteLine(totalResponse);
                    }
                    else //404 not found or 500 error. 
                    {
                        Debug.WriteLine("RAIDA " + NodeNumber + " had an error: " + json.StatusCode);
                        after = DateTime.Now;
                        ts = after.Subtract(before);//Start the timer
                        for (int i = 0; i < nn.Length; i++)
                        {
                            response[i].outcome = "error";
                            response[i].fullResponse = json.StatusCode.ToString();
                            response[i].success = false;
                            response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                            //RAIDA_Status.failsDetect[RAIDANumber] = true;
                        }//end for every CloudCoin note
                        multiResponse.responses = response;
                        return multiResponse;//END IF THE REQUEST GOT AN ERROR

                    }//end else 404 or 500

                }//end using

            }
            catch (TaskCanceledException ex)//This means it timed out
            {
                // Console.WriteLine("T1:" + ex.Message);
                after = DateTime.Now;
                ts = after.Subtract(before);//Start the timer
                for (int i = 0; i < nn.Length; i++)
                {
                    response[i].outcome = "noresponse";
                    response[i].fullResponse = ex.Message;
                    response[i].success = false;
                    response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                    //RAIDA_Status.failsDetect[RAIDANumber] = true;
                }//end for every CloudCoin note
                multiResponse.responses = response;

                return multiResponse;//END IF THE REQUEST FAILED
            }
            catch (Exception ex)//Request failed with some kind of error that did not provide a response. 
            {
                // Console.WriteLine("M1:" + ex.Message);
                after = DateTime.Now;
                ts = after.Subtract(before);//Start the timer
                for (int i = 0; i < nn.Length; i++)
                {
                    response[i].outcome = "error";
                    response[i].fullResponse = ex.Message;
                    response[i].success = false;
                    response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                    //RAIDA_Status.failsDetect[RAIDANumber] = true;
                }//end for every CloudCoin note
                multiResponse.responses = response;
                return multiResponse;//END IF THE REQUEST FAILED
            }//end catch request attmept


            /* PROCESS REQUEST*/
            after = DateTime.Now;
            ts = after.Subtract(before);//Start the timer
            //Is the request a dud?
            if (totalResponse.Contains("dud"))
            {
                //Mark all Responses as duds
                for (int i = 0; i < nn.Length; i++)
                {
                    response[i].fullResponse = totalResponse;
                    response[i].success = false;
                    response[i].outcome = "dud";
                    response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                }//end for each dud
            }//end if dud
            else
            {
                //Not a dud so break up parts into smaller pieces
                //Remove leading "[{"
                totalResponse = totalResponse.Remove(0, 2);
                //Remove trailing "}]"
                totalResponse = totalResponse.Remove(totalResponse.Length - 2, 2);
                //Split by "},{"
                string[] responseArray = Regex.Split(totalResponse, "},{");
                //Check to see if the responseArray is the same length as the request detectResponse. They should be the same
                if (response.Length != responseArray.Length)
                {
                    //Mark all Responses as duds
                    for (int i = 0; i < nn.Length; i++)
                    {
                        response[i].fullResponse = totalResponse;
                        response[i].success = false;
                        response[i].outcome = "dud";
                        response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                    }//end for each dud
                }//end if lenghts are not the same
                else//Lengths are the same so lets go through each one
                {


                    for (int i = 0; i < nn.Length; i++)
                    {
                        if (responseArray[i].Contains("pass"))
                        {
                            response[i].fullResponse = responseArray[i];
                            response[i].outcome = "pass";
                            response[i].success = true;
                            response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                        }
                        else if (responseArray[i].Contains("fail") && responseArray[i].Length < 200)//less than 200 incase there is a fail message inside errored page
                        {
                            response[i].fullResponse = responseArray[i];
                            response[i].outcome = "fail";
                            response[i].success = false;
                            response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                        }
                        else
                        {
                            response[i].fullResponse = responseArray[i];
                            response[i].outcome = "error";
                            response[i].success = false;
                            response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                        }
                    }//End for each response
                }//end if array lengths are the same

            }//End Else not a dud
             //Break the respons into sub responses. 
             //RAIDA_Status.multiDetectTime[NodeNumber] = Convert.ToInt32(ts.Milliseconds);
            multiResponse.responses = response;

            return multiResponse;
        }//End multi detect

        //int[] nn, int[] sn, String[] an, String[] pan, int[] d, int timeout
        public async Task<MultiDetectResponse> MultiDetect()
        {
            /*PREPARE REQUEST*/
            try
            {

            var raida = RAIDA.GetInstance();
            int[] nn = raida.multiRequest.nn;
            int[] sn = raida.multiRequest.sn;
            String[] an = raida.multiRequest.an[NodeNumber-1];
            String[] pan = raida.multiRequest.pan[NodeNumber-1];
            int[] d = raida.multiRequest.d;
            int timeout = raida.multiRequest.timeout;

            Response[] response = new Response[nn.Length];
            for (int i = 0; i < nn.Length; i++)
            {
                response[i] = new Response();
            }

                //Create List of KeyValuePairs to use as the POST data
                List<KeyValuePair<string, string>> postVariables = new List<KeyValuePair<string, string>>();

                //Loop over String array and add all instances to our bodyPoperties
                for (int i = 0; i < nn.Length; i++)
                {
                    postVariables.Add(new KeyValuePair<string, string>("nns[]", nn[i].ToString()));
                    postVariables.Add(new KeyValuePair<string, string>("sns[]", sn[i].ToString()));
                    postVariables.Add(new KeyValuePair<string, string>("ans[]", an[i]));
                    postVariables.Add(new KeyValuePair<string, string>("pans[]", pan[i]));
                    postVariables.Add(new KeyValuePair<string, string>("denomination[]", d[i].ToString()));
                   // Debug.WriteLine("url is " + this.fullUrl + "detect?nns[]=" + nn[i] + "&sns[]=" + sn[i] + "&ans[]=" + an[i] + "&pans[]=" + pan[i] + "&denomination[]=" + d[i]);

                    response[i].fullRequest = this.fullUrl + "detect?nns[]=" + nn[i] + "&sns[]=" + sn[i] + "&ans[]=" + an[i] + "&pans[]=" + pan[i] + "&denomination[]=" + d[i];//Record what was sent
                }

                //convert postVariables to an object of FormUrlEncodedContent
                var dataContent = new FormUrlEncodedContent(postVariables.ToArray());
                DateTime before = DateTime.Now;
                DateTime after;
                TimeSpan ts = new TimeSpan();


                /*MAKE REQEST*/
                string totalResponse = "";
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(timeout);

                try
                {
                    //POST THE REQUEST AND FILL THE ANSER IN totalResponse
                    totalResponse = "";
                    HttpResponseMessage json;

                    using (client)
                    {
                        // Console.Write("postHtml await for response: ");
                        json = await client.PostAsync(fullUrl + "multi_detect", dataContent);

                        //Console.Write(".");
                        if (json.IsSuccessStatusCode)//200 status good
                        {
                            totalResponse = await json.Content.ReadAsStringAsync();
                            // Console.Out.WriteLine("RAIDA " + NodeNumber + " returned good: " + json.StatusCode);
                            //  Console.Out.WriteLine(totalResponse);
                        }
                        else //404 not found or 500 error. 
                        {
                            Console.Out.WriteLine("RAIDA " + NodeNumber + " had an error: " + json.StatusCode);
                            after = DateTime.Now;
                            ts = after.Subtract(before);//Start the timer
                            for (int i = 0; i < nn.Length; i++)
                            {
                                response[i].outcome = "error";
                                response[i].fullResponse = json.StatusCode.ToString();
                                response[i].success = false;
                                response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                                //RAIDA_Status.failsDetect[RAIDANumber] = true;
                            }//end for every CloudCoin note
                            multiResponse.responses = response;
                            return multiResponse;//END IF THE REQUEST GOT AN ERROR

                        }//end else 404 or 500

                    }//end using

                }
                catch (TaskCanceledException ex)//This means it timed out
                {
                    // Console.WriteLine("T1:" + ex.Message);
                    after = DateTime.Now;
                    ts = after.Subtract(before);//Start the timer
                    for (int i = 0; i < nn.Length; i++)
                    {
                        response[i].outcome = "noresponse";
                        response[i].fullResponse = ex.Message;
                        response[i].success = false;
                        response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                        //RAIDA_Status.failsDetect[RAIDANumber] = true;
                    }//end for every CloudCoin note
                    multiResponse.responses = response;

                    return multiResponse;//END IF THE REQUEST FAILED
                }
                catch (Exception ex)//Request failed with some kind of error that did not provide a response. 
                {
                    // Console.WriteLine("M1:" + ex.Message);
                    after = DateTime.Now;
                    ts = after.Subtract(before);//Start the timer
                    for (int i = 0; i < nn.Length; i++)
                    {
                        response[i].outcome = "error";
                        response[i].fullResponse = ex.Message;
                        response[i].success = false;
                        response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                        //RAIDA_Status.failsDetect[RAIDANumber] = true;
                    }//end for every CloudCoin note
                    multiResponse.responses = response;
                    return multiResponse;//END IF THE REQUEST FAILED
                }//end catch request attmept


                /* PROCESS REQUEST*/
                after = DateTime.Now;
                ts = after.Subtract(before);//Start the timer
                                            //Is the request a dud?
                if (totalResponse.Contains("dud"))
                {
                    //Mark all Responses as duds
                    for (int i = 0; i < nn.Length; i++)
                    {
                        response[i].fullResponse = totalResponse;
                        response[i].success = false;
                        response[i].outcome = "dud";
                        response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                    }//end for each dud
                }//end if dud
                else
                {
                    //Not a dud so break up parts into smaller pieces
                    //Remove leading "[{"
                    totalResponse = totalResponse.Remove(0, 2);
                    //Remove trailing "}]"
                    totalResponse = totalResponse.Remove(totalResponse.Length - 2, 2);
                    //Split by "},{"
                    string[] responseArray = Regex.Split(totalResponse, "},{");
                    //Check to see if the responseArray is the same length as the request detectResponse. They should be the same
                    if (response.Length != responseArray.Length)
                    {
                        //Mark all Responses as duds
                        for (int i = 0; i < nn.Length; i++)
                        {
                            response[i].fullResponse = totalResponse;
                            response[i].success = false;
                            response[i].outcome = "dud";
                            response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                        }//end for each dud
                    }//end if lenghts are not the same
                    else//Lengths are the same so lets go through each one
                    {


                        for (int i = 0; i < nn.Length; i++)
                        {
                            if (responseArray[i].Contains("pass"))
                            {
                                response[i].fullResponse = responseArray[i];
                                response[i].outcome = "pass";
                                response[i].success = true;
                                response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                            }
                            else if (responseArray[i].Contains("fail") && responseArray[i].Length < 200)//less than 200 incase there is a fail message inside errored page
                            {
                                response[i].fullResponse = responseArray[i];
                                response[i].outcome = "fail";
                                response[i].success = false;
                                response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                            }
                            else
                            {
                                response[i].fullResponse = responseArray[i];
                                response[i].outcome = "error";
                                response[i].success = false;
                                response[i].milliseconds = Convert.ToInt32(ts.Milliseconds);
                            }
                        }//End for each response
                    }//end if array lengths are the same

                }//End Else not a dud
                 //Break the respons into sub responses. 
                 //RAIDA_Status.multiDetectTime[NodeNumber] = Convert.ToInt32(ts.Milliseconds);
                multiResponse.responses = response;
                return multiResponse;

            }
            catch (Exception e)
            {

                Debug.WriteLine(e.Message);

            }
            return null;
        }//End multi detect

    }
}
