﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudCoinCore
{
    public class CloudCoin
    {
        public string[] pan = new string[Config.NodeCount];
        public int hp;// HitPoints (1-25, One point for each server not failed)
        public String edHex;// Months from zero date that the coin will expire. 
        public string folder;
        public Task<Response>[] DetectTasks = new Task<Response>[Config.NodeCount];
        public Task[] DetectionTasks = new Task[Config.NodeCount];

        public Response response;
        public String[] gradeStatus = new String[3];// What passed, what failed, what was undetected
        //Fields
        [JsonProperty("nn")]
        public int nn { get; set; }

        [JsonProperty("sn")]
        public int sn { get { return pSN;  } set { pSN = value; denomination = getDenomination(); } }

        [JsonProperty("an")]
        public List<string> an { get; set; }

        [JsonProperty("ed")]
        public string ed { get; set; }

        [JsonProperty("pown")]
        public string pown { get; set; }

        [JsonProperty("aoid")]
        public List<string> aoid { get; set; }

        public int denomination { get; set; }

        int pSN;
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

        public int getDenomination()
        {
            int nom = 0;
            if ((sn < 1))
            {
                nom = 0;
            }
            else if ((sn < 2097153))
            {
                nom = 1;
            }
            else if ((sn < 4194305))
            {
                nom = 5;
            }
            else if ((sn < 6291457))
            {
                nom = 25;
            }
            else if ((sn < 14680065))
            {
                nom = 100;
            }
            else if ((sn < 16777217))
            {
                nom = 250;
            }
            else
            {
                nom = '0';
            }

            return nom;
        }//end get denomination
        public List<Task> detectTaskList;
        public Task[] GetDetectTasks()
        {

            var raida = RAIDA.GetInstance();
            var detectTasks = new List<Task>
            {

            };

            Task<Response>[] taskArray = new Task<Response>[Config.NodeCount];

            CloudCoin cc = this;
            var results = new Double[taskArray.Length];
            int i = 0;
            
                //Task t = Task.Factory.StartNew(() => raida.nodes[i].Detect(cc));
                //taskArray[i] = raida.nodes[i].Detect(this);
                //DetectionTasks[i] = t;
                //detectTasks.Add(taskArray[i]);

            for(int j = 0; j < Config.NodeCount; j++)
            {
                Debug.WriteLine("Count-" + j);
                Task t = Task.Factory.StartNew(() => raida.nodes[i].Detect(cc));
                taskArray[i] = raida.nodes[i].Detect(this);
                DetectionTasks[i] = t;
                detectTasks.Add(taskArray[i]);
            }
            //while (i < Config.NodeCount)
            //{
            //    Task t = Task.Factory.StartNew(() => raida.nodes[i].Detect(cc));
            //    taskArray[i] = raida.nodes[i].Detect(this);
            //    DetectionTasks[i] = t;
            //    detectTasks.Add(taskArray[i]);
            //    i++;

            //}
            //for (int i = 0; i < 25; i++)
            //{
            //    if (i == Config.NodeCount)
            //        break;
            //    Task t = Task.Factory.StartNew(() => raida.nodes[i].Detect(cc));
            //    taskArray[i] = raida.nodes[i].Detect(this);
            //    DetectionTasks[i] = t;
            //    detectTasks.Add(taskArray[i]);
            //}
            detectTaskList = detectTasks;
            //DetectTasks = taskArray;
            return taskArray;
        }
        public void GeneratePAN()
        {
            for (int i = 0; i < Config.NodeCount; i++) {
                pan[i] = this.generatePan();
            }
        }

        public void setAnsToPans()
        {
            for (int i = 0; (i < Config.NodeCount); i++)
            {
                this.pan[i] = an[i];
            }// end for 25 ans
        }// end setAnsToPans

        public void setAnsToPansIfPassed(bool partial = false)
        {
            // now set all ans that passed to the new pans
            char[] pownArray = pown.ToCharArray();

            for (int i = 0; (i < 25); i++)
            {
                if (pownArray[i] == 'p')//1 means pass
                {
                    an[i] = pan[i];
                }
                else if (pownArray[i] == 'u' && !(RAIDA.GetInstance().nodes[i].RAIDANodeStatus == NodeStatus.NotReady) && partial == false)//Timed out but there server echoed. So it probably changed the PAN just too slow of a response
                {
                    an[i] = pan[i];
                }
                else
                {
                    // Just keep the ans and do not change. Hopefully they are not fracked. 
                }
            }// for each guid in coin
        }// end set ans to pans if passed

        public void calculateHP()
        {
            hp = 25;
            char[] pownArray = pown.ToCharArray();
            for (int i = 0; (i < 25); i++)
            {
                if (pownArray[i] == 'f')
                {
                    this.hp--;
                }
            }
        }//end calculate hp


        public bool setPastStatus(string status, int raida_id)
        {

            char[] pownArray = this.pown.ToCharArray();
            switch (status)
            {
                case "error": pownArray[raida_id] = 'e'; break;
                case "fail": pownArray[raida_id] = 'f'; break;
                case "pass": pownArray[raida_id] = 'p'; break;
                case "undetected": pownArray[raida_id] = 'u'; break;
                case "noresponse": pownArray[raida_id] = 'n'; break;
            }//end switch
            this.pown = new string(pownArray);
            return true;
        }//end set past status

        public String generatePan()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] cryptoRandomBuffer = new byte[16];
                rng.GetBytes(cryptoRandomBuffer);

                Guid pan = new Guid(cryptoRandomBuffer);
                String rawpan = pan.ToString("N");
                String fullPan = "";
                switch (rawpan.Length)//Make sure the pan is 32 characters long. The odds of this happening are slim but it will happen.
                {
                    case 27: fullPan = ("00000" + rawpan); break;
                    case 28: fullPan = ("0000" + rawpan); break;
                    case 29: fullPan = ("000" + rawpan); break;
                    case 30: fullPan = ("00" + rawpan); break;
                    case 31: fullPan = ("0" + rawpan); break;
                    case 32: fullPan = rawpan; break;
                    case 33: fullPan = rawpan.Substring(0, rawpan.Length - 1); break;//trim one off end
                    case 34: fullPan = rawpan.Substring(0, rawpan.Length - 2); break;//trim one off end
                }

                return fullPan;
            }
        }

        public String[] grade()
        {
            int passed = 0;
            int failed = 0;
            int other = 0;
            String passedDesc = "";
            String failedDesc = "";
            String otherDesc = "";
            char[] pownArray = pown.ToCharArray();

            for (int i = 0; (i < 25); i++)
            {
                if (pownArray[i] == 'p')
                {
                    passed++;
                }
                else if (pownArray[i] == 'f')
                {
                    failed++;
                }
                else
                {
                    other++;
                }// end if pass, fail or unknown
            }

            // for each status
            // Calculate passed
            if (passed == 25)
            {
                passedDesc = "100% Passed!";
            }
            else if (passed > 17)
            {
                passedDesc = "Super Majority";
            }
            else if (passed > 13)
            {
                passedDesc = "Majority";
            }
            else if (passed == 0)
            {
                passedDesc = "None";
            }
            else if (passed < 5)
            {
                passedDesc = "Super Minority";
            }
            else
            {
                passedDesc = "Minority";
            }

            // Calculate failed
            if (failed == 25)
            {
                failedDesc = "100% Failed!";
            }
            else if (failed > 17)
            {
                failedDesc = "Super Majority";
            }
            else if (failed > 13)
            {
                failedDesc = "Majority";
            }
            else if (failed == 0)
            {
                failedDesc = "None";
            }
            else if (failed < 5)
            {
                failedDesc = "Super Minority";
            }
            else
            {
                failedDesc = "Minority";
            }

            // Calcualte Other RAIDA Servers did not help. 
            switch (other)
            {
                case 0:
                    otherDesc = "100% of RAIDA responded";
                    break;
                case 1:
                case 2:
                    otherDesc = "Two or less RAIDA errors";
                    break;
                case 3:
                case 4:
                    otherDesc = "Four or less RAIDA errors";
                    break;
                case 5:
                case 6:
                    otherDesc = "Six or less RAIDA errors";
                    break;
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    otherDesc = "Between 7 and 12 RAIDA errors";
                    break;
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    otherDesc = "RAIDA total failure";
                    break;
                default:
                    otherDesc = "FAILED TO EVALUATE RAIDA HEALTH";
                    break;
            }
            // end RAIDA other errors and unknowns
            // Coin will go to bank, counterfeit or fracked
            if (other > 12)
            {
                // not enough RAIDA to have a quorum
                folder = RAIDA.GetInstance().FS.SuspectFolder;
            }
            else if (failed > passed)
            {
                // failed out numbers passed with a quorum: Counterfeit
                folder = RAIDA.GetInstance().FS.CounterfeitFolder;
            }
            else if (failed > 0)
            {
                // The quorum majority said the coin passed but some disagreed: fracked. 
                folder = RAIDA.GetInstance().FS.FrackedFolder;
            }
            else
            {
                // No fails, all passes: bank
                folder = RAIDA.GetInstance().FS.BankFolder;

            }

            gradeStatus[0] = passedDesc;
            gradeStatus[1] = failedDesc;
            gradeStatus[2] = otherDesc;
            return this.gradeStatus;
        }// end gradeStatus

        public void calcExpirationDate()
        {
            DateTime expirationDate = DateTime.Today.AddYears(Config.YEARSTILEXPIRE);
            ed = (expirationDate.Month + "-" + expirationDate.Year);
            //  Console.WriteLine("ed = " + cc.ed);
            DateTime zeroDate = new DateTime(2016, 08, 13);
            // DateTime zeroDate = DateTime.Parse("8/13/2016 8:33:21 AM");
            int monthsAfterZero = (int)(expirationDate.Subtract(zeroDate).Days / (365.25 / 12));
            //Turn positive and up to down to floor
            // Console.WriteLine("Months after zero = " + monthsAfterZero);
            this.edHex = monthsAfterZero.ToString("X2");
        }// end calc exp date

    }
}

