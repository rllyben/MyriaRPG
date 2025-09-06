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
using MyriaRPG.ViewModel;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages
{
    /// <summary>
    /// Interaktionslogik für Page_Login.xaml
    /// </summary>
    public partial class Page_Login : Page
    {
        private BaseViewModel _viewModel;
        public Page_Login(bool registration = false)
        {
            InitializeComponent();
            if (registration)
            {
                _viewModel = new ViewModel_RegisterPage();
                this.DataContext = new ViewModel_RegisterPage();
            }
            else
            {
                _viewModel = new ViewModel_LoginPage();
                this.DataContext = _viewModel;
            }
            pbx_UserPassword.PasswordChanged += PasswordToViewModel;
        }

        private void PasswordToViewModel(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = pbx_UserPassword.Password;
        }

    }

}
