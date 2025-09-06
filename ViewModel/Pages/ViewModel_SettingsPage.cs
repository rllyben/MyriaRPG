using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyriaLib.Systems;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Settings;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_SettingsPage
    {
        public string Title { get; set; } = Localization.T("pg.settings.title");
        public string btnLanugage { get; set; } = Localization.T("pg.settings.language");
        public ICommand Language { get; }
        public ViewModel_SettingsPage()
        {
            Language = new RelayCommand(LanguageAction);
        }
        private void LanguageAction()
        {
            Navigation.NavigateSettings(new Page_SettingsLanguage());
        }

    }

}
