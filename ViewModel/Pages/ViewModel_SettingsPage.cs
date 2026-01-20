using System.Windows.Input;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Settings;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_SettingsPage : BaseViewModel
    {
        private string _title;
        private string _btnLanguage;
        private string _btnVisuals;

        [LocalizedKey("pg.settings.title")]
        public string Title 
        { 
            get { return _title; }
            private set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }

        }

        [LocalizedKey("pg.settings.language")]
        public string btnLanugage 
        { 
            get { return _btnLanguage; } 
            private set
            {
                _btnLanguage = value;
                OnPropertyChanged(nameof(btnLanugage));
            }

        }

        [LocalizedKey("pg.settings.visuals")]
        public string btnVisuals 
        { 
            get { return _btnVisuals; }
            private set
            {
                _btnVisuals = value;
                OnPropertyChanged(nameof(btnVisuals));
            }

        }
        public ICommand Language { get; }
        public ICommand Visuals { get; }
        public ViewModel_SettingsPage()
        {
            Language = new RelayCommand(LanguageAction);
            Visuals = new RelayCommand(VisalsAction);
            LocalizationAutoWire.Wire(this);
        }
        private void VisalsAction()
        {
            Navigation.NavigateSettings(new Page_SettingsVisuals());
        }
        private void LanguageAction()
        {
            Navigation.NavigateSettings(new Page_SettingsLanguage());
        }

    }

}
