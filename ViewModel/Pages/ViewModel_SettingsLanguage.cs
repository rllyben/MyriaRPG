using MyriaLib.Models.Settings;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel.Pages.Game;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_SettingsLanguage : BaseViewModel
    {
        private string _tblLanguage;
        private GameLanguage _selectedLanguage;
        [LocalizedKey("pg.settings.language")]
        public string TblLanguage 
        { 
            get { return _tblLanguage; }
            set
            {
                _tblLanguage = value;
                OnPropertyChanged(nameof(TblLanguage));
            }

        }
        public GameLanguage SelectedLanguage 
        {
            get { return _selectedLanguage; }
            set 
            { 
                _selectedLanguage = value;
                Settings.Current.LanguageSettings.Local = value;
                SettingsService.Save();
                Localization.Load(Settings.Current.LanguageSettings.Local);
                ViewModel_PageRoom.RefreshLocalisation();
            }

        }
        public List<GameLanguage> Languages { get; } = Enum.GetValues(typeof(GameLanguage))
                                               .Cast<GameLanguage>()
                                               .ToList();
        public ViewModel_SettingsLanguage() 
        {
            SelectedLanguage = Settings.Current.LanguageSettings.Local;
            LocalizationAutoWire.Wire(this);
        }

    }

}
