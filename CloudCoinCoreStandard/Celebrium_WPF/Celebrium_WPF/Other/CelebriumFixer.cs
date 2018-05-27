using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCoinCore;

namespace Celebrium_WPF.Other
{
    public class CelebriumFixer : Frack_Fixer 
    {
        public CelebriumFixer(IFileSystem fileUtils, int timeout) :base(fileUtils,timeout)
        {

        }

        private void JpegWrite(String Path, CloudCoin cc, String memoPath, String BankFileName, String FrackedFileName, string imagePath)
        {

            IFileSystem fileSystem = MainWindow.FS;

            if (File.Exists(Path))//If the file is a bank file, export a good bank coin
            {
                CloudCoin jpgCoin = fileSystem.LoadCoin(Path);
                if (fileSystem.writeJpeg(jpgCoin, "", imagePath, memoPath))//If the jpeg writes successfully 
                {
                    //File.Delete(Path);//Delete the files if they have been written to
                    File.Delete(imagePath);
                }//end if write was good. 

            }
            else//Export a fracked coin. 
            {
                CloudCoin jpgCoin = fileSystem.LoadCoin(FrackedFileName);
                //if (fileSystem.writeJpeg(jpgCoin, ""))
                {
                    //File.Delete(FrackedFileName);//Delete the files if they have been written to
                }//end if
            }//end else

        }//End write one jpeg 

        /* PUBLIC METHODS */
        public new int[] FixAll()
        {
            IsFixing = true;
            continueExecution = true;
            int[] results = new int[3];
            String[] frackedFileNames = new System.IO.DirectoryInfo(this.fileUtils.FrackedFolder).GetFiles().Select(o => o.Name).ToArray();


            CloudCoin frackedCC;

            ProgressChangedEventArgs pge = new ProgressChangedEventArgs();
            pge.MajorProgressMessage = "Starting Frack Fixing";
            raida.OnLogRecieved(pge);

            //CoinUtils cu = new CoinUtils(frackedCC);
            if (frackedFileNames.Length < 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine("You have no fracked coins.");
                //CoreLogger.Log("You have no fracked coins.");
                Console.ForegroundColor = ConsoleColor.White;
            }//no coins to unfrack



            for (int i = 0; i < frackedFileNames.Length; i++)
            {
                string frackedFileName = frackedFileNames[i];

                if (!continueExecution)
                {
                    Debug.WriteLine("Aborting Fix 1");
                    break;
                }
                Console.WriteLine("Unfracking coin " + (i + 1) + " of " + frackedFileNames.Length);
                //ProgressChangedEventArgs pge = new ProgressChangedEventArgs();
                pge.MajorProgressMessage = "Unfracking coin " + (i + 1) + " of " + frackedFileNames.Length;
                raida.OnLogRecieved(pge);
                //CoreLogger.Log("UnFracking coin " + (i + 1) + " of " + frackedFileNames.Length);
                try
                {
                    frackedCC = fileUtils.LoadCoin(this.fileUtils.FrackedFolder + frackedFileNames[i]);
                    if (frackedCC == null)
                        throw new IOException();
                    CoinUtils cu = new CoinUtils(frackedCC);
                    String value = frackedCC.pown;
                    //  Console.WriteLine("Fracked Coin: ");
                    cu.consoleReport();

                    CoinUtils fixedCC = fixCoin(frackedCC); // Will attempt to unfrack the coin. 
                    if (!continueExecution)
                    {
                        Debug.WriteLine("Aborting Fix 2");
                        break;
                    }
                    cu.consoleReport();
                    switch (fixedCC.getFolder().ToLower())
                    {
                        case "bank":
                            this.totalValueToBank++;
                            //this.fileUtils.overWrite(this.fileUtils.BankFolder, fixedCC.cc);
                            Console.WriteLine("CloudCoin was moved to Bank.");
                            pge.MajorProgressMessage = "CloudCoin was moved to Bank.";
                            raida.OnLogRecieved(pge);
                            fileUtils.writeJpeg(fixedCC.cc, "", fileUtils.FrackedFolder + frackedFileNames[i], fileUtils.BankFolder + frackedFileName);
                            this.deleteCoin(this.fileUtils.FrackedFolder + frackedFileNames[i]);

                            //CoreLogger.Log("CloudCoin was moved to Bank.");
                            break;
                        case "counterfeit":
                            this.totalValueToCounterfeit++;
                            //this.fileUtils.overWrite(this.fileUtils.CounterfeitFolder, fixedCC.cc);
                            Console.WriteLine("CloudCoin was moved to Trash.");
                            pge.MajorProgressMessage = "CloudCoin was moved to Trash.";
                            raida.OnLogRecieved(pge);
                            fileUtils.writeJpeg(fixedCC.cc, "", fileUtils.FrackedFolder + frackedFileNames[i], fileUtils.CounterfeitFolder + frackedFileName);
                            this.deleteCoin(this.fileUtils.FrackedFolder + frackedFileNames[i]);

                            //CoreLogger.Log("CloudCoin was moved to Trash.");
                            break;
                        default://Move back to fracked folder
                            this.totalValueToFractured++;
                            //this.fileUtils.overWrite(this.fileUtils.FrackedFolder, fixedCC.cc);
                            Console.WriteLine("CloudCoin was moved back to Fracked folder.");
                            pge.MajorProgressMessage = "CloudCoin was moved back to Fracked folder.";
                            raida.OnLogRecieved(pge);
                            fileUtils.writeJpeg(fixedCC.cc, "", fileUtils.FrackedFolder + frackedFileNames[i], fileUtils.FrackedFolder+ frackedFileName);
                            this.deleteCoin(this.fileUtils.FrackedFolder + frackedFileNames[i]);

                            //CoreLogger.Log("CloudCoin was moved back to Fraked folder.");
                            break;
                    }
                    // end switch on the place the coin will go 
                    Console.WriteLine("...................................");
                    pge.MajorProgressMessage = "...................................";
                    raida.OnLogRecieved(pge);

                    Console.WriteLine("");
                }
                catch (FileNotFoundException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    //CoreLogger.Log(ex.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (IOException ioex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ioex);
                    //CoreLogger.Log(ioex.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                } // end try catch
            }// end for each file name that is fracked

            results[0] = this.totalValueToBank;
            results[1] = this.totalValueToCounterfeit; // System.out.println("Counterfeit and Moved to trash: "+totalValueToCounterfeit);
            results[2] = this.totalValueToFractured; // System.out.println("Fracked and Moved to Fracked: "+ totalValueToFractured);
            IsFixing = false;
            continueExecution = true;
            pge.MajorProgressMessage = "Finished Frack Fixing.";
            raida.OnLogRecieved(pge);

            pge.MajorProgressMessage = "Fixed " + totalValueToBank + " CloudCoins and moved them into Bank Folder";
            if (totalValueToBank > 0)
                raida.OnLogRecieved(pge);

            return results;
        }// end fix all

    }
}
