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

namespace CloudCoinCE.UserControls
{
    /// <summary>
    /// Interaction logic for UpDownControl.xaml
    /// </summary>
    public partial class UpDownControl : UserControl
    {
        public UpDownControl()
        {
            InitializeComponent();
        }

        public delegate void ExportChanged(object sender, EventArgs e);
        public event ExportChanged OnExportChanged;

        public delegate void StatusUpdateHandler(object sender, EventArgs e);
        public event StatusUpdateHandler OnUpdateStatus;

        private void UpdateStatus(string status, int percentage = 0)
        {
            // Make sure someone is listening to event
            if (OnUpdateStatus == null) return;

            EventArgs args = new EventArgs();
            OnUpdateStatus(this, args);
        }

        public int pMax = 5;
        public int Max
        {
            get { return pMax; }
            set { pMax = value; }
        }

        public int pMin = 0;
        public int Min
        {
            get { return pMin; }
            set { pMin = value; }
        }
        public int val = 0;
        public int Value
        {
            get { return val; }
            set { val = value; }
        }
        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (val > pMin)
            {
                lblValue.Content = Convert.ToInt16(lblValue.Content) - 1;
                val--;
            }
            OnThresholdReached(EventArgs.Empty);

        }

        protected virtual void OnThresholdReached(EventArgs e)
        {
            EventHandler handler = ThresholdReached;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler ThresholdReached;

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (val < pMax)
            {
                lblValue.Content = Convert.ToInt16(lblValue.Content) + 1;
                val++;
                OnThresholdReached(EventArgs.Empty);
            }

        }
    }
}
