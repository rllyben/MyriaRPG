using MyriaLib.Models.Settings;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Windows;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_SettingsVisuals : BaseViewModel
    {
        private string darkMode;
        private string darkMidnightMode;
        private string fullScreen;
        private string fullScreenCheck;

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

        [LocalizedKey("pg.settings.visuals.fullscreen")]
        public string FullScreen
        {
            get { return fullScreen; }
            set
            {
                fullScreen = value;
                OnPropertyChanged(nameof(FullScreen));
            }
        }

        [LocalizedKey("pg.settings.visuals.fullscreencheck")]
        public string FullScreenCheck
        {
            get { return fullScreenCheck; }
            set
            {
                fullScreenCheck = value;
                OnPropertyChanged(nameof(FullScreenCheck));
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

        private bool _fullScreenSetter = Settings.Current.VisualSettings.FullScreen;
        public bool FullScreenSetter
        {
            get { return _fullScreenSetter; }
            set
            {
                _fullScreenSetter = value;
                SetFullScreen();
            }
        }
        private void SetFullScreen()
        {
            Settings.Current.VisualSettings.FullScreen = FullScreenSetter;
            SettingsService.Save();
            MainWindow.Instance.ApplyWindowMode(FullScreenSetter);
        }

        public ViewModel_SettingsVisuals()
        {
            LocalizationAutoWire.Wire(this);
        }

    }

}
