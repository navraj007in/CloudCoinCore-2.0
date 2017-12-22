using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCoinCore;

namespace CloudCoinClient.CoreClasses
{
    public class FileSystem : IFileSystem
    {

        public override bool CreateFolderStructure()
        {
            if(Properties.Settings.Default.WorkSpace == "")
            {
                RootPath = AppDomain.CurrentDomain.BaseDirectory;
            }   
            else
            {
                RootPath = Properties.Settings.Default.WorkSpace;
            }

            return true;
        }

        public override void LoadFileSystem()
        {
            
        }
    }
}
