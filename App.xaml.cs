using System.Configuration;
using System.Data;
using System.Windows;
using MyriaLib.Services;
using MyriaLib.Systems.Enums;

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
            MyriaLib.Systems.Localization.Load(SettingsService.Current.Language);
        }

    }

}
