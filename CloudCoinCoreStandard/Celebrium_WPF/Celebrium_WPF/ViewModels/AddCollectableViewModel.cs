using Celebrium_WPF.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    class AddCollectableViewModel : BaseNavigationViewModel
    {
        public AddCollectableViewModel()
        {
            Title1 = "ADD";
            Title2 = "MEMO";
            ShowFirst = System.Windows.Visibility.Visible;
            ShowLast = System.Windows.Visibility.Collapsed;
            ProgressValue = 0;
            ProgressStatus = "Not started";
        }

        private void mChooseFiles(object obj)
        {
            //TODO write code here
            System.Windows.Forms.MessageBox.Show("Write the choose files logic here and update the ProgressValue + ProgressStatus properties");

            //this below is just an example please adjust to your needs
            //update the values of these properties and the UI will update itself.
            ProgressValue += 10;
            ProgressStatus = ProgressValue.ToString() + "%";
        }

        public ICommand ChooseFiles
        {
            get { return new ActionCommand(mChooseFiles); }
        }



        private int _progressvalue;

        public int ProgressValue
        {
            get { return _progressvalue; }
            set
            {
                _progressvalue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        private string _progressStatus;

        public string ProgressStatus
        {
            get { return _progressStatus.ToUpper(); }
            set
            {
                _progressStatus = value;
                OnPropertyChanged(nameof(ProgressStatus));
            }
        }

    }
}
