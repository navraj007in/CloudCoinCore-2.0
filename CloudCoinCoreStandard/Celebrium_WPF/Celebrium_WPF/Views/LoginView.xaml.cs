using Celebrium_WPF.ViewModels;
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

namespace Celebrium_WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(txtBoxFakePass, -1);
            Panel.SetZIndex(passBox, 1);
            passBox.Focus();

        }

        private void passBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passBox.Password.Length == 0)
            {
                Panel.SetZIndex(passBox, -1);
                Panel.SetZIndex(txtBoxFakePass, 1);
            }
            else
            {
                LoginViewModel vm = this.DataContext as LoginViewModel;
                vm.Password = passBox.Password;
            }
        }
    }
}
