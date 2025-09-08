using MyriaLib.Models.Settings;
using MyriaLib.Services;
using MyriaLib.Systems.Enums;
using MyriaRPG.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MyriaRPG
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SettingsService.Load();
            MyriaLib.Systems.Localization.Load(Settings.Current.LanguageSettings.Local);
            ThemeManager.Apply(Settings.Current.VisualSettings.DarkMode);
        }

    }

}
