using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Celebrium_WPF.ViewModels
{
    public class ApplicationStartViewModel: BaseViewModel
    {
        LoginViewModel vmLogin;
        MainAppViewModel vmMain;
        int error_code = 0;
        public ApplicationStartViewModel()
        {

            vmLogin = new LoginViewModel();
            vmMain = new MainAppViewModel();
            //MessageBox.Show(Properties.Settings.Default.PIN);
            //MessageBox.Show(Properties.Settings.Default.Email);

            vmLogin.SubmitRequest += VmLogin_SubmitRequest;
           // vmLogin.Email = Properties.Settings.Default.Email;
          //  vmLogin.Password = Properties.Settings.Default.PIN;
           // vmLogin.Email = "navraj@outlook.com";
           // vmLogin.Password = "1234";
            ActiveView = vmMain;
        }

        private void VmLogin_SubmitRequest(object sender, EventArgs e)
        {
            if (LogIn(vmLogin.Email, vmLogin.Password))
            {
                ActiveView = vmMain;
            }
            else
            {
                if (error_code == Constants.ERROR_CODE_EMAIL_NULL)
                    MessageBox.Show("Email can not be null");
                if (error_code == Constants.ERROR_CODE_EMAIL_ZERO)
                    MessageBox.Show("Email can not be of 0 length.");
                if (error_code == Constants.ERROR_CODE_PIN_NULL)
                    MessageBox.Show("Pin can not be null");
                if (error_code == Constants.ERROR_CODE_PIN_ZERO)
                    MessageBox.Show("Pin can not be of 0 length");
                if (error_code == Constants.ERROR_CODE_NOT_EMAIL)
                    MessageBox.Show("Not a valid Email address.");
                if (error_code == Constants.ERROR_CODE_PIN_LENGTH)
                    MessageBox.Show("PIN can not be less than 4 characters.");
                if (error_code == Constants.ERROR_CODE_PIN_INCORRECT)
                    MessageBox.Show("PIN is incorrect");
                if (error_code == Constants.ERROR_CODE_EMAIL_INCORRECT)
                    MessageBox.Show("EMAIL is incorrect");

                //  System.Windows.Forms.MessageBox.Show("Invalid credentials");
            }
        }

        private bool LogIn(string email, string password)
        {
            // TODO add your validation logic here
            if (email == null)
            {
                error_code = Constants.ERROR_CODE_EMAIL_NULL;
                return false;
            }
            if (email.Length == 0)
            {
                error_code = Constants.ERROR_CODE_EMAIL_ZERO;
                return false;
            }
            if (password == null)
            {
                error_code = Constants.ERROR_CODE_PIN_NULL;
                return false;
            }
            if (password.Length == 0)
            {
                error_code = Constants.ERROR_CODE_PIN_ZERO;
                return false;
            }
            if(password.Length<4)
            {
                error_code = Constants.ERROR_CODE_PIN_LENGTH;
                return false;

            }
            if (!Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            {
                error_code = Constants.ERROR_CODE_NOT_EMAIL;
                return false;
            }
            if(Properties.Settings.Default.PIN.Length>0 && Properties.Settings.Default.PIN != password)
            {
                error_code = Constants.ERROR_CODE_PIN_INCORRECT;
                return false;
            }
            if (Properties.Settings.Default.Email.Length > 0 && Properties.Settings.Default.Email != email)
            {
                error_code = Constants.ERROR_CODE_EMAIL_INCORRECT;
                return false;
            }

            Properties.Settings.Default.PIN = password;
            Properties.Settings.Default.Email = email;
            Properties.Settings.Default.Save();
            return true;
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
