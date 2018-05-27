using Celebrium;
using Celebrium_WPF.ViewModels;
using CloudCoinClient.CoreClasses;
using CloudCoinCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Celebrium_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CoreVariables
        RAIDA raidaCore = App.raida;
        public static FileSystem FS;
        //string RootPath;
        //FileSystem FS;
        //RAIDA raida;
        int onesCount = 0;
        int fivesCount = 0;
        int qtrCount = 0;
        int hundredsCount = 0;
        int twoFiftiesCount = 0;

        public static int exportOnes = 0;
        public static int exportFives = 0;
        public static int exportTens = 0;
        public static int exportQtrs = 0;
        public static int exportHundreds = 0;
        public static int exportTwoFifties = 0;
        public static int exportJpegStack = 2;
        public static string exportTag = "";
        public static SimpleLogger logger = new SimpleLogger();
        public static SimpleLogger activityLogger = new SimpleLogger( "activities.log",true);

        Other.CelebriumFixer fixer;

        #endregion
        public static String RootFolder = AppDomain.CurrentDomain.BaseDirectory;
        public MainWindow()
        {
            InitializeComponent();
            ApplicationStartViewModel mainVm = new ApplicationStartViewModel();
            this.DataContext = mainVm;
            SetupFolders();
            logger = new SimpleLogger(FS.LogsFolder + "logs" + DateTime.Now.ToString("yyyyMMdd").ToLower() + ".log", true);
            //raidaCore.LoggerHandler += Raida_LogRecieved;
            fixer = new Other.CelebriumFixer(FS, Config.milliSecondsToTimeOut);

            Task.Run(() => {
                Fix();
            });

        }

        

        public event EventHandler<EventArgs> ThresholdReached;

        private void Fix()
        {
            fixer.continueExecution = true;
            fixer.IsFixing = true;
            fixer.FixAll();
            fixer.IsFixing = false;
        }

        public static void printLineDots()
        {
            updateLog("********************************************************************************");
        }

        public static void updateLog(string logLine, bool writeUI = true)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                //if (writeUI)
                  //  txtLogs.AppendText(logLine + Environment.NewLine);
                logger.Info(logLine);
            });

        }

        public static void updateActivityLog(string logLine, bool writeUI = true)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                //if (writeUI)
                //  txtLogs.AppendText(logLine + Environment.NewLine);
                activityLogger.Info(logLine);
                ProgressChangedEventArgs pge = new ProgressChangedEventArgs();
                pge.MajorProgressMessage = logLine;
                App.raida.OnLogRecieved(pge);
            });

        }

       


        private void Raida_LogRecieved(object sender, ProgressChangedEventArgs e)
        {
            EventHandler<ProgressChangedEventArgs> handler = Raida_LogRecieved;
            if (handler != null)
            {
                handler(this, e);
            }
            //throw new NotImplementedException();
        }
        public string getWorkspace()
        {
            string workspace = "";
            if (Properties.Settings.Default.WorkSpace != null && Properties.Settings.Default.WorkSpace.Length > 0)
                workspace = Properties.Settings.Default.WorkSpace;
            else
                workspace = AppDomain.CurrentDomain.BaseDirectory;
            Properties.Settings.Default.WorkSpace = workspace;
            return workspace;
        }

        public void SetupFolders()
        {
            RootFolder = App.rootFolder;
            FS = new FileSystem(RootFolder);

            // Create the Folder Structure
            FS.CreateFolderStructure();
            FS.CopyTemplates();
            // Populate RAIDA Nodes
            raidaCore = CloudCoinCore.RAIDA.GetInstance();
            raidaCore.FS = FS;
            //CoinDetected += Raida_CoinDetected;
            //raida.Echo();

            FS.LoadFileSystem();

            //fileUtils = FileUtils.GetInstance(MainWindow.rootFolder);


        }
    }
}
