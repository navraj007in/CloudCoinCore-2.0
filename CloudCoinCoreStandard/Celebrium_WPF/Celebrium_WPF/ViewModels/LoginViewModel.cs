using Celebrium_WPF.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    public class LoginViewModel:BaseViewModel
    {
        public LoginViewModel()
        {

        }

        public event EventHandler SubmitRequest;

        protected void OnSubmitRequest()
        {
            if (SubmitRequest != null)
            {
                SubmitRequest(this, EventArgs.Empty);
            }
        }

        private void mSubmit(object obj)
        {
            OnSubmitRequest();
        }

        public ICommand Submit
        {
            get { return new ActionCommand(mSubmit); }
        }
    }
}
