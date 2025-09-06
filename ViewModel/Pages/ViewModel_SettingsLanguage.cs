using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_SettingsLanguage : BaseViewModel
    {
        private GameLanguage _selectedLanguage;
        public string TblLanguage { get; set; } = (Localization.T("pg.settings.language") + ":");
        public GameLanguage SelectedLanguage 
        {
            get { return _selectedLanguage; }
            set 
            { 
                _selectedLanguage = value;
                SettingsService.Current.Language = value;
                SettingsService.Save();
                Localization.Load(SettingsService.Current.Language);
                Localization.LanguageChanged += OnLanguageChanged;
            }

        }
        private void OnLanguageChanged(object? s, EventArgs e)
        {
            TblLanguage = (Localization.T("pg.settings.language") + ":");
        }
        public List<GameLanguage> Languages { get; } = Enum.GetValues(typeof(GameLanguage))
                                               .Cast<GameLanguage>()
                                               .ToList();
        public ViewModel_SettingsLanguage() 
        {
            SelectedLanguage = SettingsService.Current.Language;
        }

    }

}
