using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celebrium_WPF.ViewModels
{
    public class ApplicationStartViewModel: BaseViewModel
    {
        LoginViewModel vmLogin;
        MainAppViewModel vmMain;

        public ApplicationStartViewModel()
        {

            vmLogin = new ViewModels.LoginViewModel();
            vmMain = new ViewModels.MainAppViewModel();

            vmLogin.SubmitRequest += VmLogin_SubmitRequest;

            ActiveView = vmLogin;
        }

        private void VmLogin_SubmitRequest(object sender, EventArgs e)
        {
            //TODO put some validation here
            ActiveView = vmMain;
        }

        private BaseViewModel _activeView;
        public BaseViewModel ActiveView
        {
            get
            {
                return _activeView;
            }
            set
            {
                _activeView = value;

                OnPropertyChanged(nameof(ActiveView));
            }
        }
    }
}
