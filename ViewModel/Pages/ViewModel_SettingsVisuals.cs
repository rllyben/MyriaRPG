using MyriaLib.Models.Settings;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_SettingsVisuals : BaseViewModel
    {
        private string darkMode;
        private string darkMidnightMode;

        [LocalizedKey("pg.settings.visuals.darkmode")]
        public string DarkMode 
        { 
            get { return darkMode; }
            set
            {
                darkMode = value;
                OnPropertyChanged(nameof(DarkMode));
            }

        }
        [LocalizedKey("pg.settings.visuals.darmodecheck")]
        public string DarkMidnightMode
        {
            get { return darkMidnightMode; }
            set
            {
                darkMidnightMode = value;
                OnPropertyChanged(nameof(DarkMidnightMode));
            }

        }
        
        private bool _darkModeSetter = Settings.Current.VisualSettings.DarkMode;
        public bool DarkModeSetter
        {
            get { return _darkModeSetter; }
            set
            {
                _darkModeSetter = value;
                SetDarkmode();
            }

        }
        private void SetDarkmode()
        {
            Settings.Current.VisualSettings.DarkMode = DarkModeSetter;
            SettingsService.Save();
            ThemeManager.Apply(Settings.Current.VisualSettings.DarkMode);
        }
        public ViewModel_SettingsVisuals()
        {
            LocalizationAutoWire.Wire(this);
        }

    }

}
