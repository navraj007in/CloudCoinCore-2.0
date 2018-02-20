using System;
using CloudCoinCore;
using CloudCoinClient.CoreClasses;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace CloudCoinConsole
{
    class Program
    {
        public static KeyboardReader reader = new KeyboardReader();
        public static String rootFolder = Directory.GetCurrentDirectory();
        static FileSystem FS = new FileSystem(rootFolder);
        static RAIDA raida;
        public static String prompt = "> ";
        public static String[] commandsAvailable = new String[] { "Echo raida", "Show CloudCoins in Bank", "Import / Pown & Deposit", "Export / Withdraw", "Fix Fracked", "Show Folders", "Export stack files with one note each", "Help", "Quit" };

        static void Main(string[] args)
        {
            Console.Out.WriteLine("Loading File system...");
            Setup();
            Console.Out.WriteLine("File system loading Completed.");
            int argLength = args.Length;
            if (argLength > 0)
            {
                handleCommand(args);
            }
            else
            {
                printWelcome();
                run();
            }
        }

        public static void printWelcome()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Out.WriteLine("                                                                  ");
            Console.Out.WriteLine("                   CloudCoin Founders Edition                     ");
            Console.Out.WriteLine("                      Version: October.10.2017                    ");
            Console.Out.WriteLine("          Used to Authenticate, Store and Payout CloudCoins       ");
            Console.Out.WriteLine("      This Software is provided as is with all faults, defects    ");
            Console.Out.WriteLine("          and errors, and without warranty of any kind.           ");
            Console.Out.WriteLine("                Free from the CloudCoin Consortium.               ");
            Console.Out.WriteLine("                                                                  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Out.Write("  Checking RAIDA");
            echoRaida();
            //RAIDA_Status.showMs();
            //Check to see if suspect files need to be imported because they failed to finish last time. 
            //String[] suspectFileNames = new DirectoryInfo(suspectFolder).GetFiles().Select(o => o.Name).ToArray();//Get all files in suspect folder
            //if (suspectFileNames.Length > 0)
            //{
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.Out.WriteLine("  Finishing importing coins from last time...");//
            //    Console.ForegroundColor = ConsoleColor.White;

            //    //import();//temp stop while testing, change this in production
            //             //grade();

            //} //end if there are files in the suspect folder that need to be imported

        } // End print welcome
        public static void help()
        {
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Customer Service:");
            Console.Out.WriteLine("Open 9am to 11pm California Time(PST).");
            Console.Out.WriteLine("1 (530) 500 - 2646");
            Console.Out.WriteLine("CloudCoin.HelpDesk@gmail.com(unsecure)");
            Console.Out.WriteLine("CloudCoin.HelpDesk@Protonmail.com(secure if you get a free encrypted email account at ProtonMail.com)");

        }//End Help

        public async static Task echoRaida()
        {
            var echos = raida.GetEchoTasks();
            Console.Out.WriteLine("Starting Echo to RAIDA\n");
            Console.Out.WriteLine("----------------------------------\n");

            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            //MessageBox.Show("Finished Echo");
            Console.Out.WriteLine("Ready Count -" + raida.ReadyCount);
            Console.Out.WriteLine("Not Ready Count -" + raida.NotReadyCount);

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
               // Console.Out.WriteLine("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n");
                Debug.WriteLine("Node" + i + " Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Console.Out.WriteLine("-----------------------------------\n");

        }

        public static async Task detect()
        {
            Console.Out.WriteLine(FS.ImportFolder);
            updateLog("Starting Multi Detect..");
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();

            var predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
            FileSystem.predetectCoins = predetectCoins;

            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if (predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            ProgressChangedEventArgs pge = new ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for (int i = 0; i < LotCount; i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i * CloudCoinCore.Config.MultiDetectLoad).Take(200);
                raida.coins = coins;

                var tasks = raida.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        //coin.pown = "";
                        for (int k = 0; k < CloudCoinCore.Config.NodeCount; k++)
                        {
                            coin.response[k] = raida.nodes[k].multiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;


                        updateLog("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "." + coin.pown);
                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raida.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raida.OnProgressChanged(pge);
                    FS.WriteCoin(coins, FS.DetectedFolder);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }


            }
            pge.MinorProgress = 100;
            Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
            raida.OnProgressChanged(pge);
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);

            // Apply Sort to Folder to all detected coins at once.
            updateLog("Starting Sort.....");
            detectedCoins.ForEach(x => x.SortToFolder());
            updateLog("Ended Sort........");

            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder == FS.CounterfeitFolder
                               select x).ToList();
            var lostCoins = (from x in detectedCoins
                             where x.folder == FS.LostFolder
                             select x).ToList();
            var suspectCoins = (from x in detectedCoins
                                where x.folder == FS.SuspectFolder
                                select x).ToList();

            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            updateLog("Coin Detection finished.");
            updateLog("Total Passed Coins - " + passedCoins.Count() + "");
            updateLog("Total Failed Coins - " + failedCoins.Count() + "");
            updateLog("Total Lost Coins - " + lostCoins.Count() + "");
            updateLog("Total Suspect Coins - " + suspectCoins.Count() + "");

            // Move Coins to their respective folders after sort
            FS.MoveCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.WriteCoin(failedCoins, FS.CounterfeitFolder, true);
            FS.MoveCoins(lostCoins, FS.DetectedFolder, FS.LostFolder);
            FS.MoveCoins(suspectCoins, FS.DetectedFolder, FS.SuspectFolder);

            // Clean up Detected Folder
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);
            FS.RemoveCoins(lostCoins, FS.DetectedFolder);
            FS.RemoveCoins(suspectCoins, FS.DetectedFolder);

            FS.MoveImportedFiles();
            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

            Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            updateLog("Detection Completed in - " + ts.TotalMilliseconds / 1000);


            //App.Current.Dispatcher.Invoke(delegate
            //{
            //    enableUI();
            //    ShowCoins();
            //});

        }

        public static void updateLog(string logLine)
        {
            Console.Out.WriteLine(logLine);
        }
            /* STATIC METHODS */
            public async static void handleCommand(string[] args)
        {
            string command = args[0];

            switch (command)
            {
                case "echo":
                    await echoRaida();
                    break;
                case "showcoins":
                    //showCoins();
                    break;
                case "import":
                    //import();
                    break;
                case "export":
                    //export();
                    break;
                case "showfolders":
                    Process.Start(FS.RootPath);
                    //showFolders();
                    break;
                case "fix":
                    //fix(timeout);
                    break;
                case "dump":
                    //dump();
                    break;
                case "help":
                    help();
                    break;
                default:
                    break;
            }
        }
        public async static void run()
        {
            bool restart = false;
            while (!restart)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine("");
                //  Console.Out.WriteLine("========================================");
                Console.Out.WriteLine("");
                Console.Out.WriteLine("  Commands Available:");//"Commands Available:";
                Console.ForegroundColor = ConsoleColor.White;
                int commandCounter = 1;
                foreach (String command in commandsAvailable)
                {
                    Console.Out.WriteLine("  " + commandCounter + (". " + command));
                    commandCounter++;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.Write(prompt);
                Console.ForegroundColor = ConsoleColor.White;
                int commandRecieved = reader.readInt(1, 9);
                switch (commandRecieved)
                {
                    case 1:
                        await echoRaida();
                        break;
                    case 2:
                        //showCoins();
                        break;
                    case 3:
                        detect();
                        //import();
                        break;
                    case 4:
                       // export();
                        break;
                    case 5:
                        //fix(timeout);
                        break;
                    case 6:
                        //showFolders();
                        break;
                    case 7:
                        //dump();
                        break;
                    case 8:
                        Environment.Exit(0);
                        break;
                    case 9:
                        //testMind();
                        //partialImport();
                        break;
                    case 10:
                        //toMind();
                        break;
                    case 11:
                        //fromMind();
                        break;
                    default:
                        Console.Out.WriteLine("Command failed. Try again.");//"Command failed. Try again.";
                        break;
                }// end switch
            }// end while
        }// end run method
        public static void Setup()
        {
            // Create the Folder Structure
            FS.CreateFolderStructure();
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            //raida.Echo();
            FS.LoadFileSystem();
            var coins  =FS.LoadFolderCoins(FS.CounterfeitFolder);
            foreach(var coin in coins)
            {
                Console.WriteLine("Found Coin - " + coin.sn + " with denomination - "+ coin.denomination);
            }
            //Load Local Coins

            Console.Read();
        }
    }
}