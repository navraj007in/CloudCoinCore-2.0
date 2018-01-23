using System;
using CloudCoinCore;
using CloudCoinClient.CoreClasses;

namespace CloudCoinConsole
{
    class Program
    {
        static FileSystem FS = new FileSystem();
        static RAIDA raida;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Setup();
        }

        public static void Setup()
        {
            // Create the Folder Structure
            FS.CreateFolderStructure();
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            //raida.Echo();
            FS.LoadFileSystem();
            var coins  =FS.LoadFolderCoins(FS.RootPath + FS.CounterfeitFolder);
            foreach(var coin in coins)
            {
                Console.WriteLine("Found Coin - " + coin.sn + " with denomination - "+ coin.denomination);
            }
            //Load Local Coins

            Console.Read();
        }
    }
}