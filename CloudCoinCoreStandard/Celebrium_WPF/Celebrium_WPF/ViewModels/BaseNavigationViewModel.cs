using Celebrium_WPF.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    public class BaseNavigationViewModel:BaseViewModel
    {
        public event EventHandler RequestBackNavigation;

        protected void OnRequestBackNavigation()
        {
            if (RequestBackNavigation != null)
            {
                RequestBackNavigation(this, EventArgs.Empty);
            }
        }

        private void mGoBack(object obj)
        {
            OnRequestBackNavigation();
        }

        public ICommand GoBack
        {
            get { return new ActionCommand(mGoBack); }
        }
    }
}
