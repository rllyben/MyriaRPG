using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages.Settings
{
    /// <summary>
    /// Interaktionslogik für Page_SettingsLanguage.xaml
    /// </summary>
    public partial class Page_SettingsLanguage : Page
    {
        public Page_SettingsLanguage()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_SettingsLanguage();
        }

    }

}
