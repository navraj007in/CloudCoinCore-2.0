using System;
using System.Collections.Generic;
using System.Text;

namespace CloudCoinCore
{
    public class Config
    {
        public static string TAG_IMPORT = "Import";
        public static string TAG_EXPORT = "Export";
        public static string TAG_BANK = "Bank";
        public static string TAG_LOST = "Lost";
        public static string TAG_IMPORTED = "Imported";
        public static string TAG_FRACKED = "Fracked";
        public static string TAG_TEMPLATES = "Templates";
        public static string TAG_COUNTERFEIT = "Counterfeit";
        public static string TAG_DETECTED = "Detected";
        public static string TAG_LANGUAGE = "Language";
        public static string TAG_PARTIAL = "Partial";
        public static string TAG_TRASH = "Trash";
        public static string TAG_SUSPECT = "Suspect";
        public static string TAG_PREDETECT = "Predetect";
        
        public static string TAG_REQUESTS = "Requests";
        public const int YEARSTILEXPIRE = 2;
        public static int milliSecondsToTimeOut = 10000;
        public static int MultiDetectLoad = 200;
        public static int NodeCount = 25;
        public static int PassCount = 16;
        public static int NetworkNumber = 1;

        public enum Folder { Suspect, Counterfeit, Fracked, Bank, Trash };

        public static string[] allowedExtensions = new[] { ".stack", ".jpeg", ".chest", ".bank",".jpg" };


    }
}
