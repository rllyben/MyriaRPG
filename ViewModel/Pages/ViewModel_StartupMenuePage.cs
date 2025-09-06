using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyriaLib.Systems;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_StartupMenuePage
    {
        public string btnSingle { get; set; } = Localization.T("pg.start.btn.single");
        public string btnLogin { get; set; } = Localization.T("app.accounting.UI.login");
        public string btnRegister { get; set; } = Localization.T("app.accounting.UI.register");
        public string btnSettings { get; set; } = Localization.T("app.general.UI.settings");
        public ICommand SinglePlayer {  get; }
        public ICommand Login { get; }
        public ICommand Register { get; }
        public ICommand Settings { get; }

        public ViewModel_StartupMenuePage()
        {
            SinglePlayer = new RelayCommand(SinglePlayerAction);
            Login = new RelayCommand(LoginAction);
            Register = new RelayCommand(RegisterAction);
            Settings = new RelayCommand(SettingsAction);
        }

        private void SinglePlayerAction()
        {
            Navigation.NavigateMain(new Page_CharacterSelection());
        }
        private void LoginAction()
        {
            Navigation.NavigateStartup(new Page_Login());
        }
        private void RegisterAction()
        {
            Navigation.NavigateStartup(new Page_Login(true));
        }
        private void SettingsAction()
        {
            Navigation.NavigateStartup(new Page_Settings());
        }

    }

}
