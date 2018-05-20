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
        Brush br = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFEEBC41"));

        public ActivityLogView()
        {
            InitializeComponent();
            activityLogs = System.AppDomain.CurrentDomain.BaseDirectory + "activities.log";
            string text;
            using (var streamReader = new StreamReader(@"activities.log", Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }
            //txtLogs.AppendText(text);

            string line = "";
            int counter = 0;
            System.IO.StreamReader file =
    new System.IO.StreamReader(@"activities.log");
            while ((line = file.ReadLine()) != null)
            {
                System.Console.WriteLine(line);
                counter++;
                var parts = line.Split(']');
                TextRange tr = new TextRange(txtLogs.Document.ContentEnd, txtLogs.Document.ContentEnd);
                tr.Text = "\n"+parts[0] + "]";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, br);

                TextRange tr1 = new TextRange(txtLogs.Document.ContentEnd, txtLogs.Document.ContentEnd);
                tr1.Text = parts[1];
                tr1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

            }

            file.Close();
            App.raida.LoggerHandler += Raida_LoggerHandler;

        }

        private void Raida_LoggerHandler(object sender, EventArgs e)
        {
            string DatetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            string timestamp =  DateTime.Now.ToString(DatetimeFormat);
            string line =  timestamp + " [INFO]";

            ProgressChangedEventArgs pge =(ProgressChangedEventArgs) e;
            line = timestamp + " [INFO]" + pge.MajorProgressMessage;

            var parts = line.Split(']');
            TextRange tr = new TextRange(txtLogs.Document.ContentEnd, txtLogs.Document.ContentEnd);
            tr.Text = "\n" + parts[0] + "]";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, br);

            TextRange tr1 = new TextRange(txtLogs.Document.ContentEnd, txtLogs.Document.ContentEnd);
            tr1.Text = parts[1];
            tr1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);


            //txtLogs.AppendText("\n"+pge.MajorProgressMessage);
        }

        private void txtLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLogs.ScrollToEnd();
            txtLogs.CaretPosition = txtLogs.CaretPosition.DocumentEnd;
        }
    }
}
