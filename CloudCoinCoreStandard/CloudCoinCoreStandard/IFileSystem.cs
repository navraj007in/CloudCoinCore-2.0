using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CloudCoinCore
{
    public abstract class IFileSystem
    {
        public string RootPath { get; set; }
        public string ImportFolder { get; set; }
        public string ExportFolder { get; set; }
        public string BankFolder { get; set; }
        public string ImportedFolder { get; set; }
        public string LostFolder { get; set; }
        public string TrashFolder { get; set; }
        public string SuspectFolder { get; set; }
        public string DetectedFolder { get; set; }
        public string FrackedFolder { get; set; }
        public string TemplateFolder { get; set; }
        public string PartialFolder { get; set; }
        public string CounterfeitFolder { get; set; }
        public string LanguageFolder { get; set; }

        //public abstract IFileSystem(string path);

        public abstract bool CreateFolderStructure();

        public abstract void LoadFileSystem();




    }
}
