using CloudCoinCore;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Celebrium_WPF.Views
{
    /// <summary>
    /// Interaction logic for ActivityLogView.xaml
    /// </summary>
    public partial class ActivityLogView : UserControl
    {
        private string activityLogs;

        public ActivityLogView()
        {
            InitializeComponent();
            activityLogs = System.AppDomain.CurrentDomain.BaseDirectory + "activities.log";
            string text;
            using (var streamReader = new StreamReader(@"activities.log", Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }
            txtLogs.AppendText(text);

            App.raida.LoggerHandler += Raida_LoggerHandler;

        }

        private void Raida_LoggerHandler(object sender, EventArgs e)
        {
            ProgressChangedEventArgs pge =(ProgressChangedEventArgs) e;
            txtLogs.AppendText(pge.MajorProgressMessage);
        }

        private void txtLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLogs.ScrollToEnd();
        }
    }
}
