using MyriaRPG.Services;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game.IngameWindow
{
    /// <summary>
    /// Interaktionslogik für Page_Settings.xaml
    /// </summary>
    public partial class Page_Settings : Page
    {
        public Page_Settings()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_SettingsPage();
            Navigation.SetNavigationFrame(frm_NavigationFrame, 5);
        }

    }

}
