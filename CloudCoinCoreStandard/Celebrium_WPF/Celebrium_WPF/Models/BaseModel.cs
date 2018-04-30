using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celebrium_WPF.Models
{
    public class BaseModel :  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region INotifyPropertyChanged members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
