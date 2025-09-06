using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyriaLib.Services.Manager;
using MyriaLib.Systems;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_RegisterPage : BaseViewModel
    {
        public ICommand Login { get; }
        public ICommand Cancel { get; }
        internal string Password { get; set; }
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
            }

        }
        public string tblUserNameMsg { get; set; }
        public string tblPasswordMsg { get; set; }
        public string tblUsername { get; set; } = Localization.T("app.accounting.UI.username");
        public string tblPassword { get; set; } = Localization.T("app.accounting.UI.password");
        public string Title { get; set; } = Localization.T("pg.register.tbl.title");
        public string btnLogin { get; set; } = Localization.T("app.accounting.UI.register");
        public string btnCancel { get; set; } = Localization.T("app.general.UI.cancel");

        public ViewModel_RegisterPage()
        {
            Login = new RelayCommand(LoginAction);
            Cancel = new RelayCommand(CancelAction);
        }

        private void LoginAction()
        {
            if (LoginManager.Login(Username, Password))
            {
                Navigation.NavigateMain(new Page_CharacterSelection());
            }
            else
                tblUserNameMsg = Localization.T("pg.login.user.nonexistent");
        }
        private void CancelAction()
        {
            Navigation.NavigateStartup(null);
        }

    }

}
