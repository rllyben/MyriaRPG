using MyriaLib.Models;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;
using System.Text.Json;
using System.Windows.Input;
using System.IO;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_StartupMenuePage : BaseViewModel
    {
        private string _btnSingle;
        private string _btnLogin;
        private string _btnRegister;
        private string _btnSettings;
        [LocalizedKey("pg.start.btn.single")]
        public string btnSingle 
        { 
            get { return _btnSingle; }
            private set
            {
                _btnSingle = value;
                OnPropertyChanged(nameof(btnSingle));
            }

        }

        [LocalizedKey("app.accounting.UI.login")]
        public string btnLogin 
        { 
            get { return _btnLogin; }
            private set
            {
                _btnLogin = value;
                OnPropertyChanged(nameof(btnLogin));
            }

        }

        [LocalizedKey("app.accounting.UI.register")]
        public string btnRegister
        {
            get { return _btnRegister; }
            private set
            {
                _btnRegister = value;
                OnPropertyChanged(nameof(btnRegister));
            }

        }

        [LocalizedKey("app.general.UI.settings")]
        public string btnSettings 
        { 
            get { return _btnSettings; }
            private set
            {
                _btnSettings = value;
                OnPropertyChanged(nameof(btnSettings));
            }

        }

        public ICommand SinglePlayer {  get; }
        public ICommand Login { get; }
        public ICommand Register { get; }
        public ICommand Settings { get; }

        public ViewModel_StartupMenuePage()
        {
            LocalizationAutoWire.Wire(this);

            SinglePlayer = new RelayCommand(SinglePlayerAction);
            Login = new RelayCommand(LoginAction);
            Register = new RelayCommand(RegisterAction);
            Settings = new RelayCommand(SettingsAction);
        }
        private void SinglePlayerAction()
        {

            string path = Path.Combine("Data/users", $"localUser.json");

            if (!File.Exists(path))
            {

                if (!Path.Exists(path))
                    Directory.CreateDirectory("Data/users");

                UserAccount account = new UserAccount();
                account.Username = "localUser";


                var jsons = JsonSerializer.Serialize(account, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, jsons);
            }

            var json = File.ReadAllText(path);
            UserAccoundService.CurrentUser = JsonSerializer.Deserialize<UserAccount>(json);

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
