using MyriaLib.Models.Settings;
using MyriaLib.Services;
using MyriaRPG.Services;
using MyriaLib.Utils;
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
            TestingClass.TestSaveNpc();
            GameService.InitializeGame();
        }

    }

}
