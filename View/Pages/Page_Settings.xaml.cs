using System.Windows.Controls;
using MyriaRPG.Services;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages
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
            Navigation.SetNavigationFrame(frm_NavigationFrame, 2);
        }
    }
}
